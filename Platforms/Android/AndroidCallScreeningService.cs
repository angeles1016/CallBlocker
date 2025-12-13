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
      long startMs = SystemClock.ElapsedRealtime();

      // Build a tag we can use to correlate lines for the same call
      var handle = callDetails.GetHandle();
      var incomingNumber = handle != null ? handle.SchemeSpecificPart : null;
      string callTag = $"{incomingNumber ?? "UNKNOWN"}@{startMs}";

      void T(string msg)
      {
         var elapsed = SystemClock.ElapsedRealtime() - startMs;
         LoggerCall($"[CallBlocker Timing {callTag}] +{elapsed}ms {msg}");
         System.Diagnostics.Debug.WriteLine($"[CallBlocker Timing {callTag}] +{elapsed}ms {msg}");
      }

      try
      {
         T("OnScreenCall invoked");

         // 1) If number is unknown/hidden
         if (string.IsNullOrWhiteSpace(incomingNumber))
         {
            T("Incoming number is null/empty (unknown/hidden) -> BLOCK");

            var response = new CallResponse.Builder()
                .SetSkipCallLog(true)
                .SetSkipNotification(true)
                .SetRejectCall(true)
                .SetDisallowCall(true)
                .Build();

            long beforeRespond = SystemClock.ElapsedRealtime() - startMs;
            if (beforeRespond > 4500) T($"WARNING: RespondToCall about to run late ({beforeRespond}ms)");

            T("Calling RespondToCall (BLOCK unknown/hidden)");
            RespondToCall(callDetails, response);
            T("RespondToCall returned (BLOCK unknown/hidden)");

            // Do slower work AFTER responding
            LogBlockedCall("<UNKNOWN OR HIDDEN>");
            return;
         }

         // 2) Time the whitelist check
         long wlStart = SystemClock.ElapsedRealtime();
         bool isWhitelisted = MainPage.WhitelistDataStore.IsNumberWhitelisted(incomingNumber);
         long wlMs = SystemClock.ElapsedRealtime() - wlStart;

         T($"Whitelist check done: isWhitelisted={isWhitelisted}, wlMs={wlMs}");

         if (isWhitelisted)
         {
            // Allow whitelisted calls
            var response = new CallResponse.Builder().Build();

            long beforeRespond = SystemClock.ElapsedRealtime() - startMs;
            if (beforeRespond > 4500) T($"WARNING: RespondToCall about to run late ({beforeRespond}ms)");

            T("Calling RespondToCall (ALLOW whitelisted)");
            RespondToCall(callDetails, response);
            T("RespondToCall returned (ALLOW whitelisted)");

            // Do slower work AFTER responding
            LogAllowedCall(incomingNumber, "whitelisted");
            System.Diagnostics.Debug.WriteLine($"[CallBlocker] Allowing whitelisted call from: {incomingNumber}");
         }
         else
         {
            // Block non-whitelisted calls
            var builder = new CallResponse.Builder();
            builder.SetSkipCallLog(true);
            builder.SetSkipNotification(true);
            builder.SetRejectCall(true);
            builder.SetDisallowCall(true);

            var response = builder.Build();

            long beforeRespond = SystemClock.ElapsedRealtime() - startMs;
            if (beforeRespond > 4500) T($"WARNING: RespondToCall about to run late ({beforeRespond}ms)");

            T("Calling RespondToCall (BLOCK non-whitelisted)");
            RespondToCall(callDetails, response);
            T("RespondToCall returned (BLOCK non-whitelisted)");

            // Do slower work AFTER responding
            System.Diagnostics.Debug.WriteLine($"[CallBlocker] Blocking non-whitelisted call from: {incomingNumber}");
            LogBlockedCall(incomingNumber);

            // Toast AFTER responding (still may be slow / not always visible in background)
            Handler mainHandler = new Handler(Looper.MainLooper);
            mainHandler.Post(() =>
            {
               Toast.MakeText(this, $"Blocked call from {incomingNumber}", ToastLength.Long).Show();
            });
         }
      }
      catch (Exception ex)
      {
         // IMPORTANT: never crash the screening path
         T($"ERROR: Exception in OnScreenCall: {ex.Message}");

         // Safe fallback: allow call if something went wrong
         var response = new CallResponse.Builder().Build();

         long beforeRespond = SystemClock.ElapsedRealtime() - startMs;
         if (beforeRespond > 4500) T($"WARNING: fallback RespondToCall about to run late ({beforeRespond}ms)");

         T("Calling RespondToCall (FALLBACK ALLOW)");
         RespondToCall(callDetails, response);
         T("RespondToCall returned (FALLBACK ALLOW)");
      }
   }



   // Inside your AndroidCallScreeningService class:
   private void LoggerCall(string msg)
   {
      try
      {
         // 1. Define the file path in the app's private data folder.
         // This is accessible only by your app and doesn't require extra permissions.
         string fileName = "BlockedCallLog.txt";
         string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

         // 2. Create the log entry with a timestamp.
         string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
         string logEntry = $"[{timestamp}]:  {msg}\n";

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


      private void LogAllowedCall(string phoneNumber, string reason)
      {
         try
         {
            string fileName = "AllowedCallLog.txt";
            string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timestamp}] ALLOWED ({reason}): {phoneNumber}\n";
            File.AppendAllText(filePath, logEntry);
            System.Diagnostics.Debug.WriteLine($"[CallBlocker Logger] Logged ALLOW: {logEntry.Trim()}");
         }
         catch (Exception ex)
         {
            System.Diagnostics.Debug.WriteLine($"[CallBlocker Logger] Failed to write allow log: {ex.Message}");
         }
      }




}





#endif