using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VisualSearchApp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddKeysPage : ContentPage
	{
        // booleans set when the keys are proven to work
        private bool computerVisionKeyWorks = false;
        private bool bingSearchKeyWorks = false;

        // URIs of the endpoints used in the test requests
        private string ocrUri = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/ocr?";
        private string searchUri = "https://api.cognitive.microsoft.com/bing/v5.0/search?q=test";

        public AddKeysPage ()
		{
			InitializeComponent();
		}

        // send a test POST request to see if the Vision API Key is functional
        async Task CheckComputerVisionKey(object sender = null, EventArgs e = null)
        {
            // Empty image for test OCR request
            byte[] emptyImage = new byte[10];

            HttpResponseMessage response;
            HttpClient VisionApiClient = new HttpClient();

            VisionApiClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ComputerVisionKeyEntry.Text);
            using (var content = new ByteArrayContent(emptyImage))
            {
                // The media type of the body sent to the API. "application/octet-stream" defines an image represented as a byte array
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await VisionApiClient.PostAsync(ocrUri, content);
            }
            if ((int)response.StatusCode != 401)
            {
                ComputerVisionKeyEntry.BackgroundColor = Color.Green;
                AppConstants.ComputerVisionApiKey = ComputerVisionKeyEntry.Text;
                computerVisionKeyWorks = true;
            }
            else
            {
                ComputerVisionKeyEntry.BackgroundColor = Color.Red;
                computerVisionKeyWorks = false;
            }
        }

        // send a test GET request to see if the Bing Search API key is functional
        async Task CheckBingSearchKey(object sender = null, EventArgs e = null)
        {
            HttpResponseMessage response;
            HttpClient SearchApiClient = new HttpClient();

            SearchApiClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", BingSearchKeyEntry.Text);

            response = await SearchApiClient.GetAsync(searchUri);
            if ((int)response.StatusCode != 401)
            {
                BingSearchKeyEntry.BackgroundColor = Color.Green;
                AppConstants.BingWebSearchApiKey = BingSearchKeyEntry.Text;
                bingSearchKeyWorks = true;
            }
            else
            {
                BingSearchKeyEntry.BackgroundColor = Color.Red;
                bingSearchKeyWorks = false;
            }
        }


        async void TryToAddKeys(object sender, EventArgs e)
        {
            if (!computerVisionKeyWorks)
                await CheckComputerVisionKey();
            if (!bingSearchKeyWorks)
                await CheckBingSearchKey();

            if (bingSearchKeyWorks && computerVisionKeyWorks)
            {
                await Navigation.PopModalAsync();
            }
            else
            {
                await DisplayAlert("Error","One or more of your keys are invalid.  Please update them and try again", "OK");
            }
        }
    }
}