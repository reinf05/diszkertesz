namespace diszkerteszClient
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(View.DetailPage), typeof(View.DetailPage));
        }
    }
}
