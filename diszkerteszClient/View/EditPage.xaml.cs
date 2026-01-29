using diszkerteszClient.Viewmodels;

namespace diszkerteszClient.View;

public partial class EditPage : ContentPage
{
	public EditPage(EditViewModel editVM)
	{
		BindingContext = editVM;
        InitializeComponent();
	}
}