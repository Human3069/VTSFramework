using MySqlConnector;
using UnityEngine;

namespace _KMH_Framework.TodayMenu
{
    [System.Serializable]
    public struct ChattingLogDatabase
    {
        [ReadOnly]
        public string user_name;
        [ReadOnly]
        public string user_color;
        [ReadOnly]
        public string chatting;

        [Space(10)]
        [ReadOnly]
        public string send_time;

        public ChattingLogDatabase(MySqlDataReader reader)
        {
            user_name = reader["user_name"].ToString();
            user_color = reader["user_color"].ToString();
            chatting = reader["chatting"].ToString();

            send_time = reader["send_time"].ToString();
        }

        public ChattingLogDatabase(string userName, string userColor, string _chatting, string _time)
        {
            user_name = userName;
            user_color = userColor;
            chatting = _chatting;
            send_time = _time;
        }

        public string ToInsertQuery(string tableName)
        {
            return "INSERT INTO " + tableName + "(user_name,user_color,chatting,send_time) " +
                   "VALUES(\'" + user_name + "\',\'" + user_color + "\',\'" + chatting + "\',\'" + send_time + "\')";
        }
    }
}