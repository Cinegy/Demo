using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using CinegyType.RSS.Controller.Model;

namespace CinegyType.RSS.Controller
{
    public class Controller : IDisposable
    {
        private readonly int _interval;
        private readonly Timer _timer;

        private DateTime _dateLastModified;
        private bool _isUpdating;

        public Controller(Settings settings, int interval)
        {
            _interval = interval;
            _dateLastModified = settings.RssSettings.StartPubDate;

            _timer = new Timer(TimerCallback, settings, 0, Timeout.Infinite); // init timer
        }

        public void Dispose()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();
        }

        private void TimerCallback(object state)
        {
            if (_isUpdating) return; // prevent update callback intersection

            _isUpdating = true;

            try
            {
                UpdateCallback((Settings) state);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            _timer.Change(_interval, Timeout.Infinite);
            _isUpdating = false;
        }

        private void UpdateCallback(Settings settings)
        {
            XmlNodeList nodes;
            using (var reader = XmlReader.Create(settings.RssSettings.Url))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(reader);
                nodes = xmlDoc.SelectNodes("rss/channel/item");
            }

            var rssList = new List<RssItem>();

            if (nodes == null || nodes.Count == 0)
            {
                Console.WriteLine("RSS-feed has no items");
                return;
            }

            foreach (XmlNode node in nodes)
            {
                if (node == null) continue;

                try
                {
                    var rssItem = new RssItem(settings.RssSettings.DateTimeFormat)
                    {
                        Title = node["title"].InnerText,
                        Link = node["link"].InnerText,
                        Content = node["description"].InnerText,
                        PublishDateStr = node["pubDate"].InnerText,
                    };

                    rssList.Add(rssItem);
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} - Failed adding RSS-item.\nInnerXml: {1}.\nError: {2}",
                        DateTime.Now.ToLongTimeString(),
                        node.InnerXml,
                        e.Message);
                }
            }

            var orderedRssList = rssList.Where(i => i.PublishDate >= _dateLastModified).OrderBy(i => i.PublishDate);
            var lastItem = orderedRssList.FirstOrDefault();
            if (lastItem == null) return;

            if (_dateLastModified != lastItem.PublishDate)
            {
                Console.WriteLine("{0} - New RSS-item! Timestamp: {1}",
                    DateTime.Now.ToLongTimeString(),
                    lastItem.PublishDate);

                _dateLastModified = lastItem.PublishDate;

                var templateVariables = TypeStudioHelper.GetTemplateVariables(settings.TemplatePath);
                var targetTextVariable =
                    templateVariables.FirstOrDefault(t => t.Type == TypeStudioHelper.VariableType.Text &&
                                                          t.Name.Equals(settings.TargetVariableName, StringComparison.InvariantCultureIgnoreCase));

                if (targetTextVariable != null)
                {
                    targetTextVariable.Value = lastItem.Content;
                    Console.WriteLine("{0} - Target text-variable was modified by RSS-item published {1}",
                        DateTime.Now.ToLongTimeString(),
                        lastItem.PublishDate);
                }
                else
                {
                    Console.WriteLine("{0} - No target text-variable has been found in file {1}",
                        DateTime.Now.ToLongTimeString(),
                        settings.TemplatePath);
                    return;
                }

                var request = TypeStudioHelper.CreateXmlRequest(templateVariables);
                if (TypeStudioHelper.SendPlayoutRequest(settings.Playout, request))
                {
                    Console.WriteLine("{0} - Success on sending request. Server: {1}, instance: {2}.",
                        DateTime.Now.ToLongTimeString(),
                        settings.Playout.Hostname,
                        settings.Playout.Id);
                }
            }
        }

    }
}