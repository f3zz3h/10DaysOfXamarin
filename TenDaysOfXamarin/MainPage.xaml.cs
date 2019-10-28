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
using System.Net.Http;
using Newtonsoft.Json;

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
                UpdatedAt = DateTime.Now,
                VenueName = venueNameLabel.Text,
                VenueCategory = venueCategoryLabel.Text,
                VenueLat = float.Parse(venueCoordinatesLabel.Text.Split(',')[0]),
                VenueLng = float.Parse(venueCoordinatesLabel.Text.Split(',')[1])
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
                venueNameLabel.Text = String.Empty;
                venueCategoryLabel.Text = String.Empty;
                venueCoordinatesLabel.Text = String.Empty;
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

        private async void searchEntry_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!string.IsNullOrWhiteSpace(searchEntry.Text))
            {
                string url = $"https://api.foursquare.com/v2/venues/search?ll={position.Latitude},{position.Longitude}&radius=500&query={searchEntry.Text}&limit=3&client_id={Helpers.Constants.FOURSQR_CLIENT_ID}&client_secret={Helpers.Constants.FOURSQR_CLIENT_SECRET}&v={DateTime.Now.ToString("yyyyMMdd")}";

                // added using System.Net.Http;
                using (HttpClient client = new HttpClient())
                {
                    // made the method async
                    string json = await client.GetStringAsync(url);

                    // added using Newtonsoft.Json;
                    Search searchResult = JsonConvert.DeserializeObject<Search>(json);
                    venuesListView.IsVisible = true;
                    venuesListView.ItemsSource = searchResult.response.venues;
                }
            }
            else
            {
                venuesListView.IsVisible = false;
            }
        }

        private void venuesListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (venuesListView.SelectedItem != null)
            {
                selectedVenueStackLayout.IsVisible = true;
                searchEntry.Text = string.Empty;
                venuesListView.IsVisible = false;

                Venue selectedVenue = venuesListView.SelectedItem as Venue;
                venueNameLabel.Text = selectedVenue.name;
                venueCategoryLabel.Text = selectedVenue.categories.FirstOrDefault()?.name;
                venueCoordinatesLabel.Text = $"{selectedVenue.location.lat:0.000}, {selectedVenue.location.lng:0.000}";
            }
            else
            {
                selectedVenueStackLayout.IsVisible = false;
            }

        }
    }
}
