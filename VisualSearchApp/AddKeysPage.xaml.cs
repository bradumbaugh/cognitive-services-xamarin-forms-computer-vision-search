using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VisualSearchApp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddKeysPage : ContentPage
	{
        #region fields
        private bool computerVisionKeyWorks = false;
        private bool bingSearchKeyWorks = false;
        #endregion

        #region constructors
        public AddKeysPage ()
		{
			InitializeComponent();
		}
        #endregion

        #region methods

        /// <summary>
        /// Send a test POST request to see if the Vision API Key is functional
        /// </summary>
        async Task CheckComputerVisionKey(object sender = null, EventArgs e = null)
        {
            // Empty image for test OCR request
            byte[] emptyImage = new byte[10];

            HttpResponseMessage response;
            HttpClient VisionApiClient = new HttpClient();

            VisionApiClient.DefaultRequestHeaders.Add(AppConstants.OcpApimSubscriptionKey, ComputerVisionKeyEntry.Text);

            try
            {
                using (var content = new ByteArrayContent(emptyImage))
                {
                    // The media type of the body sent to the API. "application/octet-stream" defines an image represented as a byte array
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await VisionApiClient.PostAsync(AppConstants.ComputerVisionApiOcrUrl, content);
                }

                if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
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
            catch( Exception exception )
            {
                ComputerVisionKeyEntry.BackgroundColor = Color.Red;
                Console.WriteLine($"ERROR: {exception.Message}");
            }

        }

        /// <summary>
        /// Send a test GET request to see if the Bing Search API key is functional
        /// </summary>
        async Task CheckBingSearchKey(object sender = null, EventArgs e = null)
        {
            HttpResponseMessage response;
            HttpClient SearchApiClient = new HttpClient();

            SearchApiClient.DefaultRequestHeaders.Add(AppConstants.OcpApimSubscriptionKey, BingSearchKeyEntry.Text);

            try
            {
                response = await SearchApiClient.GetAsync(AppConstants.BingWebSearchApiUrl + "q=test");

                if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
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
            catch( Exception exception )
            {
                BingSearchKeyEntry.BackgroundColor = Color.Red;
                Console.WriteLine($"ERROR: {exception.Message}");
            }
        }

        /// <summary>
        /// Check both API keys.
        /// </summary>
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
                await DisplayAlert("Error", "One or more of your keys are invalid. Please update them and try again", "OK");
            }
        }
        #endregion
    }
}