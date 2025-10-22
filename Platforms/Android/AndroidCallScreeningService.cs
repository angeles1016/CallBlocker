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
#if ANDROID
using Android.App;
using Android.OS; // You'll likely need this for BuildVersionCodes
using Android.Content; // You'll likely need this for Context
using Android.Views; // You'll likely need this for IWindowManager or similar
using Android.Runtime; // You'll likely need this for JavaCast
using Android.Widget; // You'll likely need this for Toast
using Android.Telecom; // You need this for the CallScreeningService
#endif

using System.IO;
using Microsoft.Maui.Storage; // Make sure this is included at the top of your file




// 7a. The Android CallScreeningService implementation
[Service(Exported = true, Permission = "android.permission.BIND_SCREENING_SERVICE",
            Name = "com.companyname.AndroidCallScreeningService")]
[IntentFilter(new[] { "android.telecom.CallScreeningService" })]
public class AndroidCallScreeningService : CallScreeningService
   {
      public override void OnScreenCall([GeneratedEnum] Call.Details callDetails)
      {
         // 1. Check if the incoming number is valid
         var handle = callDetails.GetHandle();
         var incomingNumber = handle != null ? handle.SchemeSpecificPart : null;

         if (string.IsNullOrWhiteSpace(incomingNumber))
         {
            // Number is private/unknown, usually blocked by default unless whitelisted.
            // For safety, let the call through if we can't read the number.
            RespondToCall(callDetails, new CallResponse.Builder().Build());
            return;
         }

         if (string.IsNullOrWhiteSpace(incomingNumber))
         {
            // Number is private/unknown, usually blocked by default unless whitelisted.
            // For safety, let the call through if we can't read the number.
            RespondToCall(callDetails, new CallResponse.Builder().Build());
            return;
         }

         // 2. Perform the whitelist check using the shared logic
         bool isWhitelisted = MainPage.WhitelistDataStore.IsNumberWhitelisted(incomingNumber);

         if (isWhitelisted)
         {
            // Call is whitelisted, allow it to proceed
            // SetAllow(true) is not needed; simply building a default CallResponse is sufficient.
            var response = new CallResponse.Builder()
                .Build();
            RespondToCall(callDetails, response);
            System.Diagnostics.Debug.WriteLine($"[CallBlocker] Allowing whitelisted call from: {incomingNumber}");
         }
         else
         {
         // Call is NOT whitelisted, reject it
         // Call is NOT whitelisted, reject it
         // Call is NOT whitelisted, reject it
         var response = new CallResponse.Builder(); 

         response.SetSkipCallLog(true);           // HIDE from logs to prevent "missed call" notif
         response.SetSkipNotification(true);      // Silence the notification
         response.SetRejectCall(true);            // REJECT the call
         response.SetDisallowCall(true);      // DISALLOW the call

         var thisResponse =response.Build();     
         RespondToCall(callDetails, thisResponse);
         // ...
         // ...
         System.Diagnostics.Debug.WriteLine($"[CallBlocker] Blocking non-whitelisted call from: {incomingNumber}");

         // ----------------------------------------------------
         // >> CALL THE NEW LOGGING METHOD HERE <<
         LogBlockedCall(incomingNumber);
         // ----------------------------------------------------


         // Show a transient message to the user that a call was blocked (for testing/confirmation)
         // Note: Toast/Notification visibility might be limited by the OS when running in the background.
         Handler mainHandler = new Handler(Looper.MainLooper);
            mainHandler.Post(() =>
            {
               Toast.MakeText(this, $"Blocked call from {incomingNumber}", ToastLength.Short).Show();
            });
         }
      } 


// Inside your AndroidCallScreeningService class:

      private void LogBlockedCall(string phoneNumber)
      {
         try
         {
            // 1. Define the file path in the app's private data folder.
            // This is accessible only by your app and doesn't require extra permissions.
            string fileName = "BlockedCallLog.txt";
            string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            // 2. Create the log entry with a timestamp.
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timestamp}] BLOCKED: {phoneNumber}\n";

            // 3. Append the entry to the file.
            // If the file doesn't exist, it will be created automatically.
            File.AppendAllText(filePath, logEntry);

            // For debugging purposes, you can also write to the console/logcat
            System.Diagnostics.Debug.WriteLine($"[CallBlocker Logger] Logged to file: {logEntry.Trim()} at {filePath}");
         }
         catch (Exception ex)
         {
            System.Diagnostics.Debug.WriteLine($"[CallBlocker Logger] Failed to write log: {ex.Message}");
         }
      }



   }

   

 

#endif