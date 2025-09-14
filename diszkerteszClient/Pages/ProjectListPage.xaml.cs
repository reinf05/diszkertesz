namespace Díszkertész_kliens.Pages
{
    public partial class ProjectListPage : ContentPage
    {
        public ProjectListPage(ProjectListPageModel model)
        {
            BindingContext = model;
            InitializeComponent();
        }
    }
}