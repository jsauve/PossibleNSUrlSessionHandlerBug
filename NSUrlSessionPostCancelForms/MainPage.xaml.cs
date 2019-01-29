using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace NSUrlSessionPostCancelForms
{
    public partial class MainPage : ContentPage
    {
        readonly HttpClient _httpClient;
        const string _locationsUrl = "https://xamioshttptest.azurewebsites.net/api/xamioshandlertest";

        CancellationTokenSource _cts;
        bool _isBusy;

        public MainPage()
        {
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            InitializeComponent();

            LoadButton.Clicked += LoadButton_Clicked;

            CancelButton.Clicked += CancelButton_Clicked;
        }

        async void LoadButton_Clicked(object sender, EventArgs e)
        {
            if (_isBusy)
                return;

            _isBusy = true;

            var timer = new Stopwatch();
            timer.Start();

            try
            {
                LoadButton.Text = "Posting data...";

                CancelButton.IsVisible = true;

                await PerformPost();

                LoadButton.Text = "Data posted! Post again?";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                LoadButton.Text = "Post failed. Try again?";
            }
            finally
            {
                _isBusy = false;
                CancelButton.IsVisible = false;
                timer.Stop();
                Debug.WriteLine($"Elapsed time: {timer.ElapsedMilliseconds} ms");
            }
        }

        async Task PerformPost()
        {
            _cts = new CancellationTokenSource();

            var request = new HttpRequestMessage(HttpMethod.Post, _locationsUrl);

            var query = new Dictionary<string, string>()
            {
                { "Limit", $"{int.MaxValue}" }
            };

            var contentText = JsonConvert.SerializeObject(query);

            request.Content = new StringContent(contentText);

            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            HttpResponseMessage response = null;

            response = await _httpClient.SendAsync(request, _cts.Token);

            var content = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"Response content length: {content.Length}");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(response.ReasonPhrase);
        }

        void CancelButton_Clicked(object sender, EventArgs e)
        {
            _cts.Cancel();
        }
    }
}
