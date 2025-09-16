using diszkerteszClient.Services;
using diszkerteszClient.View;
using diszkerteszClient.Viewmodels;
using Microsoft.Extensions.Logging;

namespace diszkerteszClient
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

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            //Add services
            builder.Services.AddSingleton<PlantService>();

            builder.Services.AddSingleton<MainViewModel>();

            builder.Services.AddSingleton<MainPage>();

            return builder.Build();
        }
    }
}
