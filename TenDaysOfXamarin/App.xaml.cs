using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TenDaysOfXamarin.View;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace TenDaysOfXamarin
{
    public partial class App : Application
    {
        public static string DatabasePath;
        public App(string dataBasepath)
        {
            InitializeComponent();

            DatabasePath = dataBasepath;

            MainPage = new NavigationPage(new ExperiencesPage());
        }
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new ExperiencesPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
