# Sample - Visual Search App with Cognitive Services

This sample illustrates how the [Azure Computer Vision API](https://azure.microsoft.com/en-us/services/cognitive-services/computer-vision/) and [Bing Web Search API](https://azure.microsoft.com/en-us/services/cognitive-services/bing-web-search-api/) can be used together to build a simple visual search application.  

## The Sample
This sample is a Xamarin.Forms application which uses the Bing Computer Vision and Bing Web Search RESTful APIs to parse text from images and then query that text on Bing.  It can import photos or capture them with the OS-default camera on Android, iOS, or the Universal Windows Platform.  

This sample, along with its implementation and deployment are discussed in much greater detail in [this guide](https://github.com/Aristoddle/azure-docs-pr/blob/master/articles/cognitive-services/Bing-Web-Search/computer-vision-web-search-tutorial.md).


### Build the sample

1. Ensure that you've installed the **Mobile development with .NET** package from the Visual Studio Installer.

2. Start Microsoft Visual Studio 2017 and select `File > Open >
    Project/Solution`.
    
3. Navigate to the folder where you cloned this repository.

4. Open the Visual Studio Solution (.sln) file `VisualSearchApp.sln` using your Visual Studio installation.  It may take a few minutes for the project to initialize.

5. Open the NuGet Package Manager (right click your solution in the solution explorer, and select `Manage NuGet Packages For Solution`)

6. Install the **Xamarin Media Plugin** (Xam.Plugin.Media) and **Json.NET** (Newtonsoft.Json) packages.

7. Select your target platform from the ribbon menu at the top of your Visual Studio window (see guide for more details).

8. Build and run the sample from this same ribbon menu.

### Run the sample
After building the sample, you should be taken to a screen where you are asked to input your Bing Web Search and Computer Vision API Keys.  To attain 30-day trial keys to these APIs, see [this page](https://azure.microsoft.com/en-us/try/cognitive-services/).  For more information about attaining keys for professional use, see [Pricing](https://azure.microsoft.com/en-us/pricing/calculator/).

After inputting the keys, you should be taken to a screen where you can import or capture a photo, and then feed that photo to the relevant OCR 
  

## Contributing
This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (for example, label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## License

## Developer Code of Conduct


https://azure.microsoft.com/en-us/resources/samples/cognitive-speech-stt-windows/
