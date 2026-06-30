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

        [ObservableProperty]
        private QuizName quizName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsMultipleChoice))]
        [NotifyPropertyChangedFor(nameof(IsImageToName))]
        private GameType selectedGameType = GameType.MultipleChoice;

        [ObservableProperty]
        private string typedAnswer;


        public bool IsMultipleChoice => SelectedGameType == GameType.MultipleChoice;
        public bool IsImageToName => SelectedGameType == GameType.ImageToName;

        public QuizViewModel(PlantService plantService)
        {
            this.plantService = plantService;
            Title = "Játék";
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            IsLoaded = false;
            Quiz = null;
            QuizName = null;
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

                int randomImage = rnd.Next(0, quizGet.ImagePath.Count);

                quizGet.ChosenImage = $"{quizGet.ImagePath[randomImage]}";

                quizGet.Names = quizGet.Names.OrderBy(x => rnd.Next()).ToArray();

                Quiz = quizGet;
                SelectedGameType = GameType.MultipleChoice;
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
        private async Task GetQuizNameAsync()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                TypedAnswer = string.Empty;

                var quizGet = await plantService.GetQuizName();
                if (quizGet is null)
                {
                    await Shell.Current.DisplayAlert("Error", "Could not load quiz", "OK");
                    return;
                }

                QuizName = quizGet;
                SelectedGameType = GameType.ImageToName;
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
            if (string.IsNullOrEmpty(givenAnswer))
            {
                await Shell.Current.DisplayAlert("Hiba", "Kérlek adj meg egy választ!", "OK");
                return;
            }
            await EvaluateAnswerAsync(givenAnswer);
        }

        [RelayCommand]
        private async Task SubmitAnswerAsync()
        {
            if (string.IsNullOrEmpty(TypedAnswer))
            {
                await Shell.Current.DisplayAlert("Hiba", "Kérlek adj meg egy választ!", "OK");
                return;
            }
            await EvaluateAnswerAsync(TypedAnswer);
        }

        private async Task EvaluateAnswerAsync(string givenAnswer)
        {
            givenAnswer = givenAnswer.ToLower();
            string correctAnswer = string.Empty;
            if (SelectedGameType == GameType.ImageToName)
            {
                correctAnswer = QuizName.Name.ToLower();
            }
            else
            {
                correctAnswer = Quiz.Correct.ToLower();
            }
            if (!string.IsNullOrEmpty(correctAnswer) && string.Equals(correctAnswer.Trim(), givenAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.DisplayAlert("Helyes!", "Gratulálok, helyes választ adtál!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Helytelen!", $"Sajnos nem jó a válaszod! A helyes válasz: {correctAnswer}", "OK");
            }

            if (SelectedGameType == GameType.ImageToName)
            {
                await GetQuizNameAsync();
            }
            else
            {
                await GetQuizAsync();
            }
        }
    }
}
