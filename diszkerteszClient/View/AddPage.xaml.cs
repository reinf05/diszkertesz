using diszkerteszClient.Viewmodels;

namespace diszkerteszClient.View;

public partial class AddPage : ContentPage
{
	public AddPage(AddViewModel addVM)
	{
		BindingContext = addVM;
        InitializeComponent();
	}
}