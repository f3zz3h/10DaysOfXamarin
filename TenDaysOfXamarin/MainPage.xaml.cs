using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using TenDaysOfXamarin.Model;
using SQLite;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

namespace TenDaysOfXamarin
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            locator.PositionChanged += Locator_PositionChanged;

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
            Navigation.PopAsync();
        }

        private async void GetLocationPermission()
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.LocationWhenInUse);
            if(status != PermissionStatus.Granted)
            {
                //not granted
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.LocationWhenInUse))
                {
                    // This is not the actual permission request just the Rationale message
                    await DisplayAlert("Need your permission", "We need to access your location", "Ok");
                }

                var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.LocationWhenInUse);
                if (results.ContainsKey(Permission.LocationWhenInUse))
                {
                    status = results[Permission.LocationWhenInUse];
                }
            }

            //granted
            if (status == PermissionStatus.Granted)
            {

            }
            else
            {
                await DisplayAlert("Access to location denied", "We don't have access to your location", "Ok");
            }

        }

        Position position;
        IGeolocator locator = CrossGeolocator.Current;
        private async void GetLocation()
        {
     
            position = await locator.GetPositionAsync();
            await locator.StartListeningAsync(TimeSpan.FromMinutes(30), 500);
        }


        private void Locator_PositionChanged(object sender, PositionEventArgs e)
        {
            position = e.Position;
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            GetLocationPermission();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            locator.StopListeningAsync();
        }
    }
}
