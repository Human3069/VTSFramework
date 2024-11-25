using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace _KMH_Framework.TodayMenu
{
    public class TM_TCPServer : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=red><b>[TM_TCPServer]</b></color> {0}";

        protected Queue<Action> sendAllQueue = new Queue<Action>();
        protected Queue<Action> chatLogQueue = new Queue<Action>();

        protected static TM_TCPServer _instance;
        public static TM_TCPServer Instance
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

        protected TcpListener _listener;

        protected List<TcpClient> connectedClientList = new List<TcpClient>(); // 연결된 클라이언트 리스트
        protected Dictionary<TcpClient, string> connectedClientNameDic = new Dictionary<TcpClient, string>();

        protected bool isRunning = true;

        protected Dictionary<string, List<string>> lunchDic = new Dictionary<string, List<string>>();

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

            StartServerAsync().Forget();

            // 메시지 브로드캐스트 테스트
            // while (_isRunning)
            // {
            //     string message = Console.ReadLine(); // 콘솔에서 메시지 입력
            //     if (message == "exit")
            //     {
            //         Stop();
            //         break;
            //     }
            //     _Message(message);
            // }
        }

        protected virtual async UniTaskVoid StartServerAsync()
        {
            await UniTask.WaitUntil(delegate { return ConfigurationReader.Instance != null; });
            await ConfigurationReader.Instance.WaitUntilReady();

            int port = ConfigurationReader.Instance.TCPClientConfigHandler.Result.ServerPortNumber;

            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            Debug.LogFormat(LOG_FORMAT, "Server start!");

            Thread connectionThread = new Thread(AcceptClient);
            connectionThread.Start();

            PollingQueueAsync().Forget();
        }

        protected void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;

            StopServer();
        }

        protected virtual async UniTaskVoid PollingQueueAsync()
        {
            while (true)
            {
                lock (sendAllQueue)
                {
                    while (sendAllQueue.Count > 0)
                    {
                        Action sendAllAction = sendAllQueue.Dequeue();
                        sendAllAction.Invoke();
                    }
                }

                lock (chatLogQueue)
                {
                    while (chatLogQueue.Count > 0)
                    {
                        Action chatLogAction = chatLogQueue.Dequeue();
                        chatLogAction.Invoke();
                    }
                }

                await UniTask.NextFrame();
            }
        }

        protected void AcceptClient()
        {
            while (isRunning == true)
            {
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    IPEndPoint clientEndPoint = client.Client.LocalEndPoint as IPEndPoint;
                    string clientIPText = clientEndPoint.Address.ToString() + " : " + clientEndPoint.Port;

                    Debug.LogFormat(LOG_FORMAT, "New client connected, ip : " + clientIPText);

                    connectedClientList.Add(client); // 클라이언트 저장

                    // 클라이언트 통신 스레드 시작
                    Thread clientThread = new Thread(() => HandleClientThread(client, clientIPText));
                    clientThread.Start();
                }
                catch (Exception exception)
                {
                    Debug.LogFormat(LOG_FORMAT, "Connection error : " + exception.Message);
                }
            }
        }

        protected void HandleClientThread(TcpClient client, string clientIP)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                Debug.LogFormat(LOG_FORMAT, "Client " + clientIP + " Connected.");

                while (isRunning == true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break; // 클라이언트 연결 종료
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    OnReceivedFromClient(client, message).Forget();
                }
            }
            catch (Exception exception)
            {
                Debug.LogFormat(LOG_FORMAT, "클라이언트 통신 오류 : " + exception.Message);
            }
            finally
            {
                Debug.LogFormat(LOG_FORMAT, "Client " + clientIP + " Connection Lost");
                SendAdminChatToAll(connectedClientNameDic[client] + " 님이 접속을 종료했습니다.").Forget();

                connectedClientList.Remove(client); 

                client.Close();
            }
        }

        protected virtual async UniTaskVoid OnReceivedFromClient(TcpClient client, string message)
        {
            Debug.LogFormat(LOG_FORMAT, "OnReceivedFromClient(), message : <color=white>" + message + "</color>");

            string pattern = @"_ProtocolType"":\s*(\d+)"; // _ProtocolType":100, _ProtocolType":200, _ProtocolType":201, ...
            Match match = Regex.Match(message, pattern);
            int protocolIndex = int.Parse(match.Groups[1].Value);

            ProtocolType type = (ProtocolType)protocolIndex;
            if (type == ProtocolType.CreateAccount_100)
            {
                RequestCreateAccount requestedData = JsonConvert.DeserializeObject<RequestCreateAccount>(message);
                Debug.LogFormat(LOG_FORMAT, requestedData.ToString());

                List<AccountDatabase> accountList = await TM_DBHandler.Instance.GetAccountDataList();
                List<AccountDatabase> foundAccountList = accountList.FindAll(PredicateFunc);

                bool PredicateFunc(AccountDatabase account)
                {
                    return account.user_id.Equals(requestedData.UserID) == true ||
                           account.user_name.Equals(requestedData.UserName) == true;
                }

                ResponseCreateAccount dataToSend;
                if (foundAccountList.Count == 0)
                {
                    bool isCreated = await TM_DBHandler.Instance.TryCreateAccountData(requestedData.UserName,
                                                                                      requestedData.UserID,
                                                                                      requestedData.Password,
                                                                                      requestedData.CreateDateTime);

                    if (isCreated == true)
                    {
                        dataToSend = new ResponseCreateAccount(0, "create user successfully");
                    }
                    else
                    {
                        dataToSend = new ResponseCreateAccount(-1, "invalid data");
                    }
                }
                else
                {
                    dataToSend = new ResponseCreateAccount(-1, "user id already exist");
                }

                string json = dataToSend.AsJson();
                SendMessageTo(client, json);
            }
            else if (type == ProtocolType.IsCreatableAccount_101)
            {
                RequestIsCreatableAccount requestedData = JsonConvert.DeserializeObject<RequestIsCreatableAccount>(message);
                Debug.LogFormat(LOG_FORMAT, requestedData.ToString());
            }

            else if (type == ProtocolType.Login_110)
            {
                RequestLogin requestedData = JsonConvert.DeserializeObject<RequestLogin>(message);
                Debug.LogFormat(LOG_FORMAT, requestedData.ToString());

                List<AccountDatabase> accountList = await TM_DBHandler.Instance.GetAccountDataList();
                List<AccountDatabase> foundAccountList = accountList.FindAll(PredicateFunc);
           
                bool PredicateFunc(AccountDatabase account)
                {
                    return account.user_id.Equals(requestedData.UserID) == true &&
                           account.encrypted_password.Equals(requestedData.Password) == true;
                }

                ResponseLogin dataToSend;
                bool isSuccess = foundAccountList.Count != 0;
                if (isSuccess == true)
                {
                    string userName = foundAccountList[0].user_name;
                    dataToSend = new ResponseLogin(userName, 0, "login successfully", lunchDic);
                }
                else
                {
                    dataToSend = new ResponseLogin("", 1, "account doesn't exist", null);
                }

                await UniTask.NextFrame();

                if (isSuccess == true)
                {
                    List<ChattingLogDatabase> chattingLogList = await TM_DBHandler.Instance.GetChattingLogList();

                    ResponseChattingLog dataToSendLogList = new ResponseChattingLog(chattingLogList);
                    string logListJson = dataToSendLogList.AsJson();

                    chatLogQueue.Enqueue(() => SendMessageTo(client, logListJson));

#if false
                    foreach (ChattingLogDatabase chattingLog in chattingLogList)
                    {
                        string userName = chattingLog.user_name;
                        string userColor = chattingLog.user_color;
                        string chatting = chattingLog.chatting;
                        string sendTime = chattingLog.send_time;

                        if (chattingLog.user_name.Equals("admin") == true)
                        {
                            ResponseAdminChatting dataToSendIndividually = new ResponseAdminChatting(chatting, sendTime);
                            string jsonIndividually = dataToSendIndividually.AsJson();

                            chatLogQueue.Enqueue(() => SendMessageTo(client, jsonIndividually));
                        }
                        else
                        {
                            ResponseChatting dataToSendIndividually = new ResponseChatting(userName, userColor, chatting, sendTime);
                            string jsonIndividually = dataToSendIndividually.AsJson();

                            chatLogQueue.Enqueue(() => SendMessageTo(client, jsonIndividually));
                        }
                    }
#endif

                    SendAdminChatToAll(foundAccountList[0].user_name + "님이 접속하였습니다").Forget();
                    connectedClientNameDic.Add(client, dataToSend.UserName);

                    string json = dataToSend.AsJson();
                    SendMessageTo(client, json);
                }
            }
            else if (type == ProtocolType.Logout_111)
            {
                RequestLogout requestedData = JsonConvert.DeserializeObject<RequestLogout>(message);
                Debug.LogFormat(LOG_FORMAT, requestedData.ToString());
            }
         
            else if (type == ProtocolType.Chatting_200)
            {
                RequestChatting requestedData = JsonConvert.DeserializeObject<RequestChatting>(message);
                Debug.LogFormat(LOG_FORMAT, requestedData.ToString());

                string userName = requestedData.UserName;
                string userColorText = requestedData.UserColorText;
                string chatText = requestedData.ChatText;
                string sendTime = requestedData.SendTime;

                await TM_DBHandler.Instance.CreateChattingLogData(userName, userColorText, chatText, sendTime);

                ResponseChatting dataToSend = new ResponseChatting(userName, userColorText, chatText, sendTime);
                string json = dataToSend.AsJson();

                EnqueueSendMessageAll(json);
            }

            //Region
            else if (type == ProtocolType.AddLunch_210)
            {
                RequestAddLunch requestedData = JsonConvert.DeserializeObject<RequestAddLunch>(message);
                Debug.LogFormat(LOG_FORMAT, requestedData.ToString());

                string lunchName = requestedData.LunchName;

                ResponseAddLunch dataToSend = new ResponseAddLunch(lunchName);
                string json = dataToSend.AsJson();

                if (lunchDic.ContainsKey(lunchName) == false && string.IsNullOrEmpty(lunchName) == false)
                {
                    lunchDic.Add(lunchName, new List<string>());
                }

                EnqueueSendMessageAll(json);
            }
            else if (type == ProtocolType.RemoveLunch_211)
            {
                RequestRemoveLunch requestedData = JsonConvert.DeserializeObject<RequestRemoveLunch>(message);
                Debug.LogFormat(LOG_FORMAT, requestedData.ToString());

                string lunchName = requestedData.LunchName;
        
                ResponseRemoveLunch dataToSend = new ResponseRemoveLunch(lunchName);
                string json = dataToSend.AsJson();

                if (lunchDic.ContainsKey(lunchName) == true)
                {
                    lunchDic.Remove(lunchName);
                }

                EnqueueSendMessageAll(json);
            }
            else if (type == ProtocolType.AddReservedName_212)
            {
                RequestAddReservedName requestedData = JsonConvert.DeserializeObject<RequestAddReservedName>(message);
                Debug.LogFormat(LOG_FORMAT, requestedData.ToString());

                string lunchName = requestedData.LunchName;
                string reservedName = requestedData.ReservedName;

                ResponseAddReservedName dataToSend = new ResponseAddReservedName(lunchName, reservedName);
                string json = dataToSend.AsJson();

                if (lunchDic.ContainsKey(lunchName) == true)
                {
                    lunchDic[lunchName].Add(reservedName);
                }

                EnqueueSendMessageAll(json);
            }
            else if (type == ProtocolType.RemoveReservedName_213)
            {
                RequestRemoveReservedName requestedData = JsonConvert.DeserializeObject<RequestRemoveReservedName>(message);
                Debug.LogFormat(LOG_FORMAT, requestedData.ToString());

                string lunchName = requestedData.LunchName;
                string reservedName = requestedData.ReservedName;

                ResponseRemoveReservedName dataToSend = new ResponseRemoveReservedName(lunchName, reservedName);
                string json = dataToSend.AsJson();

                if (lunchDic.ContainsKey(dataToSend.LunchName) == true)
                {
                    if (lunchDic[lunchName].Contains(reservedName) == true)
                    {
                        lunchDic[lunchName].Remove(reservedName);
                    }
                }

                EnqueueSendMessageAll(json);
            }
            else if (type == ProtocolType.ClearReservedName_214)
            {
                RequestClearReservedName requestedData = JsonConvert.DeserializeObject<RequestClearReservedName>(message);
                Debug.LogFormat(LOG_FORMAT, requestedData.ToString());

                string reservedName = requestedData.ReservedName;

                ResponseClearReservedName dataToSend = new ResponseClearReservedName(requestedData.ReservedName);
                string json = dataToSend.AsJson();

                Dictionary<string, List<string>>.Enumerator enumerator = lunchDic.GetEnumerator();
                while (enumerator.MoveNext() == true)
                {
                    KeyValuePair<string, List<string>> pair = enumerator.Current;
                    if (pair.Value.Contains(reservedName) == true)
                    {
                        lunchDic[pair.Key].Remove(reservedName);
                    }
                }

                EnqueueSendMessageAll(json);
            }
        }

        public void SendMessageTo(TcpClient client, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            try
            {
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
            }
            catch (Exception exception)
            {
                Debug.LogFormat(LOG_FORMAT, "Failed to SendMessageAll : " + exception.Message);
            }

            Debug.LogFormat(LOG_FORMAT, "SendMessage : " + message);
        }

        public virtual async UniTaskVoid SendAdminChatToAll(string chatting)
        {
            string sendTime = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");

            ResponseAdminChatting packetData = new ResponseAdminChatting(chatting, sendTime);
            string json = packetData.AsJson();

            await TM_DBHandler.Instance.CreateChattingLogData("admin", "000000", packetData.ChatText, sendTime);

            EnqueueSendMessageAll(json);
        }

        protected void EnqueueSendMessageAll(string message)
        {
            sendAllQueue.Enqueue(() => SendMessageAll(message));
        }

        protected void SendMessageAll(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            lock (connectedClientList) 
            {
                foreach (TcpClient client in connectedClientList)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogFormat(LOG_FORMAT, "Failed to SendMessageAll : " + exception.Message);
                    }
                }
            }
        }

        public void StopServer()
        {
            isRunning = false;
            _listener.Stop();

            lock (connectedClientList)
            {
                foreach (TcpClient client in connectedClientList)
                {
                    client.Close();
                }
                connectedClientList.Clear();
            }

            Debug.LogFormat(LOG_FORMAT, "Server stopped");
        }
    }
}