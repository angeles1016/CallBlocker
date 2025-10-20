using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainPage
{
   public interface ICallBlockerService
   {
      void InitializeService();

      // Method to add a number to the permanent block list (whitelist for exceptions)
      void AddToWhitelist(string phoneNumber);

      // Method to remove a number from the list
      void RemoveFromWhitelist(string phoneNumber);

      // Method to retrieve the current whitelist
      ObservableCollection<string> GetWhitelist();
   }
}
