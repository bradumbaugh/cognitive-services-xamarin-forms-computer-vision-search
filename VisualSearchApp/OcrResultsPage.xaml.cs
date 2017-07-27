using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace VisualSearchApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OcrResultsPage : ContentPage
    {
        #region fields
        HttpClient visionApiClient;
        bool isHandwritten;
        byte[] photo;
        ObservableCollection<string> values;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:VisualSearchApp.OcrResultsPage"/> class.
        /// </summary>
        /// <param name="photo">Photo to find text in</param>
        /// <param name="isHandwritten">Indicates whether the text in the photo is handwritten</param>
        public OcrResultsPage(byte[] photo, bool isHandwritten)
        {
            InitializeComponent();
            this.photo = photo;
            this.isHandwritten = isHandwritten;
            visionApiClient = new HttpClient();
            visionApiClient.DefaultRequestHeaders.Add(AppConstants.OcpApimSubscriptionKey, AppConstants.ComputerVisionApiKey);
        }
        #endregion

        #region overrides
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (values == null)
            {
                await LoadData();
            }
        }
		#endregion

		#region methods
        async Task LoadData()
        {
			// Try loading the results, show error message if necessary.
			Boolean error = false;
			try
			{
				values = isHandwritten ? await FetchHandwrittenWordList() : await FetchPrintedWordList();
			}
			catch
			{
				error = true;
			}

			// Hide the spinner, show the table
			LoadingIndicator.IsVisible = false;
			LoadingIndicator.IsRunning = false;
			DataTable.IsVisible = true;

			if (error)
			{
				await ErrorAndPop("Error", "Error fetching words", "OK");
			}
            else if (values.Count() > 0)
			{
				DataTable.ItemsSource = values.Distinct();
			}
			else
			{
				await ErrorAndPop("Error", "No words found", "OK"); ;
			}
        }

		/// <summary>
		/// Handles the selection of an item in the list
		/// </summary>
		async void ListItemSelectionEventHandler(object sender, SelectedItemChangedEventArgs e)
        {
            // ItemSelected is called on both selection and deselection; if null 
            // (i.e. it's a deselect) do nothing
            if (e.SelectedItem == null) { return; }

            // Show the WebResultsPage for the selected item
            await Navigation.PushAsync(new WebResultsPage((string)e.SelectedItem));

            // Deselect the item
            ((ListView)sender).SelectedItem = null;
        }

		/// <summary>
		/// Uses the Computer Vision API to parse printed text from the photo 
        /// set in the constructor
		/// </summary>
		async Task<ObservableCollection<string>> FetchPrintedWordList()
        {
            ObservableCollection<string> wordList = new ObservableCollection<string>();
            if (photo != null)
            {
                HttpResponseMessage response = null;
                using (var content = new ByteArrayContent(photo))
                {
                    // The media type of the body sent to the API. 
                    // "application/octet-stream" defines an image represented 
                    // as a byte array
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await visionApiClient.PostAsync(AppConstants.ComputerVisionApiOcrUrl, content);
                }

                string ResponseString = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(ResponseString);

                // Here, we pull down each "line" of text and then join it to 
                // make a string representing the entirety of each line.  
                // In the Handwritten endpoint, you are able to extract the 
                // "line" without any further processing.  If you would like 
                // to simply get a list of all extracted words,* you can do 
                // this with:
                //
                // json.SelectTokens("$.regions[*].lines[*].words[*].text) 
                IEnumerable<JToken> lines = json.SelectTokens("$.regions[*].lines[*]");
                if (lines != null)
                {
                    foreach (JToken line in lines)
                    {
                        IEnumerable<JToken> words = line.SelectTokens("$.words[*].text");
                        if (words != null)
                        {
                            wordList.Add(string.Join(" ", words.Select(x => x.ToString())));
                        }
                    }
                }
            }
            return wordList;
        }

        /// <summary>
        /// Shows an error message, navigates back after it is dismissed.
        /// </summary>
		protected async Task ErrorAndPop(string title, string text, string button)
		{
			await DisplayAlert(title, text, button);
			await Task.Delay(TimeSpan.FromSeconds(0.1d));
			await Navigation.PopAsync(true);
		}

		/// <summary>
		/// Uses the Computer Vision API to parse handwritten text from the 
        /// photo set in the constructor
		/// </summary>
		async Task<ObservableCollection<string>> FetchHandwrittenWordList()
        {
            ObservableCollection<string> wordList = new ObservableCollection<string>();
            if (photo != null)
            {
                // Make the POST request to the handwriting recognition URL
                HttpResponseMessage response = null;
                using (var content = new ByteArrayContent(photo))
                {
                    // The media type of the body sent to the API. 
                    // "application/octet-stream" defines an image represented 
                    // as a byte array
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await visionApiClient.PostAsync(AppConstants.ComputerVisionApiHandwritingUrl, content);
                }

                // Fetch results
                IEnumerable<string> operationLocationValues;
                string statusUri = string.Empty;
                if (response.Headers.TryGetValues("Operation-Location", out operationLocationValues))
                {
                    statusUri = operationLocationValues.FirstOrDefault();

                    // Ping status URL, wait for processing to finish 
                    JObject obj = await FetchResultFromStatusUri(statusUri);
					IEnumerable<JToken> strings = obj.SelectTokens("$.recognitionResult.lines[*].text");
					foreach (string s in strings)
					{
						wordList.Add((string)s);
					}
                }
            }
            return wordList;
        }

		/// <summary>
		/// Takes in the url to check for handwritten text parsing results, and 
        /// pings it per second until processing is finished. 
        /// </summary>
		async Task<JObject> FetchResultFromStatusUri(string statusUri)
        {
            JObject obj = null;
            int timeoutcounter = 0;
            HttpResponseMessage response = await visionApiClient.GetAsync(statusUri);
            string responseString = await response.Content.ReadAsStringAsync();
            obj = JObject.Parse(responseString);
			while ((!((string)obj.SelectToken("status")).Equals("Succeeded")) && (timeoutcounter++ < 60))
			{
				await Task.Delay(1000);
				response = await visionApiClient.GetAsync(statusUri);
				responseString = await response.Content.ReadAsStringAsync();
				obj = JObject.Parse(responseString);
			} 
            return obj;
        }
        #endregion
    }
}