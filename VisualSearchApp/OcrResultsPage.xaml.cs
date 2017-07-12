using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class OcrResultsPage : ContentPage
    {
        private static HttpClient VisionApiClient = new HttpClient();
        private bool isHandwritten;
        private byte[] photo;

        private bool isFirstPageLoad = true;

        /* This is the url that will be passed into the POST request for parsing printed text.  It's parameters are as follows:
         * [language = en] Tells the system to look for english printed text.  Other options are unk (unknown), and a series of other languages listed on the API reference site.
         * [detectOrientation = True] This allows the system to attempt to rotate the photo to improve parse results.
         * 
         * [Note] This API is only available on Azure servers in the following domains: westus, eastus2, westcentralus, westeurope, souteheastasia. 
         * [API Reference] https://westus.dev.cognitive.microsoft.com/docs/services/56f91f2d778daf23d8ec6739/operations/56f91f2e778daf14a499e1fc
         */
        public const string ocrUri = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/ocr?language=en&detectOrientation=true";

        /* This is the url that will be passed into the POST request for parsing handwritten text.  Its parameters are as follows:
         * [handwriting = True] This tells the system to try to parse handwritten text from the image.  If set to False, this API will perform processing similar to the print OCR endpoint. 
         * 
         * [Note] This API is only available on Azure servers in the following domains: westus, eastus2, westcentralus, westeurope, souteheastasia. 
         * [API Reference] https://westus.dev.cognitive.microsoft.com/docs/services/56f91f2d778daf23d8ec6739/operations/587f2c6a154055056008f200
         */
        public const string handwritingUri = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/recognizeText?handwriting=true";

        // Constructor method; 
        // byte[] photo: the photo taken or imported from the OcrSelectPage
        // bool isHandwritten: a flag determining whether to perform standard or Handwritten OCR
        public OcrResultsPage(byte[] photo, bool isHandwritten)
        {
            InitializeComponent();
            this.photo = photo;
            this.isHandwritten = isHandwritten;
            //Computer vision requires a subscription key attached to the 
            VisionApiClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiKeys.computerVisionKey);
        }

        // Overrides Page.OnAppearing
        // If this is the first time that the page is loaded, this will call the appropriate Microsoft Computer Vision API to extract the printed or handwritten text from the given image
        protected override async void OnAppearing()
        {
            if (isFirstPageLoad)
            {
                ObservableCollection<string> Values = isHandwritten ? await FetchHandwrittenWordList() : await FetchPrintedWordList();

                DataTable.ItemsSource = Values.Distinct();
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                DataTable.IsVisible = true;
            }
            this.isFirstPageLoad = false;
        }

        // Handles the selection of an item in the list; defined in OcrResultsPage.xaml
        async void ListItemSelectionEventHandler(object sender, SelectedItemChangedEventArgs e)
        {
            //ItemSelected is called on both selection and deselection; if null (i.e. it's a deselect) do nothing
            if (e.SelectedItem == null) { return; }
            await Navigation.PushAsync(new WebResultsPage((string)e.SelectedItem));
            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        // Uses the Microsoft Computer Vision OCR API to parse printed text from the photo set in the constructor
        async Task<ObservableCollection<string>> FetchPrintedWordList()
        {
            ObservableCollection<string> wordList = new ObservableCollection<string>();
            if (photo != null)
            {
                HttpResponseMessage response = null;
                using (var content = new ByteArrayContent(photo))
                {
                    // The media type of the body sent to the API. "application/octet-stream" defines an image represented as a byte array
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await VisionApiClient.PostAsync(ocrUri, content);
                }

                if ((int)response?.StatusCode == 200)
                {
                    string ResponseString = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(ResponseString);
                    /*Here, we pull down each "line" of text and then join it to make a string representing the 
                    * entirety of each line.  In the Handwritten endpoint, you are able to extract the "line" 
                    * without any further processing.  If you would like to simply get a list of all extracted words,* you can do this with json.SelectTokens("$.regions[*].lines[*].words[*].text) 
                    */
                    IEnumerable<JToken> lines = json.SelectTokens("$.regions[*].lines[*]");
                    foreach (JToken line in lines)
                    {
                        IEnumerable<JToken> words = line.SelectTokens("$.words[*].text");
                        wordList.Add(string.Join(" ", words.Select(x=> x.ToString())));
                    }
                }
            }

            if (!wordList.Any())
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "No words found.", "OK");
                    await Task.Delay(TimeSpan.FromSeconds(0.1d));
                    await Navigation.PopAsync(true);
                });
            }

            return wordList;
        }

        // Uses the Microsoft Computer Vision Handwritten OCR API to parse handwritten text from the photo set in the constructor
        async Task<ObservableCollection<string>> FetchHandwrittenWordList()
        {
            ObservableCollection<string> wordList = new ObservableCollection<string>();
            if (photo != null)
            {
                HttpResponseMessage response = null;
                using (var content = new ByteArrayContent(photo))
                {
                    // The media type of the body sent to the API. "application/octet-stream" defines an image represented as a byte array
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await VisionApiClient.PostAsync(handwritingUri, content);
                }
                if ((int)response?.StatusCode == 202)
                {
                    IEnumerable<string> values;
                    string statusUri = string.Empty;
                    if (response.Headers.TryGetValues("Operation-Location", out values))
                    {
                        statusUri = values.FirstOrDefault();

                        // Open a new thread to intermittently ping the statusUri endpoint and wait for processing to finish
                        JObject obj = await FetchResultFromStatusUri(statusUri);

                        IEnumerable<JToken> strings = obj.SelectTokens("$.recognitionResult.lines[*].text");
                        foreach (string s in strings)
                        {
                            wordList.Add((string)s);
                        }
                    }
                }
            }
            if (!wordList.Any())
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "No words found.", "OK");
                    await Navigation.PopAsync(true);
                });
            }

            return wordList;
        }

        // Takes in the url to check for handwritten text parsing results, and pings it per second until processing is finished
        // Returns the JObject holding data for a successful parse
        async Task<JObject> FetchResultFromStatusUri(string statusUri)
        {
            JObject obj = null;
            int timeoutcounter = 0;
            HttpResponseMessage response = await VisionApiClient.GetAsync(statusUri);
            string responseString = await response.Content.ReadAsStringAsync();
            obj = JObject.Parse(responseString);

            while ((!((string)obj.SelectToken("status")).Equals("Succeeded")) && (timeoutcounter++ < 60))
            {
                await Task.Delay(1000);
                response = await VisionApiClient.GetAsync(statusUri);
                responseString = await response.Content.ReadAsStringAsync();
                obj = JObject.Parse(responseString);
            }
            return obj;
        }
    }
}