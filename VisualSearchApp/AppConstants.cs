namespace VisualSearchApp
{
    public static class AppConstants
    {
        #region bing web search api
        /// <summary>
        /// Header parameter used to provide the authentication key for all API calls
        /// </summary>
        public const string OcpApimSubscriptionKey = "Ocp-Apim-Subscription-Key";

        /// <summary>
        /// Url of the Bing Web Search API
        /// </summary>
        public const string BingWebSearchApiUrl = "https://api.cognitive.microsoft.com/bing/v5.0/search?";

        /// <summary>
        /// User's API key for the Bing Web Search API
        /// </summary>
        public static string BingWebSearchApiKey = "478a1e606e31481bb9a6d83c3d2d3cf1";
        #endregion

        #region computer vision api
        /// <summary>
        /// Url of the Computer Vision API OCR method for printed text
        /// [language=en] Text in image is in English. 
        /// [detectOrientation=true] Improve results by detecting orientation
        /// </summary>
        public const string ComputerVisionApiOcrUrl = "https://westus.api.cognitive.microsoft.com/vision/v1.0/ocr?language=en&detectOrientation=true";

        /// <summary>
        /// Url of the Computer Vision API handwritten text recognition method
        /// [handwriting=true] Text in image is handwritten. Set to false for printed text.
        /// </summary>
		public const string ComputerVisionApiHandwritingUrl = "https://westus.api.cognitive.microsoft.com/vision/v1.0/recognizeText?handwriting=true";

        /// <summary>
        /// User's API Key for the Computer Vision API
        /// </summary>
		public static string ComputerVisionApiKey = "d732b769b1cd48d58b84e7e02bac673e";
        #endregion
    }
}
