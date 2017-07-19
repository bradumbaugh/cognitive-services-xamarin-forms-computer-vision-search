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