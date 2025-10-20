#if ANDROID
using Android.App;
using Android.Content;
using Android.Telecom;
using Android.Telephony;
using Android.OS;
using Android.Widget;
using Android.Runtime;
using System;

using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Telecom;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Telecom.CallScreeningService;

namespace MainPage.Platforms.Android
{

   // 6a. The Android CallScreeningService implementation
   [Service(Permission = "android.permission.BIND_SCREENING_SERVICE",
            Name = "com.companyname.CallBlockerApp.AndroidCallScreeningService")]
   public class AndroidCallScreeningService : CallScreeningService
   {
      public override void OnScreenCall([GeneratedEnum] Call.Details callDetails)
      {
         // 1. Check if the incoming number is valid
#pragma warning disable CA1416 // Validate platform compatibility
         var handle = callDetails.GetHandle();
#pragma warning restore CA1416 // Validate platform compatibility
         var incomingNumber = handle != null ? handle.SchemeSpecificPart : null;

         if (string.IsNullOrWhiteSpace(incomingNumber))
         {
            // Number is private/unknown, usually blocked by default unless whitelisted.
            // For safety, let the call through if we can't read the number.
            RespondToCall(callDetails, new CallResponse.Builder().Build()); // FIXED: Removed SetAllow(true)
            return;
         }

         // 2. Perform the whitelist check using the shared logic
         bool isWhitelisted = WhitelistDataStore.IsNumberWhitelisted(incomingNumber);

         if (isWhitelisted)
         {
            // Call is whitelisted, allow it to proceed
            // FIX: SetAllow(true) is not needed; simply building a default CallResponse is sufficient.
            var response = new CallResponse.Builder()
                .Build();
            RespondToCall(callDetails, response);
            System.Diagnostics.Debug.WriteLine($"[CallBlocker] Allowing whitelisted call from: {incomingNumber}");
         }
         else
         {
            // Call is NOT whitelisted, reject it
            var response = new CallResponse.Builder()
                .SetRejectCall(true)            // REJECT the call
                .SetSkipCallLog(false)          // Optionally hide from logs (or leave false)
                .SetSkipNotification(true)      // Optionally silence the notification
                .Build();
            RespondToCall(callDetails, response);
            System.Diagnostics.Debug.WriteLine($"[CallBlocker] Blocking non-whitelisted call from: {incomingNumber}");

            // Show a transient message to the user that a call was blocked (for testing/confirmation)
            // Note: Toast/Notification visibility might be limited by the OS when running in the background.
            Handler mainHandler = new Handler(Looper.MainLooper);
            mainHandler.Post(() =>
            {
               Toast.MakeText(this, $"Blocked call from {incomingNumber}", ToastLength.Short).Show();
            });
         }
      }
   }


   // 6b. The Android Platform-Specific Service Wrapper
   // This class implements the shared interface using native Android APIs.
   public partial class AndroidCallBlockerService : ICallBlockerService
   {
      public void InitializeService()
      {
         // This method is primarily used to ensure the native CallScreeningService
         // is registered in the manifest and to instruct the user on setup.
         // No code required here as the Service is started by the OS when a call comes in, 
         // provided the app is set as the default Caller ID app.
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
   }

}

#endif