using diszkerteszClient.Viewmodels;

namespace diszkerteszClient.View;

public partial class ProfilePage : ContentPage
{
	public ProfilePage(ProfileViewModel profilePageViewModel)
	{
		BindingContext = profilePageViewModel;
        InitializeComponent();
	}
}