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
using TenDaysOfXamarin.ViewModels;

namespace TenDaysOfXamarin
{
    public partial class MainPage : ContentPage
    {
        MainVM ViewModel;
        public MainPage()
        {
            InitializeComponent();
            ViewModel = new MainVM();
            BindingContext = ViewModel;

            locator.PositionChanged += Locator_PositionChanged;

        }

        private void saveButton_Clicked(object sender, EventArgs e)
        {
            int insertedCount = 0;
            Experience newExp = new Experience()
            {
                Title = ViewModel.Title,
                Content = ViewModel.Content,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                VenueName = ViewModel.SelectedVenue.name,
                VenueCategory = ViewModel.SelectedVenue.MainCategory ,
                VenueLat = float.Parse(ViewModel.SelectedVenue.location.Coordinates.Split(',')[0]),
                VenueLng = float.Parse(ViewModel.SelectedVenue.location.Coordinates.Split(',')[1])
            };

            using (SQLiteConnection connection = new SQLiteConnection(App.DatabasePath))
            {
                connection.CreateTable<Experience>();
                insertedCount = connection.Insert(newExp);
            }

            if (insertedCount > 0)
            {
                ViewModel.Title = String.Empty;
                ViewModel.Content = String.Empty;
                ViewModel.SelectedVenue = null;              
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
                GetLocation();
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

            if (!string.IsNullOrWhiteSpace(ViewModel.Query) && position != null)
            {
                string url = $"https://api.foursquare.com/v2/venues/search?ll={position.Latitude},{position.Longitude}&radius=500&query={ViewModel.Query}&limit=3&client_id={Helpers.Constants.FOURSQR_CLIENT_ID}&client_secret={Helpers.Constants.FOURSQR_CLIENT_SECRET}&v={DateTime.Now.ToString("yyyyMMdd")}";

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
                ViewModel.Query = string.Empty;
                venuesListView.IsVisible = false;
            }
            else
            {
                selectedVenueStackLayout.IsVisible = false;
            }

        }
    }
}
