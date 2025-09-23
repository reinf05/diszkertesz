using diszkerteszClient.Viewmodels;

namespace diszkerteszClient.View;

public partial class IdentifyPage : ContentPage
{
	public IdentifyPage(IdentifyViewModel identifyViewModel)
	{
		InitializeComponent();
		BindingContext = identifyViewModel;
    }
}