using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        [NotifyPropertyChangedFor(nameof(IsNotLoaded))]
        private bool isLoaded = false;
        public bool IsNotLoaded => !IsLoaded;

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

                var quiz = await plantService.GetQuiz();

                if(quiz is null)
                {
                    await Shell.Current.DisplayAlert("Error", "Could not load quiz", "OK");
                    return;
                }
                string baseURL = "http://192.168.1.151:5000/images/";
                quiz.ImagePath = baseURL + quiz.ImagePath;
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
    }
}
