using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using TenDaysOfXamarin.Model;
using SQLite;

namespace TenDaysOfXamarin
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void CheckIfShouldBeEnabled()
        {
            if (!string.IsNullOrWhiteSpace(titleEntry.Text) && !string.IsNullOrWhiteSpace(contentEditor.Text))
            {
                saveButton.IsEnabled = true;
            }
            else
            {
                saveButton.IsEnabled = false;
            }
        }

        private void contentEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfShouldBeEnabled();
        }

        private void titleEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfShouldBeEnabled();
        }

        private void saveButton_Clicked(object sender, EventArgs e)
        {
            int insertedCount = 0;
            Experience newExp = new Experience()
            {
                Title = titleEntry.Text,
                Content = contentEditor.Text,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            using (SQLiteConnection connection = new SQLiteConnection(App.DatabasePath))
            {
                connection.CreateTable<Experience>();
                insertedCount = connection.Insert(newExp);
            }

            if (insertedCount > 0)
            {
                titleEntry.Text = String.Empty;
                contentEditor.Text = String.Empty;
            }
            else
            {
                DisplayAlert("Error", "There was an error inserting the Experience, Please try again", "Ok");
            }
        }

        private void cancelButton_Clicked(object sender, EventArgs e)
        {

        }
    }
}
