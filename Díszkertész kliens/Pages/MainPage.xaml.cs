using Díszkertész_kliens.Models;
using Díszkertész_kliens.PageModels;

namespace Díszkertész_kliens.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}