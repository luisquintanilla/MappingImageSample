using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Windows.Devices.Geolocation;
using Windows.Graphics.Imaging;
using Windows.Perception.Spatial;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MappingImageSampleUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void MapControl_Loaded(object sender, RoutedEventArgs e)
        {
            BasicGeoposition cityPosition = new BasicGeoposition() { Latitude = 47.604, Longitude = -122.329 };
            Geopoint cityCenter = new Geopoint(cityPosition);

            await (sender as MapControl).TrySetViewAsync(cityCenter);
        }

        private async void QueryLocation_Click(object sender, RoutedEventArgs e)
        {
            // 1. Reverse geocode 
            var coordinates = await GetCoordinatesAsync(AddressBar.Text);

            // 2. Update map with new address location
            await UpdateMapLocation(SatelliteMap, coordinates);

            // 3. Convert map display into an image
            var satelliteImage = await GetMapAsImageAsync();

            // 4. Make a prediction
            PredictionText.Text = "Inspecting Image";
            var prediction = await ClassifyImageAsync(satelliteImage);

            // 5. Display prediction
            PredictionText.Text = $"Prediction: {prediction}";
        }

        private async Task<Coordinates> GetCoordinatesAsync(string address)
        {
            Coordinates result;

            using (HttpClient client = new HttpClient())
            {
                //Generate URL
                string urlEncodedAddress = HttpUtility.UrlEncode(address);
                var uri = new Uri($"https://nominatim.openstreetmap.org/search?q={urlEncodedAddress}&format=json");

                // Build request
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add("User-Agent", "MappingImageSampleUWP/1.0");

                // Get coordinates
                var response = await client.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                //Parse results
                var coordinates = JsonSerializer.Deserialize<IEnumerable<Coordinates>>(body).FirstOrDefault();

                //Return results
                if (coordinates == null)
                {
                    result = new Coordinates { Latitude = "47.604", Longitude = "-122.329" };
                    await new MessageDialog("Could not find address provided.", "Address Not Found").ShowAsync();
                }
                else
                {
                    result = coordinates;
                }
            }

            return result;
        }

        private async Task UpdateMapLocation(MapControl map, Coordinates coordinates)
        {
            BasicGeoposition newPosition = new BasicGeoposition()
            {
                Latitude = float.Parse(coordinates.Latitude),
                Longitude = float.Parse(coordinates.Longitude)
            };

            await map.TrySetViewAsync(new Geopoint(newPosition));
        }

        private async Task<byte[]> GetMapAsImageAsync()
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap();
            await renderBitmap.RenderAsync(SatelliteMap);
            IBuffer pixelBuffer = await renderBitmap.GetPixelsAsync();

            var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(pixelBuffer, BitmapPixelFormat.Bgra8, renderBitmap.PixelWidth, renderBitmap.PixelHeight, BitmapAlphaMode.Ignore);

            byte[] array;
            using(var stream = new InMemoryRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                encoder.SetSoftwareBitmap(softwareBitmap);
                await encoder.FlushAsync();
                array = new byte[stream.Size];
                await stream.ReadAsync(array.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
            }

            return array;
        }

        private async Task<string> ClassifyImageAsync(byte[] imageBytes)
        {
            string prediction;
            string base64image = Convert.ToBase64String(imageBytes);
            string content = JsonSerializer.Serialize(
                new Dictionary<string, string>
                {
                    { "data", base64image }
                });

            using (var client = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (a,b,c,d) => true}))
            {
                var res = await client.PostAsync("https://localhost:44335/api/classification", new StringContent(content,Encoding.UTF8,"application/json"));
                prediction = await res.Content.ReadAsStringAsync();
            }

            return prediction;
        }
    }
}
