using MySqlConnector;
using UnityEngine;

namespace _KMH_Framework.TodayMenu
{
    [System.Serializable]
    public struct AccountDatabase
    {
        [ReadOnly]
        public string user_id;
        [ReadOnly]
        public string user_name;
        [ReadOnly]
        public string encrypted_password;

        [Space(10)]
        [ReadOnly]
        public string create_date;

        public AccountDatabase(MySqlDataReader reader)
        {
            user_id = reader["user_id"].ToString();
            user_name = reader["user_name"].ToString();
            encrypted_password = reader["encrypted_password"].ToString();

            create_date = reader["create_date"].ToString();
        }

        public AccountDatabase(string userID, string userName, string encryptedPassword, string createDate)
        {
            user_id = userID;
            user_name = userName;
            encrypted_password = encryptedPassword;
            create_date = createDate;
        }

        public string ToInsertQuery(string tableName)
        {
            return "INSERT INTO " + tableName + "(user_id,user_name,encrypted_password,create_date) " +
                   "VALUES(\'" + user_id + "\',\'" + user_name + "\',\'" + encrypted_password + "\',\'" + create_date + "\')";
        }
    }
}