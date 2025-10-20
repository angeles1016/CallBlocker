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
        "210-966-2250" // mom
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
