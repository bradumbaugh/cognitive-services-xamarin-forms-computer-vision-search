using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace VisualSearchApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            //ApiKeys.computerVisionKey = <Add Your Cognitive Services Vision API Key Here>;
            //ApiKeys.bingSearchKey = <Add Your Bing Search API Key Here>;
            ApiKeys.computerVisionKey = "ae8f05ea4ff246fdbcf94e5e16d1e3d1";
            ApiKeys.bingSearchKey = "a641845d1758470ca2c92b138c6ee85c";

            MainPage = new NavigationPage(new OcrSelectPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}