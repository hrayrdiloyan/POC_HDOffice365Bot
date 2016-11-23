using System.Collections.Generic;
namespace HDOffice365Bot
{
    public class Document
    {
        public string language { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }
    public class RootObject
    {
        public List<Document> documents { get; set; }
    }
}