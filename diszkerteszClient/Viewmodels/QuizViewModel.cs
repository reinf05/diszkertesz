using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using diszkerteszClient.Models;
using diszkerteszClient.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Viewmodels
{
    public partial class QuizViewModel : BaseViewModel
    {
        private PlantService plantService;

        [ObservableProperty]
        private Quiz quiz;

        public QuizViewModel(PlantService plantService)
        {
            this.plantService = plantService;
            Title = "Játék";
        }

        [RelayCommand]
        private async Task GetQuizAsync()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;

                var quizGet = await plantService.GetQuiz();

                if(quizGet is null)
                {
                    await Shell.Current.DisplayAlert("Error", "Could not load quiz", "OK");
                    return;
                }
                var rnd = new Random();

                string baseURL = "https://stdiszkerteszgerdev001.blob.core.windows.net/images/";
                quizGet.ImagePath = $"{baseURL}{quizGet.ImagePath}{rnd.Next(1, 6)}.jpeg";

                quizGet.Names = quizGet.Names.OrderBy(x => rnd.Next()).ToArray();

                Quiz = quizGet;
                IsLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GiveAnswerAsync(string givenAnswer)
        {
            if(givenAnswer == Quiz.Correct)
            {
                await Shell.Current.DisplayAlert("Helyes!", "Gratulálok, helyes választ adtál!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Helytelen!", $"Sajnos nem jó a válaszod! A helyes válasz: {Quiz.Correct}", "OK");
            }

            await GetQuizAsync();
        }
    }
}
