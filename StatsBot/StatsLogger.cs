using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;

namespace StatsBot {
    class StatsLogger : IDisposable {

        public StatsLogger() {

            try {
#if DEBUG
                conn = new SqlConnection(PropertiesPublicSettings.Default.StatsBotConnectionString);
#else
                conn = new SqlConnection(Properties.PublicSettings.Default.StatsBotAppHarborString);
#endif
                conn.Open();

            } catch (SqlException e) {
                Console.WriteLine("Sql Exception: {0}",e.Message);
            }
        }

        private readonly IDbConnection conn;

        public void LogMessage(string message, string username, DateTime time, string channel) {

            IDbTransaction transac = null;
            int wordCount = message.Split(' ').Count();

            const string sql = "insert into Messages values (@Username, @Message, @WordCount, @Time, @Channel)";

            try {
                transac = conn.BeginTransaction();

                conn.Execute(sql, new {
                                          Username = username, Message = message,
                                          WordCount = wordCount, Time = time, Channel = channel
                                      }, transac);
                transac.Commit();
            }catch(Exception e) {
                try {
                    transac.Rollback();
                    Console.WriteLine("Transaction Rolled Back: " + e.Message);

                }catch(Exception e2) {
                    Console.WriteLine("Rollback Failure: " + e2.Message);
                }
            }
        }

        public void LogTopicChange(string topic, string channel, DateTime time) {
            //stored procedure to prevent duplicates
            const string sql = "execute InsertTopic @Topic, @Channel, @Time, @TopicHash";

            //No way to store a UInt32 in SQL Server so we'll add int.MinValue
            //and store it as an Int32
            var hash = Crc32.Compute(topic) + int.MinValue;

            IDbTransaction transac = null;

            try {
                transac = conn.BeginTransaction();

                conn.Execute(sql, new {Topic = topic, Channel = channel, Time = time, TopicHash = hash}, transac);
                transac.Commit();
            }catch(Exception e) {
                try {
                    transac.Rollback();
                    Console.WriteLine("Transaction Rolled Back: "+e.Message);
                }catch(Exception e2) {
                    Console.WriteLine("Rollback Failure: " + e2.Message);
                }
            }
        }

        #region IDisposable Members

        public void Dispose() {
            conn.Close();
        }

        #endregion
    }
}
