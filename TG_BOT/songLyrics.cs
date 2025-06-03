using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_BOT
{
    public class songLyrics
    {
        public class Rootobject
        {
            public Lyrics lyrics { get; set; }
            public Colors colors { get; set; }
            public bool hasVocalRemoval { get; set; }
        }

        public class Lyrics
        {
            public string syncType { get; set; }
            public Line[] lines { get; set; }
            public string provider { get; set; }
            public string providerLyricsId { get; set; }
            public string providerDisplayName { get; set; }
            public string syncLyricsUri { get; set; }
            public bool isDenseTypeface { get; set; }
            public object[] alternatives { get; set; }
            public string language { get; set; }
            public bool isRtlLanguage { get; set; }
            public string capStatus { get; set; }
            public bool isSnippet { get; set; }
            public Previewline[] previewLines { get; set; }
        }

        public class Line
        {
            public string startTimeMs { get; set; }
            public string words { get; set; }
            public object[] syllables { get; set; }
            public string endTimeMs { get; set; }
        }

        public class Previewline
        {
            public string startTimeMs { get; set; }
            public string words { get; set; }
            public object[] syllables { get; set; }
            public string endTimeMs { get; set; }
        }

        public class Colors
        {
            public int background { get; set; }
            public int text { get; set; }
            public int highlightText { get; set; }
        }

    }
}
