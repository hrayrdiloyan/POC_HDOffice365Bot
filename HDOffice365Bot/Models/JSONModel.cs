using System.Collections.Generic;
namespace HDOffice365Bot
{
    /// <summary>
    ///  Represents C# classes for post payload of cognitive services.
    /// </summary>
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