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
                .UseMauiCommunityToolkit()
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
            builder.Services.AddSingleton<UserService>();

            //Add viewmodels
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddTransient<DetailViewModel>();
            builder.Services.AddTransient<QuizViewModel>();
            builder.Services.AddTransient<IdentifyViewModel>();
            builder.Services.AddTransient<AddViewModel>();
            builder.Services.AddTransient<EditViewModel>();
            builder.Services.AddSingleton<ProfileViewModel>();

            //Add views
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<DetailPage>();
            builder.Services.AddTransient<QuizPage>();
            builder.Services.AddTransient<IdentifyPage>();
            builder.Services.AddTransient<AddPage>();
            builder.Services.AddTransient<EditPage>();
            builder.Services.AddSingleton<ProfilePage>();

            return builder.Build();
        }
    }
}
