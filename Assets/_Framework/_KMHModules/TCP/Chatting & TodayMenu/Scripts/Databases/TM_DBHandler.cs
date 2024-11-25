using UnityEngine;
using Cysharp.Threading.Tasks;
using MySqlConnector;
using System;
using System.Threading;
using System.Collections.Generic;

namespace _KMH_Framework.TodayMenu
{
    public class TM_DBHandler : MonoBehaviour
    {
        public const string ENCRYPTION_KEY = "개발조아"; // 비밀번호는 DB에 암호화하여 저장합니다. 암호화 및 복호화할 때의 키입니다.

        private const string LOG_FORMAT = "<color=yellow><b>[TM_DBHandler]</b></color> {0}";

        private const string TABLE_NAME_ACCOUNT = "vts_account";
        private const string TABLE_NAME_CHATTING_LOG = "vts_chatting_log";

        protected static TM_DBHandler _instance;
        public static TM_DBHandler Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected MySqlConnection mySql;

        [ReadOnly]
        [SerializeField]
        protected List<AccountDatabase> accountDBList = new List<AccountDatabase>();
        [ReadOnly]
        [SerializeField]
        protected List<ChattingLogDatabase> chattingLogList = new List<ChattingLogDatabase>();

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this.gameObject);
                return;
            }

            Connect().Forget();
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;

            mySql.Dispose();
            mySql = null;
        }

        public virtual async UniTaskVoid Connect()
        {
            await UniTask.WaitUntil(delegate { return ConfigurationReader.Instance != null; });
            await ConfigurationReader.Instance.WaitUntilReady();

            DatabaseConfig config = ConfigurationReader.Instance.DatabaseConfigHandler.Result;

            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder()
            {
                Server = config.IPAddress,
                UserID = config.UserID,
                Password = config.UserPassword,
                Database = config.DatabaseName,
            };

            try
            {
                mySql = new MySqlConnection(stringBuilder.ConnectionString);
            }
            catch(Exception e)
            {
                Debug.LogErrorFormat(LOG_FORMAT, e.Message);
            }
        }

        public virtual async UniTask<List<AccountDatabase>> GetAccountDataList()
        {
            await mySql.OpenAsync(new CancellationToken());
            accountDBList.Clear();

            string commandText = "SELECT * FROM " + TABLE_NAME_ACCOUNT;
            MySqlCommand command = new MySqlCommand(commandText, mySql);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read() == true)
            {
                AccountDatabase accountDB = new AccountDatabase(reader);
                accountDBList.Add(accountDB);
            }

            await mySql.CloseAsync();
            return accountDBList;
        }

        public virtual async UniTask<bool> TryCreateAccountData(string userName, string userID, string encryptedPassword, string createDate)
        {
            await mySql.OpenAsync(new CancellationToken());

            AccountDatabase newAccount = new AccountDatabase(userID, userName, encryptedPassword, createDate);
            MySqlCommand command = new MySqlCommand(newAccount.ToInsertQuery(TABLE_NAME_ACCOUNT), mySql);

            bool isCreated = command.ExecuteNonQuery() == 1;
            if (isCreated == true)
            {
                accountDBList.Add(newAccount);
            }

            await mySql.CloseAsync();
            return isCreated;
        }

        public virtual async UniTask<List<ChattingLogDatabase>> GetChattingLogList()
        {
            await mySql.OpenAsync(new CancellationToken());
            chattingLogList.Clear();

            string commandText = "SELECT * FROM " + TABLE_NAME_CHATTING_LOG;
            MySqlCommand command = new MySqlCommand(commandText, mySql);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read() == true)
            {
                ChattingLogDatabase chattingLog = new ChattingLogDatabase(reader);
                chattingLogList.Add(chattingLog);
            }

            await mySql.CloseAsync();
            return chattingLogList;
        }

        public virtual async UniTask CreateChattingLogData(string userName, string userColor, string chatting, string time)
        {
            await mySql.OpenAsync(new CancellationToken());

            ChattingLogDatabase newChatting = new ChattingLogDatabase(userName, userColor, chatting, time);
            MySqlCommand command = new MySqlCommand(newChatting.ToInsertQuery(TABLE_NAME_CHATTING_LOG), mySql);

            bool isCreated = command.ExecuteNonQuery() == 1;
            if (isCreated == true)
            {
                chattingLogList.Add(newChatting);
            }
            else
            {
                Debug.Assert(false);
            }

            await mySql.CloseAsync();
        }
    }
}