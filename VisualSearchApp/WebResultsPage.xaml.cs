using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VisualSearchApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WebResultsPage : ContentPage
    {
        // The root URI for the v5 Bing Search API
        const string SearchUri = "https://api.cognitive.microsoft.com/bing/v5.0/search?";
        ObservableCollection<WebValueObject> Values { get; set; }
        static HttpClient SearchApiClient = new HttpClient();

        private string queryTerm;
        private bool isFirstPageLoad = true;

        public WebResultsPage(string queryTerm)
        {
            InitializeComponent();
            this.queryTerm = queryTerm;
            SearchApiClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiKeys.bingSearchKey);
        }

        // Overrides Page.OnAppearing 
        // If this is the first time that the page is loaded, this will call the Bing Search API to find results for the given string
        protected override async void OnAppearing()
        {
            if (isFirstPageLoad)
            {
                WebResultsList webResults = await GetQueryResults();
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                DataTable.IsVisible = true;

                if (webResults.totalEstimatedMatches != 0)
                {
                    DataTable.ItemsSource = webResults.value;
                    Values = webResults.value;
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("Error", "No results found.", "OK");
                        await Task.Delay(TimeSpan.FromSeconds(0.1d));
                        await Navigation.PopAsync(true);
                    });
                }
            }
            this.isFirstPageLoad = false;
        }

        // Calls the Bing Web Search API to find query results for a given string
        async Task<WebResultsList> GetQueryResults()
        {
            WebResultsList webResults = new WebResultsList { name = queryTerm };
            try
            {
                var queryString = System.Net.WebUtility.UrlEncode(queryTerm);

                /* Here we encode the URL that will be used for the GET request to find query results.  Its arguments are as follows:
                 * [count=20] This sets the number of webpage objects returned at "$.webpages" in the JSON response.  Currently, the API aks for 20 webpages in the response
                 * [mkt=en-US] This sets the market where the results come from.  Currently, the API looks for english results based in the United States.
                 * [q=queryString] This sets the string queried using the Search API.   
                 * [responseFilter=Webpages] This filters the response to only include Webpage results.  This tag can take a comma seperated list of response types that you are looking for.  If left blank, all responses (webPages, News, Videos, etc) are returned.
                 * [setLang=en] This sets the languge for user interface strings.  To learn more about UI strings, check the Web Search API reference.
                 * 
                 * [API Reference] https://docs.microsoft.com/en-us/rest/api/cognitiveservices/bing-web-api-v5-reference
                 */
                string uri = SearchUri + $"count=20&mkt=en-US&q={queryString}&responseFilter=Webpages&setLang=en";

                HttpResponseMessage httpResponseMessage = await SearchApiClient.GetAsync(uri);
                var responseContentString = await httpResponseMessage.Content.ReadAsStringAsync();

                JObject json = JObject.Parse(responseContentString);
                JToken resultBlock = json.SelectToken("$.webPages");
                webResults = JsonConvert.DeserializeObject<WebResultsList>(resultBlock.ToString());
            }
            catch
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "Error fetching results.", "OK");
                    await Task.Delay(TimeSpan.FromSeconds(0.1d));
                });
            }
            return webResults;
        }

        // Called upon selecting an item from the ListView
        async void ItemSelectedEventHandler(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) { return; }
            //ItemSelectedEventHandler is called on both selection and deselection; if null (i.e. it's a deselect) do nothing
            WebView webView = new WebView
            {
                Source = new UrlWebViewSource { Url = ((WebValueObject)e.SelectedItem).url },
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            await Navigation.PushAsync(new ContentPage
            {
                Content = webView,
                Title = ((WebValueObject)e.SelectedItem).name
            });

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}