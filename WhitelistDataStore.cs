using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainPage
{
   public static class WhitelistDataStore
   {

      public static readonly HashSet<string> _whitelistSet = new HashSet<string>
    {
        "2104522031", // luvs
        "2107741437", //baby girl
        "9793448888", //baby boy
        "4088386160", // bro
        "9169474683", // ethan
        "2098752885", // shane
        "2109662250", // mom

        "9797645575", // A&M Middle School Front Desk
        "9797644264", // A&M Middle School Nurse
        "9797644207", // A&M Middle School Registrar
        "9797644279", // A&M Middle School Library
        "9797644211", // A&M Middle School Counselor

        "9797645400", // CSISD
        "9797645530", // CSISD

        "2104884754", // Alex Garner
        "2108632912", // Greer Beacker

        "9794585437", // Becky Gates Children’s Center
        "9794586836" // Charlotte Sharp Children’s Center
    };

      public static void Initialize()
      {
         IsThisNumberWhitelisted("");
      }


      public static bool IsThisNumberWhitelisted(string number)
      {
         if (string.IsNullOrWhiteSpace(number)) return false;

         // Normalize the incoming number once
         string normalized = Normalize(number);

         // HashSet.Contains is extremely fast
         return _whitelistSet.Contains(normalized);
      }

      private static string Normalize(string number)
      {
         // Faster way to filter digits than LINQ for high-frequency calls
         char[] buffer = new char[number.Length];
         int idx = 0;
         foreach (char c in number)
         {
            if (char.IsDigit(c)) buffer[idx++] = c;
         }
         return new string(buffer, 0, idx);
      }









      // Using a static ObservableCollection as a simple, in-memory data store for the draft.
      // In a production app, this would be backed by SQLite or Secure Storage.
      public static ObservableCollection<string> Whitelist { get; private set; } = new ObservableCollection<string>
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

      public static void AddNumber(string number)
      {
         if (!Whitelist.Contains(number))
         {
            Whitelist.Add(number);
            // In a real app, save to persistent storage here.
         }
      }

      public static void RemoveNumber(string number)
      {
         Whitelist.Remove(number);
         // In a real app, remove from persistent storage here.
      }

      // Simple check function used by both the UI and the native service
      public static bool IsNumberWhitelisted(string number)
      {
         // Simple normalization (remove non-digits for comparison)
         string normalizedNumber = new string(number.Where(char.IsDigit).ToArray());
         return Whitelist.Any(w => new string(w.Where(char.IsDigit).ToArray()) == normalizedNumber);
      }
   }

}
