using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TextEditor
{
    public class TimeMatch
    {
        public int Line { get; set; }
        public int Position { get; set; }
        public string Value { get; set; }
    }

    public class RegexSearcher
    {
        private static readonly Regex timeRegex = new Regex(@"\b([01]?\d|2[0-3]):[0-5]\d:[0-5]\d\b");

        public List<TimeMatch> FindTimes(string text)
        {
            var results = new List<TimeMatch>();
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                foreach (Match match in timeRegex.Matches(lines[i]))
                {
                    results.Add(new TimeMatch
                    {
                        Line = i + 1,
                        Position = match.Index + 1,
                        Value = match.Value
                    });
                }
            }

            return results;
        }
    }
}
