using System;

namespace CinegyType.RSS.Controller.Model
{
    public class PlayoutSettings
    {
        public int Id { get; set; }
        public string Hostname { get; set; }
    }

    public class RssSettings
    {
        public string Url { get; set; }
        public DateTime StartPubDate { get; set; }

        public string DateTimeFormat { get; set; }
    }

    public class Settings
    {
        public Settings(RssSettings rssSettings, string templatePath, string targetVariable, PlayoutSettings playout)
        {
            RssSettings = rssSettings;

            TemplatePath = templatePath;
            TargetVariableName = targetVariable;

            Playout = playout;
        }

        public RssSettings RssSettings { get; set; }
        public string TemplatePath { get; }
        public string TargetVariableName { get; }
        public PlayoutSettings Playout { get; }
    }
}