using diszkerteszClient.Viewmodels;

namespace diszkerteszClient.View
{
    public partial class DetailPage : ContentPage
    {
        public DetailPage(DetailViewModel detailViewModel)
        {

            BindingContext = detailViewModel;
            InitializeComponent();
        }
    }
}