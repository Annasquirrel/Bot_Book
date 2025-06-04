using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot_Book.Models
{
    public class Book
    {
        public class BookRoot
        {
            public string kind { get; set; }
            public int totalItems { get; set; }
            public List<Item> items { get; set; }
        }

        public class Item
        {
            public string kind { get; set; }
            public string id { get; set; }
            public string etag { get; set; }
            public string selfLink { get; set; }
            public VolumeInfo volumeInfo { get; set; }
            public AccessInfo accessInfo { get; set; }

        }

        public class VolumeInfo
        {
            public string title { get; set; }
            public List<string> authors { get; set; }
            public string publisher { get; set; }
            public string publishedDate { get; set; }
            public string description { get; set; }
            public int? pageCount { get; set; }
            public string printType { get; set; }
            public List<string> categories { get; set; }
            public string maturityRating { get; set; }
            public string language { get; set; }
        }
        public class AccessInfo
        {
            public string country { get; set; }
            public string viewability { get; set; }
            public bool embeddable { get; set; }
            public bool publicDomain { get; set; }
            public string textToSpeechPermission { get; set; }
            public Epub epub { get; set; }
            public Pdf pdf { get; set; }
            public string webReaderLink { get; set; }
            public string accessViewStatus { get; set; }
            public bool quoteSharingAllowed { get; set; }
        }

        public class Epub
        {
            public bool isAvailable { get; set; }
            public string acsTokenLink { get; set; }
        }

        public class Pdf
        {
            public bool isAvailable { get; set; }
            public string acsTokenLink { get; set; }
        }
    }
}
