using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenDaysOfXamarin.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TenDaysOfXamarin.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExperiencesPage : ContentPage
    {
        public ExperiencesPage()
        {
            InitializeComponent();
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MainPage());
        }

        private void ReadExperiences()
        {
            using (SQLiteConnection connection = new SQLiteConnection(App.DatabasePath))
            {
                connection.CreateTable<Experience>();
                List<Experience> experiences = connection.Table<Experience>().ToList();
                experiencesListView.ItemsSource = experiences;
            }
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            ReadExperiences();
        }
    }
}