using CommunityToolkit.Maui;
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
                .UseMauiCommunityToolkitCamera()
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

            //Add viewmodels
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddTransient<DetailViewModel>();
            builder.Services.AddTransient<QuizViewModel>();
            builder.Services.AddTransient<IdentifyViewModel>();

            //Add views
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<DetailPage>();
            builder.Services.AddTransient<QuizPage>();
            builder.Services.AddTransient<IdentifyPage>();

            return builder.Build();
        }
    }
}
