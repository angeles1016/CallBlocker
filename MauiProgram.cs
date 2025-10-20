 
namespace MainPage
{
    public static class MauiProgram
    {

      public static MauiApp CreateMauiApp()
      {
         var builder = MauiApp.CreateBuilder();
         builder
             .UseMauiApp<App>()
             .ConfigureFonts(fonts =>
             {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
             });

         // Register all necessary components for Dependency Injection
         builder.Services.AddSingleton<ICallBlockerService, AndroidCallBlockerService>();
         // FIX: Register the App and the main page so the framework can resolve dependencies
         builder.Services.AddSingleton<App>();
         builder.Services.AddTransient<AndroidCallBlockerService>();

         return builder.Build();
      } 
   }
}
