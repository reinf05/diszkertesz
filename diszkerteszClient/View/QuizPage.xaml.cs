using diszkerteszClient.Viewmodels;

namespace diszkerteszClient.View;

public partial class QuizPage : ContentPage
{
	public QuizPage(QuizViewModel quizViewModel)
	{
		InitializeComponent();
		BindingContext = quizViewModel;
    }
}