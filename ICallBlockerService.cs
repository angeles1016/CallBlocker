using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ----------------------------------------------------------------------
// *** ADD THIS CONDITIONAL BLOCK ***
// This ensures that 'using Android.App' is only compiled when building for Android.
#if ANDROID
using Android.App;
using Android.OS; // You'll likely need this for BuildVersionCodes
using Android.Content; // You'll likely need this for Context
using Android.Views; // You'll likely need this for IWindowManager or similar
using Android.Runtime; // You'll likely need this for JavaCast
using Android.Widget; // You'll likely need this for Toast
using Android.Telecom; // You need this for the CallScreeningService
#endif
// ----------------------------------------------------------------------

namespace MainPage
{
   public interface ICallBlockerService
   {

      void RequestCallScreeningRole();
      void InitializeService();

      // Method to add a number to the permanent block list (whitelist for exceptions)
      void AddToWhitelist(string phoneNumber);

      // Method to remove a number from the list
      void RemoveFromWhitelist(string phoneNumber);

      // Method to retrieve the current whitelist
      ObservableCollection<string> GetWhitelist();
   }
}
