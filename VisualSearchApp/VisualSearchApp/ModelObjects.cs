using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace VisualSearchApp
{
    public class SearchTagsObj
    {
        public string name { get; set; }
        public string content { get; set; }
    }

    public class WebValueObject
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string displayUrl { get; set; }
        public string snippet { get; set; }
        public string dateLastCrawled { get; set; }
        public List<SearchTagsObj> searchTags { get; set; }
    }

    public class WebResultsList
    {
        public string name { get; set; }
        public string webSearchUrl { get; set; }
        public int totalEstimatedMatches { get; set; }
        public ObservableCollection<WebValueObject> value { get; set; }
        public bool someResultsRemoved { get; set; }
    }

    public class ApiKeys
    {
        public static string computerVisionKey = String.Empty;
        public static string bingSearchKey = String.Empty;
    }
}
