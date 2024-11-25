using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _KMH_Framework.TodayMenu
{
    public class TM_UI_ServerLogger : MonoBehaviour
    {
        [SerializeField]
        protected TextMeshProUGUI logText;

        [Space(10)]
        [SerializeField]
        protected TMP_InputField chatInputField;

        protected virtual void Awake()
        {
            Application.logMessageReceived += OnLogMessageReceived;

            chatInputField.onSubmit.AddListener(OnChatInputFieldSubmit);

            if (EventSystem.current == null)
            {
                EventSystem.current = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).GetComponent<EventSystem>();
            }
        }

        protected virtual void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        protected void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            logText.text = logText.text + "\n[" + type + "]" + condition;
        }

        protected virtual void OnChatInputFieldSubmit(string text)
        {
            TM_TCPServer.Instance.SendAdminChatToAll(text);

            chatInputField.text = "";
            chatInputField.Select();
            chatInputField.ActivateInputField();
        }
    }
}