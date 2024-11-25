using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _KMH_Framework.TodayMenu
{
    public class TM_UI_Chat : MonoBehaviour
    {
        [SerializeField]
        protected TextMeshProUGUI userIDText;
        [SerializeField]
        protected TextMeshProUGUI messageText;
        [SerializeField]
        protected TextMeshProUGUI timeText;

        // 챗을 보낸 시각이 아닌, 챗을 받은 시각 기준으로 세팅되어있음.
        public virtual void Set(string userID, Color color, string message, string sendTime)
        {
            userIDText.text = userID;
            userIDText.color = color;

            messageText.text = message;

            string[] splitted = sendTime.Split(' ');
            string hourToSec = splitted[1];
            timeText.text = hourToSec;
        }
    }
}