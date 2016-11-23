using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HDOffice365Bot
{
    public class DocumentReturn
    {
        public List<string> keyPhrases { get; set; }
        public string id { get; set; }
    }

    public class ReturnRootObJect
    {
        public List<DocumentReturn> documents { get; set; }
        public List<object> errors { get; set; }
    }
}