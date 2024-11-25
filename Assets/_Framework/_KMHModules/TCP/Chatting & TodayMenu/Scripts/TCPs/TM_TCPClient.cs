using _KMH_Framework._TS_Module;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace _KMH_Framework.TodayMenu
{
    public class TM_TCPClient : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[TM_TCPClient]</b></color> {0}";

        protected static TM_TCPClient _instance;
        public static TM_TCPClient Instance
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

        protected TcpClient client;

        protected NetworkStream stream;
        protected Thread clientThread;

        protected Queue<Action> requestMainThreadQueue = new Queue<Action>();
        protected Queue<Action> responseMainThreadQueue = new Queue<Action>();

        public delegate void _ResponseCreateAccount(int result, string message);
        public static event _ResponseCreateAccount OnResponseCreateAccount;

        public delegate void _ResponseLogin(string userName, int result, string message, Dictionary<string, List<string>> lunchDic);
        public static event _ResponseLogin OnResponseLogin;

        public delegate void _ResponseChatting(string userID, string userColorText, string chatText, string sendTime);
        public static event _ResponseChatting OnResponseChatting;

        public delegate void _ResponseAdminChatting(string message, string sendTime);
        public static event _ResponseAdminChatting OnResponseAdminChatting;


        public delegate void _ResponseAddLunch(string lunchName);
        public static event _ResponseAddLunch OnResponseAddLunch;

        public delegate void _ResponseRemoveLunch(string lunchName);
        public static event _ResponseRemoveLunch OnResponseRemoveLunch;

        public delegate void _ResponseAddReservedName(string lunchName, string reservedName);
        public static event _ResponseAddReservedName OnResponseAddReservedName;

        public delegate void _ResponseRemoveReservedName(string lunchName, string reservedName);
        public static event _ResponseRemoveReservedName OnResponseRemoveReservedName;

        public delegate void _ResponseClearReservedName(string reservedName);
        public static event _ResponseClearReservedName OnResponseClearReservedName;

        [ReadOnly]
        public string UserName = "#UNDEFINED";

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

            AwakeAsync().Forget();
        }

        protected virtual async UniTaskVoid AwakeAsync()
        {
            await UniTask.WaitUntil(delegate { return ConfigurationReader.Instance != null; });
            await ConfigurationReader.Instance.WaitUntilReady();
            await UniTask.NextFrame();

            string ip = ConfigurationReader.Instance.TCPClientConfigHandler.Result.ServerIPAddress;
            int port = ConfigurationReader.Instance.TCPClientConfigHandler.Result.ServerPortNumber;

            ConnectToServer(ip, port).Forget();
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;

            Disconnect();
        }

        protected virtual async UniTaskVoid ConnectToServer(string ip, int port)
        {
            Debug.LogFormat(LOG_FORMAT, "ConnectToServerAsync(), ip : " + ip + " : " + port);

            await UniTask.SwitchToThreadPool();

            client = new TcpClient();

            try
            {
                client.Connect(ip, port);
                Debug.LogFormat(LOG_FORMAT, "Connected to server!");

                stream = client.GetStream();

                clientThread = new Thread(OnThreadStart);
                void OnThreadStart()
                {
                    while (client.Connected == true)
                    {
                        try
                        {
                            byte[] buffer = new byte[4096];
                            if (stream.DataAvailable == true)
                            {
                                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                                responseMainThreadQueue.Enqueue(() => OnResponseFromServer(message));
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogErrorFormat(LOG_FORMAT, "client error: " + e.Message);
                            break;
                        }
                    }
                }

                clientThread.IsBackground = true;
                clientThread.Start();
            }
            catch (Exception e)
            {
                Debug.LogError("Connection error: " + e.Message);

#if !UNITY_EDITOR
                Application.Quit();
#endif
            }

            await UniTask.SwitchToMainThread();
        }

        protected virtual void Update()
        {
            // 굳이 다이렉트로 호출하지않고 Request 및 Response를 Queue에 쌓아두고 하나씩 실행하는 이유는 2가지입니다.

            // 1. 분리된 스레드가 아닌 메인스레드에서만 할 수 있는 행위들이 있기때문 (foreach, .gameObject, ...)
            // 2. 통신 순서가 역전되면서 꼬일 가능성을 방지하기위해

            lock (requestMainThreadQueue)
            {
                while (requestMainThreadQueue.Count > 0)
                {
                    Action requestAction = requestMainThreadQueue.Dequeue();
                    requestAction.Invoke();
                }
            }

            lock (responseMainThreadQueue)
            {
                while (responseMainThreadQueue.Count > 0)
                {
                    Action responseAction = responseMainThreadQueue.Dequeue();
                    responseAction.Invoke();
                }
            }
        }

        protected virtual void Disconnect()
        {
            if (client != null)
            {
                client.Close();
            }

            if (clientThread != null)
            {
                clientThread.Abort();
            }

            Debug.LogFormat(LOG_FORMAT, "Disconnected from server.");
        }

        protected virtual void EnqueueRequest(string message)
        {
            requestMainThreadQueue.Enqueue(() => SendMessageToServer(message));
        }

        protected virtual void SendMessageToServer(string message)
        {
            if (client == null || client.Connected == false)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "cannot connect to server.");
                return;
            }

            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            string pattern = @"_ProtocolType"":\s*(\d+)"; // _ProtocolType":100, _ProtocolType":200, _ProtocolType":201, ...
            Match match = Regex.Match(message, pattern);
            int protocolIndex = int.Parse(match.Groups[1].Value);

            ProtocolType type = (ProtocolType)protocolIndex;

            Debug.LogFormat(LOG_FORMAT, "SendMessageToServer(), protocol : <color=yellow>" + type + "</color>, message : <color=white>" + message + "</color>");
        }

        public virtual void RequestCreateAccount(string userName, string userID, string password, string createDateTime)
        {
            RequestCreateAccount socketData = new RequestCreateAccount(userName, userID, password, createDateTime);
            string json = socketData.AsJson();

            EnqueueRequest(json);
        }

        public virtual void RequestIsCreatableAccount(string userID, string password)
        {
            RequestIsCreatableAccount socketData = new RequestIsCreatableAccount(userID, password);
            string json = socketData.AsJson();

            EnqueueRequest(json);
        }

        public virtual void RequestLogin(string userID, string password)
        {
            RequestLogin socketData = new RequestLogin(userID, password);
            string json = socketData.AsJson();

            EnqueueRequest(json);
        }

        public virtual void RequestLogout(string userID)
        {
            RequestLogout socketData = new RequestLogout(userID);
            string json = socketData.AsJson();

            EnqueueRequest(json);
        }

        public virtual void RequestChatting(string userName, Color userColor, string chatText)
        {
            RequestChatting socketData = new RequestChatting(userName, userColor.ToHtmlString(), chatText, DateTime.Now.ToString("yy-MM-dd HH:mm:ss"));
            string json = socketData.AsJson();

            EnqueueRequest(json);
        }

        public virtual void RequestAddLunch(string lunchName)
        {
            RequestAddLunch socketData = new RequestAddLunch(lunchName);
            string json = socketData.AsJson();

            EnqueueRequest(json);
        }

        public virtual void RequestRemoveLunch(string lunchName)
        {
            RequestRemoveLunch socketData = new RequestRemoveLunch(lunchName);
            string json = socketData.AsJson();

            EnqueueRequest(json);
        }

        public virtual void RequestAddReservedName(string lunchName, string reservedName)
        {
            RequestAddReservedName socketData = new RequestAddReservedName(lunchName, reservedName);
            string json = socketData.AsJson();

            EnqueueRequest(json);
        }

        protected Action<string> _OnResponseRemoveReservedName = null;

        public virtual void RequestRemoveReservedName(string lunchName, string reservedName, Action<string> onResponse = null)
        {
            RequestRemoveReservedName socketData = new RequestRemoveReservedName(lunchName, reservedName);
            string json = socketData.AsJson();

            _OnResponseRemoveReservedName = onResponse;

            EnqueueRequest(json);
        }

        public virtual void RequestClearReservedName(string reservedName)
        {
            RequestClearReservedName socketData = new RequestClearReservedName(reservedName);
            string json = socketData.AsJson();

            EnqueueRequest(json);
        }

        protected virtual void OnResponseFromServer(string message)
        {
            // 서버로부터 텍스트가 한 번에 여러개가 들어올 수 있음. 따라서 이를 사전에 나눠줌
            List<string> resultMessageList = new List<string>();
            int depth = 0;
            int startIndex = 0;

            for (int i = 0; i < message.Length; i++)
            {
                if (message[i] == '{')
                {
                    if (depth == 0)
                    {
                        startIndex = i; // JSON 객체의 시작
                    }
                    depth++;
                }
                else if (message[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        // JSON 객체의 끝
                        resultMessageList.Add(message.Substring(startIndex, i - startIndex + 1));
                    }
                }
            }

            foreach (string resultMessage in resultMessageList)
            {
                if (resultMessage.Equals("{}") == true)
                {
                    continue;
                }
                Debug.Log("rawData : " + resultMessage);

                string pattern = @"_ProtocolType"":\s*(\d+)"; // _ProtocolType":100, _ProtocolType":200, _ProtocolType":201, ...
                Match match = Regex.Match(resultMessage, pattern);
                int protocolIndex = int.Parse(match.Groups[1].Value);

                ProtocolType type = (ProtocolType)protocolIndex;
                Debug.LogFormat(LOG_FORMAT, "OnResponseFromServer(), protocol : " + type + ", resultMessage : <color=white>" + resultMessage + "</color>");

                if (type == ProtocolType.CreateAccount_100)
                {
                    ResponseCreateAccount responseData = JsonConvert.DeserializeObject<ResponseCreateAccount>(resultMessage);
                    if (OnResponseCreateAccount != null)
                    {
                        OnResponseCreateAccount(responseData.Result, responseData.Message);
                    }
                }
                else if (type == ProtocolType.Login_110)
                {
                    ResponseLogin responseData = JsonConvert.DeserializeObject<ResponseLogin>(resultMessage);
                    if (OnResponseLogin != null)
                    {
                        OnResponseLogin(responseData.UserName, responseData.Result, responseData.Message, responseData.LunchDic);
                    }
                }

                else if (type == ProtocolType.Chatting_200)
                {
                    ResponseChatting responseData = JsonConvert.DeserializeObject<ResponseChatting>(resultMessage);
                    if (OnResponseChatting != null)
                    {
                        OnResponseChatting(responseData.UserName, responseData.UserColorText, responseData.ChatText, responseData.SendTime);
                    }
                }
                else if (type == ProtocolType.AdminChatting_201)
                {
                    ResponseAdminChatting responseData = JsonConvert.DeserializeObject<ResponseAdminChatting>(resultMessage);
                    if (OnResponseAdminChatting != null)
                    {
                        OnResponseAdminChatting(responseData.ChatText, responseData.SendTime);
                    }
                }
                else if (type == ProtocolType.ChattingLog_112)
                {
                    ResponseChattingLog responseData = JsonConvert.DeserializeObject<ResponseChattingLog>(resultMessage);
                    foreach (ChattingLogDatabase log in responseData.ChattingLogList)
                    {
                        if (log.user_name.Equals("admin") == true)
                        {
                            if (OnResponseAdminChatting != null)
                            {
                                OnResponseAdminChatting(log.chatting, log.send_time);
                            }
                        }
                        else
                        {
                            if (OnResponseChatting != null)
                            {
                                OnResponseChatting(log.user_name, log.user_color, log.chatting, log.send_time);
                            }
                        }
                    }
                }

                else if (type == ProtocolType.AddLunch_210)
                {
                    ResponseAddLunch responseData = JsonConvert.DeserializeObject<ResponseAddLunch>(resultMessage);
                    if (OnResponseAddLunch != null)
                    {
                        OnResponseAddLunch(responseData.LunchName);
                    }
                }
                else if (type == ProtocolType.RemoveLunch_211)
                {
                    ResponseRemoveLunch responseData = JsonConvert.DeserializeObject<ResponseRemoveLunch>(resultMessage);
                    if (OnResponseRemoveLunch != null)
                    {
                        OnResponseRemoveLunch(responseData.LunchName);
                    }
                }
                else if (type == ProtocolType.AddReservedName_212)
                {
                    ResponseAddReservedName responseData = JsonConvert.DeserializeObject<ResponseAddReservedName>(resultMessage);
                    if (OnResponseAddReservedName != null)
                    {
                        OnResponseAddReservedName(responseData.LunchName, responseData.ReservedName);
                    }
                }
                else if (type == ProtocolType.RemoveReservedName_213)
                {
                    ResponseRemoveReservedName responseData = JsonConvert.DeserializeObject<ResponseRemoveReservedName>(resultMessage);
                    if (OnResponseRemoveReservedName != null)
                    {
                        OnResponseRemoveReservedName(responseData.LunchName, responseData.ReservedName);
                    }

                    if (_OnResponseRemoveReservedName != null)
                    {
                        _OnResponseRemoveReservedName.Invoke(responseData.ReservedName);
                        _OnResponseRemoveReservedName = null;
                    }
                }
                else if (type == ProtocolType.ClearReservedName_214)
                {
                    ResponseClearReservedName responseData = JsonConvert.DeserializeObject<ResponseClearReservedName>(resultMessage);
                    if (OnResponseClearReservedName != null)
                    {
                        OnResponseClearReservedName(responseData.ReservedName);
                    }
                }
            }
        }
    }
}