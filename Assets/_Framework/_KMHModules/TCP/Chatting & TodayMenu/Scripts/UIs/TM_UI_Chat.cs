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

        // ê�� ���� �ð��� �ƴ�, ê�� ���� �ð� �������� ���õǾ�����.
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