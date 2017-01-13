using System;
using System.Globalization;

namespace CinegyType.RSS.Controller.Model
{
    public class RssItem
    {
        private readonly string _datetimeFormat;

        public RssItem(string datetimeFormat)
        {
            _datetimeFormat = datetimeFormat;
        }

        public string Link { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public DateTime PublishDate
        {
            get
            {
                return DateTime.ParseExact(PublishDateStr,
                    _datetimeFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None);
            }
        }

        public string PublishDateStr { get; set; }
    }
}