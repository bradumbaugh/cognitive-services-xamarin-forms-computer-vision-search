using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VisualSearchApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WebResultsPage : ContentPage
    {
        #region fields
        ObservableCollection<WebValueObject> values;
        string queryTerm;
        HttpClient searchApiClient;
        #endregion

        #region constructors
        public WebResultsPage(string queryTerm)
        {
            InitializeComponent();
            this.queryTerm = queryTerm;
            searchApiClient = new HttpClient();
            searchApiClient.DefaultRequestHeaders.Add(AppConstants.OcpApimSubscriptionKey, AppConstants.BingWebSearchApiKey);
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
        protected async Task LoadData()
        {
            // Try loading the results, show error message if necessary.
            WebResultsList webResults = null;
            Boolean error = false;
            try
            {
                webResults = await GetQueryResults();
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
                await ErrorAndPop("Error", "Error fetching results", "OK");
            }
            else if (webResults?.totalEstimatedMatches > 0)
            {
                values = webResults.value;
                DataTable.ItemsSource = values;
            }
            else
            {
                await ErrorAndPop("Error", "No results found", "OK"); ;
            }
        }

        protected async Task ErrorAndPop(string title, string text, string button)
        {
            await DisplayAlert(title, text, button);
            await Task.Delay(TimeSpan.FromSeconds(0.1d));
            await Navigation.PopAsync(true);
        }

        async Task<WebResultsList> GetQueryResults()
        {
            // URL-encode the query term
            var queryString = System.Net.WebUtility.UrlEncode(queryTerm);

            // Here we encode the URL that will be used for the GET request to 
            // find query results.  Its arguments are as follows:
            // 
            // - [count=20] This sets the number of webpage objects returned at 
            //   "$.webpages" in the JSON response.  Currently, the API aks for 
            //   20 webpages in the response
            //
            // - [mkt=en-US] This sets the market where the results come from.
            //   Currently, the API looks for english results based in the 
            //   United States.
            //
            // - [q=queryString] This sets the string queried using the Search 
            //   API.   
            //
            // - [responseFilter=Webpages] This filters the response to only 
            //   include Webpage results.  This tag can take a comma seperated 
            //   list of response types that you are looking for.  If left 
            //   blank, all responses (webPages, News, Videos, etc) are 
            //   returned.
            //
            // - [setLang=en] This sets the languge for user interface strings. 
            //   To learn more about UI strings, check the Web Search API 
            //   reference.
            //
            // - [API Reference] https://docs.microsoft.com/en-us/rest/api/cognitiveservices/bing-web-api-v5-reference
            string uri = AppConstants.BingWebSearchApiUrl + $"count=20&mkt=en-US&q={queryString}&responseFilter=Webpages&setLang=en";

            // Make the HTTP Request
            WebResultsList webResults = null;
            HttpResponseMessage httpResponseMessage = await searchApiClient.GetAsync(uri);
            var responseContentString = await httpResponseMessage.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseContentString);
            JToken resultBlock = json.SelectToken("$.webPages");
            if (resultBlock != null)
            {
                webResults = JsonConvert.DeserializeObject<WebResultsList>(resultBlock.ToString());
            }
            return webResults;
        }

        async void ItemSelectedEventHandler(object sender, SelectedItemChangedEventArgs e)
        {
			// ItemSelectedEventHandler is called on both selection and 
			// deselection; if null (i.e. it's a deselect) do nothing
			if (e.SelectedItem == null) { return; }

            // Create the WebView
            WebView webView = new WebView
            {
                Source = new UrlWebViewSource { Url = ((WebValueObject)e.SelectedItem).url },
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            // Display the WebView
            await Navigation.PushAsync(new ContentPage
            {
                Content = webView,
                Title = ((WebValueObject)e.SelectedItem).name
            });

            // Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
        #endregion
    }
}