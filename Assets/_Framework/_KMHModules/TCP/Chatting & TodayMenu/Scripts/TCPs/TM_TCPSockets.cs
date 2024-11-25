using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace _KMH_Framework.TodayMenu
{
    public enum ProtocolType
    {
        none = -1,

        Connected_0 = 0,
        Disconnected_1 = 1,

        CreateAccount_100 = 100,
        IsCreatableAccount_101 = 101,

        Login_110 = 110,
        Logout_111 = 111,
        ChattingLog_112 = 112,

        Chatting_200 = 200,
        AdminChatting_201 = 201,

        AddLunch_210 = 210,
        RemoveLunch_211 = 211,
        AddReservedName_212 = 212,
        RemoveReservedName_213 = 213,
        ClearReservedName_214 = 214,
    }

    public class BaseTCPSocket
    {
        public ProtocolType _ProtocolType = ProtocolType.none;

        public string AsJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class RequestCreateAccount : BaseTCPSocket
    {
        public string UserName;
        public string UserID;
        public string Password;
        public string CreateDateTime;

        public RequestCreateAccount(string userName, string userID, string password, string createDateTime)
        {
            _ProtocolType = ProtocolType.CreateAccount_100;

            UserName = userName;
            UserID = userID;
            Password = password;
            CreateDateTime = createDateTime;
        }

        public override string ToString()
        {
            return "[ UserName : " + UserName + " / UserID : " + UserID + " / Password : " + Password.ToEncryptAES(TM_DBHandler.ENCRYPTION_KEY) + " / CreateDateTime : " + CreateDateTime + " / _ProtocolType : <color=yellow>" + _ProtocolType + "</color> ]";
        }
    }

    public class RequestIsCreatableAccount : BaseTCPSocket
    {
        public string UserID;
        public string Password;

        public RequestIsCreatableAccount(string userID, string password)
        {
            _ProtocolType = ProtocolType.IsCreatableAccount_101;

            UserID = userID;
            Password = password;
        }

        public override string ToString()
        {
            return "[ UserID : " + UserID + " / Password : " + Password + " / _ProtocolType : <color=yellow>" + _ProtocolType + "</color> ]";
        }
    }

    public class RequestLogin : BaseTCPSocket
    {
        public string UserID;
        public string Password; // Decrypted

        public RequestLogin(string userID, string password)
        {
            _ProtocolType = ProtocolType.Login_110;

            UserID = userID;
            Password = password;
        }

        public override string ToString()
        {
            return "[ UserID : " + UserID + " / Password : " + Password.ToEncryptAES(TM_DBHandler.ENCRYPTION_KEY) + " / _ProtocolType : <color=yellow>" + _ProtocolType + "</color> ]";
        }
    }

    public class ResponseChattingLog : BaseTCPSocket
    {
        public List<ChattingLogDatabase> ChattingLogList;

        public ResponseChattingLog(List<ChattingLogDatabase> chattingLogList)
        {
            _ProtocolType = ProtocolType.ChattingLog_112;

            ChattingLogList = chattingLogList;
        }
    }

    public class RequestLogout : BaseTCPSocket
    {
        public string UserID;

        public RequestLogout(string userID)
        {
            _ProtocolType = ProtocolType.Logout_111;
            UserID = userID;
        }

        public override string ToString()
        {
            return "[ UserID : " + UserID + " / _ProtocolType : <color=yellow>" + _ProtocolType + "</color> ]";
        }
    }

    public class RequestChatting : BaseTCPSocket
    {
        public string UserName;
        public string UserColorText;
        public string ChatText;
        public string SendTime;

        public RequestChatting(string userName, string userColorText, string chatText, string sendTime)
        {
            _ProtocolType = ProtocolType.Chatting_200;

            UserName = userName;
            UserColorText = userColorText;
            ChatText = chatText;
            SendTime = sendTime;
        }

        public override string ToString()
        {
            return "[ UserID : " + UserName + " / ChatText : " + ChatText + " / _ProtocolType : <color=yellow>" + _ProtocolType + "</color> ]";
        }
    }

    public class RequestAddLunch : BaseTCPSocket
    {
        public string LunchName;

        public RequestAddLunch(string lunchName)
        {
            _ProtocolType = ProtocolType.AddLunch_210;

            LunchName = lunchName;
        }
    }

    public class RequestRemoveLunch : BaseTCPSocket
    {
        public string LunchName;
        public List<string> CurrentReservedNameList;

        public RequestRemoveLunch(string lunchName)
        {
            _ProtocolType = ProtocolType.RemoveLunch_211;

            LunchName = lunchName;
        }
    }

    public class RequestAddReservedName : BaseTCPSocket
    {
        public string LunchName;
        public string ReservedName;

        public RequestAddReservedName(string lunchName, string reservedName)
        {
            _ProtocolType = ProtocolType.AddReservedName_212;

            LunchName = lunchName;
            ReservedName = reservedName;
        }
    }

    public class RequestRemoveReservedName : BaseTCPSocket
    {
        public string LunchName;
        public string ReservedName;

        public RequestRemoveReservedName(string lunchName, string reservedName)
        {
            _ProtocolType = ProtocolType.RemoveReservedName_213;

            LunchName = lunchName;
            ReservedName = reservedName;
        }
    }

    public class RequestClearReservedName : BaseTCPSocket
    {
        public string ReservedName;

        public RequestClearReservedName(string reservedName)
        {
            _ProtocolType = ProtocolType.ClearReservedName_214;

            ReservedName = reservedName;
        }
    }

    public class ResponseCreateAccount : BaseTCPSocket
    {
        public int Result; // -1 => 실패 (이미 있는 계정 or DB 문제)
                           // 0 => 성공

        public string Message;

        public ResponseCreateAccount(int result, string message)
        {
            _ProtocolType = ProtocolType.CreateAccount_100;

            Result = result;
            Message = message;
        }
    }

    public class ResponseLogin : BaseTCPSocket
    {
        public string UserName;
        public int Result; // -1 => 실패 (없는 계정 정보)
                           // 0 => 성공
        public string Message;

        public Dictionary<string, List<string>> LunchDic; // 기존 등록된 점심투표 정보

        public ResponseLogin(string userName, int result, string message, Dictionary<string, List<string>> lunchDic)
        {
            _ProtocolType = ProtocolType.Login_110;

            UserName = userName;
            Result = result;
            Message = message;

            LunchDic = lunchDic;
        }
    }

    public class ResponseChatting : BaseTCPSocket
    {
        public string UserName;
        public string UserColorText;
        public string ChatText;
        public string SendTime;

        public ResponseChatting(string userName, string userColorText, string chatText, string sendTime)
        {
            _ProtocolType = ProtocolType.Chatting_200;

            UserName = userName;
            UserColorText = userColorText;
            ChatText = chatText;
            SendTime = sendTime;
        }
    }

    public class ResponseAdminChatting : BaseTCPSocket
    {
        public string ChatText;
        public string SendTime;

        public ResponseAdminChatting(string chatText, string sendTime)
        {
            _ProtocolType = ProtocolType.AdminChatting_201;

            ChatText = chatText;
            SendTime = sendTime;
        }
    }

    public class ResponseAddLunch : BaseTCPSocket
    {
        public string LunchName;

        public ResponseAddLunch(string lunchName)
        {
            _ProtocolType = ProtocolType.AddLunch_210;

            LunchName = lunchName;
        }
    }

    public class ResponseRemoveLunch : BaseTCPSocket
    {
        public string LunchName;

        public ResponseRemoveLunch(string lunchName)
        {
            _ProtocolType = ProtocolType.RemoveLunch_211;

            LunchName = lunchName;
        }
    }

    public class ResponseAddReservedName : BaseTCPSocket
    {
        public string LunchName;
        public string ReservedName;

        public ResponseAddReservedName(string lunchName, string reservedName)
        {
            _ProtocolType = ProtocolType.AddReservedName_212;

            LunchName = lunchName;
            ReservedName = reservedName;
        }
    }

    public class ResponseRemoveReservedName : BaseTCPSocket
    {
        public string LunchName;
        public string ReservedName;

        public ResponseRemoveReservedName(string lunchName, string reservedName)
        {
            _ProtocolType = ProtocolType.RemoveReservedName_213;

            LunchName = lunchName;
            ReservedName = reservedName;
        }
    }

    public class ResponseClearReservedName : BaseTCPSocket
    {
        public string ReservedName;

        public ResponseClearReservedName(string reservedName)
        {
            _ProtocolType = ProtocolType.ClearReservedName_214;

            ReservedName = reservedName;
        }
    }
}