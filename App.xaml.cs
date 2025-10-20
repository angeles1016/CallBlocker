using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MainPage
{
    public partial class App : Application
    {
      private readonly ICallBlockerService _callBlockerService;

      // Parameterless constructor is usually needed by the framework
      public App()
      {
      }

      // Dependency injected constructor (used by the framework when resolving App)
      public App(ICallBlockerService callBlockerService)
      {
         _callBlockerService = callBlockerService;
      }

      // Override CreateWindow to ensure dependency injection happens correctly and 
      // to prevent the "Both MainPage was set and CreateWindow was overridden" error.
      protected override Window CreateWindow(IActivationState activationState)
      {
         // Use the injected service to create the NavigationPage with the main content
         var mainPage = new MainPage(_callBlockerService);

         var navigationPage = new NavigationPage(mainPage)
         {
            BarBackgroundColor = Color.FromHex("#00796B"),
            BarTextColor = Colors.White
         };

         return new Window(navigationPage);
      }
   }
}