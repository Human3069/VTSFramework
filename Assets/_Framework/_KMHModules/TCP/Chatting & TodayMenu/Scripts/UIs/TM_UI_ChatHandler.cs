using _KMH_Framework._TS_Module;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _KMH_Framework.TodayMenu
{
    public class TM_UI_ChatHandler : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[TM_UI_ChatHandler]</b></color> {0}";

        [SerializeField]
        protected TM_UI_Chat chatPrefab;
        [SerializeField]
        protected TM_UI_AdminChat adminChatPrefab;

        [Space(10)]
        [SerializeField]
        protected RectTransform chatLogsT;

        [Space(10)]
        [SerializeField]
        protected TMP_InputField sendChatInputField;

        [Space(10)]
        [SerializeField]
        protected Scrollbar verticalScrollbar;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected Color assignedColor = Color.red;

        protected virtual void Awake()
        {
            AwakeAsync().Forget();

            sendChatInputField.onSubmit.AddListener(OnSubmitSendChatInputField);

            TM_TCPClient.OnResponseChatting += OnResponseChatting;
            TM_TCPClient.OnResponseAdminChatting += OnResponseAdminChatting;
        }

        protected virtual async UniTaskVoid AwakeAsync()
        {
            await UniTask.WaitUntil(delegate { return ConfigurationReader.Instance != null; });
            await ConfigurationReader.Instance.WaitUntilReady();

            assignedColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }

        protected virtual void OnDestroy()
        {
            TM_TCPClient.OnResponseAdminChatting -= OnResponseAdminChatting;
            TM_TCPClient.OnResponseChatting -= OnResponseChatting;
        }

        protected virtual void OnResponseChatting(string userID, string userColorText, string message, string sendTime)
        {
            TM_UI_Chat chatInstance = Instantiate(chatPrefab, chatLogsT);

            Color userColor = ("#" + userColorText).ToColor();
            chatInstance.Set(userID, userColor, message, sendTime);

            PostOnResponseChatting().Forget();
        }

        protected virtual void OnResponseAdminChatting(string message, string sendTime)
        {
            TM_UI_AdminChat chatInstance = Instantiate(adminChatPrefab, chatLogsT);
            chatInstance.Set(message, sendTime);

            PostOnResponseChatting().Forget();
        }

        protected virtual async UniTaskVoid PostOnResponseChatting()
        {
            await UniTask.NextFrame();
            await UniTask.NextFrame();

            verticalScrollbar.value = 0f;
        }

        protected virtual void OnSubmitSendChatInputField(string text)
        {
            sendChatInputField.text = "";
            if (string.IsNullOrEmpty(text) == false)
            {
                TM_TCPClient.Instance.RequestChatting(TM_TCPClient.Instance.UserName, assignedColor, text);
            }

            sendChatInputField.Select();
            sendChatInputField.ActivateInputField();
        }

        public virtual void OnClickLGRedButton()
        {
            assignedColor = new Color(0.58f, 0.15f, 0.25f);
        }

        public virtual void OnClickGrapefruitButton()
        {
            assignedColor = new Color(0.7f, 0.32f, 0f);
        }

        public virtual void OnClickRottenOrangeButton()
        {
            assignedColor = new Color(0.76f, 0.61f, 0.25f);
        }

        public virtual void OnClickRottenYellowButton()
        {
            assignedColor = new Color(0.76f, 0.75f, 0.25f);
        }

        public virtual void OnClickLimeButton()
        {
            assignedColor = new Color(0.69f, 0.79f, 0.43f);
        }

        public virtual void OnClickGreenButton()
        {
            assignedColor = new Color(0.49f, 0.79f, 0.43f);
        }

        public virtual void OnClickRottenSkyButton()
        {
            assignedColor = new Color(0.45f, 0.6f, 0.6f);
        }

        public virtual void OnClickPurpleButton()
        {
            assignedColor = new Color(0.45f, 0.46f, 0.61f);
        }
    }
}