using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace StatsSite.Models {
    public class StatRepository {

        private StatServiceDataContext db = new StatServiceDataContext();

        public class StatisticRecord {
            public string Username { get; set; }
            public int LineCount { get; set; }
            public double AverageWordsPerLine { get; set; }
            public string RandomLine { get; set; }
        }

        public class Topic {
            public string TopicContent { get; set; }
            public DateTime DateSet { get; set; }
        }

        //Query Methods
        public IEnumerable<StatisticRecord> GetStatisticRecords(string channel, int minLength) {

            const string sql = @"SELECT m.Username, COUNT(m.Message) AS LineCount, 
                                AVG(CONVERT(float,m.WordCount)) AS AverageWordsPerLine, 
                                Foo.Message as RandomLine
                                FROM Messages m 
                                CROSS APPLY (select TOP 1 Message, Username 
                                FROM Messages m2 
                                WHERE m2.Username = m.Username AND m2.Channel = {0} AND LEN(m2.Message) > {1}
                                ORDER BY NEWID()) FOO 
                                WHERE m.Channel = {0}
                                GROUP BY m.Username, FOO.Message ORDER BY LineCount DESC";

            return db.ExecuteQuery<StatisticRecord>(sql, channel, minLength);

        }

        public IEnumerable<Topic> GetLastXTopics(string channel, int topicsToGet) {
            var results = db.Topics.
                Where(x => x.Channel.Equals(channel)).
                OrderBy(x => x.Time).
                Take(topicsToGet).
                Select(x => new Topic {TopicContent = x.Content, DateSet = x.Time}).AsEnumerable();

            return results;
        }

        private string ConvertUrlsToLinks(string msg) {
            string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            return r.Replace(msg, "<a href=\"$1\" title=\"Click to open in a new window or tab\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"http://www");
        }
    }
}