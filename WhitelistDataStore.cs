using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace MainPage
{
   public static class WhitelistDataStore
   {
      // UI binding list (keep this for your MAUI pages)
      public static ObservableCollection<string> Whitelist { get; } = new ObservableCollection<string>();

      // Internal fast lookup (digits-only)
      private static readonly HashSet<string> _normalizedWhitelist = new HashSet<string>(StringComparer.Ordinal);

      // Thread safety + init guard
      private static readonly object _gate = new object();
      private static bool _loaded;
      private static bool _suppressPersist;

      private static bool _bulkUpdating;


      // Persistent storage (shared between UI + CallScreeningService)
      private static readonly string StorePath =
         Path.Combine(FileSystem.AppDataDirectory, "whitelist.json");

      // Your existing defaults (from your original file)
      private static readonly string[] DefaultWhitelist =
      {
        "210-452-2031", // luvs
        "210-774-1437", //baby girl
        "979-344-8888", //baby boy
        "408-838-6160", // bro
        "916-947-4683", // ethan
        "209-875-2885", // shane
        "210-966-2250", // mom

        "979-764-5575", // A&M Middle School Front Desk
        "979-764-4264", // A&M Middle School Nurse
        "979-764-4207", // A&M Middle School Registrar
        "979-764-4279", // A&M Middle School Library
        "979-764-4211", // A&M Middle School Counselor

        "979-764-5400", // CSISD
        "979-764-5530", // CSISD

        "210-488-4754", // Alex Garner
        "210-863-2912", // Greer Beacker

        "979-458-5437", // Becky Gates Children’s Center
        "979-458-6836" // Charlotte Sharp Children’s Center 
      };

      static WhitelistDataStore()
      {
         Whitelist.CollectionChanged += (_, __) =>
         {
            // Ignore changes during initial load / bulk updates
            if (!_loaded || _bulkUpdating) return;

            lock (_gate)
            {
               _normalizedWhitelist.Clear();
               foreach (var entry in Whitelist)
               {
                  var normalized = NormalizeDigits(entry);
                  if (!string.IsNullOrEmpty(normalized))
                     _normalizedWhitelist.Add(normalized);
               }

               Persist_NoLock();
            }
         };

      }

      /// <summary>
      /// Optional: call this early (App startup) to avoid first-call load during screening.
      /// Safe to call multiple times.
      /// </summary>
      public static void Initialize()
      {
         EnsureLoaded();
      }

      public static bool IsNumberWhitelisted(string number)
      {
         EnsureLoaded();

         var normalized = NormalizeDigits(number);
         if (string.IsNullOrEmpty(normalized))
            return false;

         lock (_gate)
         {
            return _normalizedWhitelist.Contains(normalized);
         }
      }

      public static bool AddNumber(string number)
      {
         EnsureLoaded();

         if (string.IsNullOrWhiteSpace(number))
            return false;

         var normalized = NormalizeDigits(number);
         if (string.IsNullOrEmpty(normalized))
            return false;

         lock (_gate)
         {
            if (_normalizedWhitelist.Contains(normalized))
               return false;

            _normalizedWhitelist.Add(normalized);

            // Update UI list on main thread
            if (MainThread.IsMainThread)
               Whitelist.Add(number);
            else
               MainThread.BeginInvokeOnMainThread(() => Whitelist.Add(number));

            Persist_NoLock();
            return true;
         }
      }

      public static bool RemoveNumber(string number)
      {
         EnsureLoaded();

         if (string.IsNullOrWhiteSpace(number))
            return false;

         var normalized = NormalizeDigits(number);
         if (string.IsNullOrEmpty(normalized))
            return false;

         lock (_gate)
         {
            var removed = _normalizedWhitelist.Remove(normalized);

            // Remove matching entries from UI list (by normalized digits)
            void RemoveFromUi()
            {
               for (int i = Whitelist.Count - 1; i >= 0; i--)
               {
                  if (NormalizeDigits(Whitelist[i]) == normalized)
                     Whitelist.RemoveAt(i);
               }
            }

            if (MainThread.IsMainThread) RemoveFromUi();
            else MainThread.BeginInvokeOnMainThread(RemoveFromUi);

            if (removed)
               Persist_NoLock();

            return removed;
         }
      }

      // -----------------------
      // Internals
      // -----------------------

      private static void EnsureLoaded()
      {
         lock (_gate)
         {
            if (_loaded) return;

            _bulkUpdating = true;
            _suppressPersist = true;
            try
            {
               var list = LoadFromDisk_NoLock();
               ReplaceAll_NoLock(list);
            }
            finally
            {
               _suppressPersist = false;
               _bulkUpdating = false;
            }

            _loaded = true;
         }

      }

      private static List<string> LoadFromDisk_NoLock()
      {
         try
         {
            if (File.Exists(StorePath))
            {
               var json = File.ReadAllText(StorePath);
               var list = JsonSerializer.Deserialize<List<string>>(json);
               if (list != null && list.Count > 0)
                  return list;
            }
         }
         catch
         {
            // If file is corrupt/unreadable, fall back to defaults.
         }

         return DefaultWhitelist.ToList();
      }

      private static void Persist_NoLock()
      {
         if (_suppressPersist) return;

         try
         {
            // Snapshot current UI list for persistence
            var list = Whitelist.ToList();
            var json = JsonSerializer.Serialize(list);
            File.WriteAllText(StorePath, json);
         }
         catch
         {
            // Swallow persistence errors; blocking calls should never crash over this.
         }
      }

      private static void ReplaceAll_NoLock(IEnumerable<string> numbers)
      {
         Whitelist.Clear();
         _normalizedWhitelist.Clear();

         foreach (var n in numbers.Where(s => !string.IsNullOrWhiteSpace(s)))
         {
            Whitelist.Add(n);

            var normalized = NormalizeDigits(n);
            if (!string.IsNullOrEmpty(normalized))
               _normalizedWhitelist.Add(normalized);
         }

         Persist_NoLock();
      }

      private static void RebuildNormalizedFromWhitelistAndPersist()
      {
         EnsureLoaded();

         lock (_gate)
         {
            _normalizedWhitelist.Clear();
            foreach (var entry in Whitelist)
            {
               var normalized = NormalizeDigits(entry);
               if (!string.IsNullOrEmpty(normalized))
                  _normalizedWhitelist.Add(normalized);
            }

            Persist_NoLock();
         }
      }

      private static string NormalizeDigits(string? input)
      {
         if (string.IsNullOrEmpty(input))
            return string.Empty;

         // Fast, allocation-minimized normalization (digits-only)
         Span<char> buffer = stackalloc char[input.Length];
         int count = 0;

         for (int i = 0; i < input.Length; i++)
         {
            char c = input[i];
            if (c >= '0' && c <= '9')
               buffer[count++] = c;
         }

         return count == 0 ? string.Empty : new string(buffer.Slice(0, count));
      }
   }
}
