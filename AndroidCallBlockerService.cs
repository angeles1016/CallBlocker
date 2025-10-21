using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if ANDROID
using Android.App;
using Android.OS; // You'll likely need this for BuildVersionCodes
using Android.Content; // You'll likely need this for Context
using Android.Views; // You'll likely need this for IWindowManager or similar
using Android.Runtime; // You'll likely need this for JavaCast
using Android.Widget; // You'll likely need this for Toast
using Android.Telecom; // You need this for the CallScreeningService
#endif

namespace MainPage
{
   public partial class AndroidCallBlockerService : ICallBlockerService
   {
      public void InitializeService()
      {
         // This is called by the UI to ensure initialization.
         System.Diagnostics.Debug.WriteLine("[AndroidCallBlockerService] Initialization complete.");
      }

      public void AddToWhitelist(string phoneNumber)
      {
         WhitelistDataStore.AddNumber(phoneNumber);
      }

      public void RemoveFromWhitelist(string phoneNumber)
      {
         WhitelistDataStore.RemoveNumber(phoneNumber);
      }

      public ObservableCollection<string> GetWhitelist()
      {
         return WhitelistDataStore.Whitelist;
      }

      public void RequestCallScreeningRole()
      {
         #if ANDROID
         // Requires API 29 (Android 10) or higher, which the A12 has.
         if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
         {
            var activity = Platform.CurrentActivity;
            if (activity == null) return;

            // Get the RoleManager (for API 29+)
            var roleManager = activity.GetSystemService(Android.Content.Context.RoleService) as Android.App.Roles.RoleManager;

            // Check if we are already the default call screening app.
            if (roleManager.IsRoleHeld(Android.App.Roles.RoleManager.RoleCallScreening))
            {
               System.Diagnostics.Debug.WriteLine("[CallBlocker] Already the default Call Screening app.");
               return;
            }

            // Create an Intent to request the Call Screening role
            var intent = roleManager.CreateRequestRoleIntent(Android.App.Roles.RoleManager.RoleCallScreening);

            // Start the activity which displays the system role selection dialog
            activity.StartActivityForResult(intent, 12345); // Use a unique request code
         }
        #endif
      }
   }
}
