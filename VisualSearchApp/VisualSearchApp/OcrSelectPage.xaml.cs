using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.IO;

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;

namespace VisualSearchApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OcrSelectPage : TabbedPage
    {
        private bool isFirstPageLoad = true;

        public OcrSelectPage()
        {
            InitializeComponent();
            CrossMedia.Current.Initialize();
        }

        // Overrides Page.OnAppearing
        // If no keys are found in the App.xaml.cs file, a window will appear for manual entry
        protected override async void OnAppearing()
        {
            if (isFirstPageLoad)
            {
                if ((!ApiKeys.computerVisionKey.Any()) || (!ApiKeys.bingSearchKey.Any()))
                {
                    await Navigation.PushModalAsync(new AddKeysPage());
                }
            }
            this.isFirstPageLoad = false;
        }

        // Called when Take Photo is pressed from the standard and handwritten OCR page
        async void TakePhotoButtonClickEventHandler(object sender, EventArgs e)
        {
            byte[] photoByteArray = null;

            try
            {
                photoByteArray = await TakePhoto();
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message);
            }

            if (photoByteArray != null)
            {
                bool handwritten = (sender == ButtonTakeHandwritten);
                await Navigation.PushAsync(new OcrResultsPage(photoByteArray, handwritten));
            }
        }

        // Uses the Xamarin Media Plugin to import photos from the native photo library
        async void ImportPhotoButtonClickEventHandler(object sender, EventArgs e)
        {
            MediaFile photoMediaFile = null;
            byte[] photoByteArray = null;

            try
            {
                photoMediaFile = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    PhotoSize = PhotoSize.Medium,
                });

                photoByteArray = MediaFileToByteArray(photoMediaFile);
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message);
            }

            if (photoByteArray != null)
            {
                bool handwritten = (sender == ButtonImportHandwritten);
                await Navigation.PushAsync(new OcrResultsPage(photoByteArray, handwritten));
            }
        }

        // Uses the Xamarin Media Plugin to take photos using the native camera application
        async Task<byte[]> TakePhoto()
        {
            MediaFile photoMediaFile = null;
            byte[] photoByteArray = null;

            if (CrossMedia.Current.IsCameraAvailable)
            {
                var mediaOptions = new StoreCameraMediaOptions
                {
                    PhotoSize = PhotoSize.Medium,
                    AllowCropping = true,
                    SaveToAlbum = true,
                    Name = $"{DateTime.UtcNow}.jpg"
                };
                photoMediaFile = await CrossMedia.Current.TakePhotoAsync(mediaOptions);
                photoByteArray = MediaFileToByteArray(photoMediaFile);
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => {
                    DisplayAlert("Error", "No camera found", "OK");
                });
            }

            return photoByteArray;
        }

        byte[] MediaFileToByteArray(MediaFile photoMediaFile)
        {
            using (var memStream = new MemoryStream())
            {
                photoMediaFile.GetStream().CopyTo(memStream);
                return memStream.ToArray();
            }
        }
            
    }
}