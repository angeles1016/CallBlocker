namespace MainPage
{
   public partial class MainPage : ContentPage
   {
      private readonly ICallBlockerService _service;
      private Entry _newNumberEntry;
      private ListView _whitelistView;

      public MainPage(ICallBlockerService callBlockerService)
      {
         Title = "Secure Call Whitelist";
         _service = callBlockerService;

         // Ensure the native service is initialized when the UI loads
         _service.InitializeService();

         // ** A critical instruction for the user **
         var warningLabel = new Label
         {
            Text = "⚠️ SETUP REQUIRED: For this app to block calls, you MUST manually set it as the default 'Caller ID & Spam app' in your phone settings after installation.",
            TextColor = Colors.Red,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(15),
            HorizontalTextAlignment = TextAlignment.Center
         };

         // Input field for new numbers
         _newNumberEntry = new Entry
         {
            Placeholder = "Enter number to whitelist (e.g., 555-123-4567)",
            Keyboard = Keyboard.Telephone,
            Margin = new Thickness(10, 0)
         };

         // Button to add the number
         var addButton = new Button
         {
            Text = "Add to Whitelist",
            BackgroundColor = Color.FromHex("#4CAF50"),
            TextColor = Colors.White,
            CornerRadius = 8,
            Margin = new Thickness(10)
         };
         addButton.Clicked += OnAddButtonClicked;

         // Role Manager 
         var RoleButton = new Button
         {
            Text = "Request as Role Manager",
            BackgroundColor = Color.FromHex("#4CAF50"),
            TextColor = Colors.White,
            CornerRadius = 8,
            Margin = new Thickness(10)
         };
         RoleButton.Clicked += OnRoleButtonClicked;


         // List view for whitelisted numbers
         _whitelistView = new ListView
         {
            ItemsSource = WhitelistDataStore.Whitelist,
            HasUnevenRows = true,
            ItemTemplate = new DataTemplate(() =>
            {
               var cell = new TextCell();
               cell.SetBinding(TextCell.TextProperty, ".");

               var deleteAction = new MenuItem { Text = "Remove", IsDestructive = true };
               deleteAction.SetBinding(MenuItem.CommandParameterProperty, ".");
               deleteAction.Clicked += OnRemoveItemClicked;

               // FIX: Return the TextCell directly and add ContextActions to it.
               cell.ContextActions.Add(deleteAction);
               return cell;
            }),
            Margin = new Thickness(10)
         };

         Content = new StackLayout
         {
            Padding = new Thickness(0, 20),
            Children =
            {
                warningLabel,
                _newNumberEntry,
                addButton,
                RoleButton,
                new Label { Text = "Whitelisted Numbers (Exceptions)", Margin = new Thickness(15, 10, 15, 0), FontAttributes = FontAttributes.Bold },
                _whitelistView
            }
         };
      }


      
      private async void OnRoleButtonClicked(object sender, System.EventArgs e)
      {
         _service.RequestCallScreeningRole(); 
         await DisplayAlert("Success", $"Request Role Manager Completed", "OK");
      }

      private async void OnAddButtonClicked(object sender, System.EventArgs e)
      {
         string number = _newNumberEntry.Text?.Trim();
         if (string.IsNullOrWhiteSpace(number))
         {
            await DisplayAlert("Error", "Please enter a valid phone number.", "OK");
            return;
         }

         _service.AddToWhitelist(number);
         _newNumberEntry.Text = string.Empty;
         await DisplayAlert("Success", $"Number {number} added to whitelist.", "OK");
      }

      private void OnRemoveItemClicked(object sender, System.EventArgs e)
      {
         var menuItem = sender as MenuItem;
         if (menuItem?.CommandParameter is string numberToRemove)
         {
            _service.RemoveFromWhitelist(numberToRemove);
         }
      }

      private void OnCounterClicked(object sender, System.EventArgs e)
      {
         // Counter button logic (if needed)
      }
   }

}
