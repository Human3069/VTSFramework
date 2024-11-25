using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _KMH_Framework.TodayMenu
{
    public class TM_UI_AdminChat : MonoBehaviour
    {
        [SerializeField]
        protected TextMeshProUGUI messageText;
        [SerializeField]
        protected TextMeshProUGUI timeText;

        public virtual void Set(string message, string sendTime)
        {
            messageText.text = message;

            // 24-13-23 23:23:23

            string[] splitted = sendTime.Split(' ');
            // string yearToDay = splitted[0];
            string hourToSec = splitted[1];

            // splitted = yearToDay.Split("-");
            // int receivedYear = int.Parse(splitted[0]);
            // int receivedMonth = int.Parse(splitted[1]);
            // int receivedDay = int.Parse(splitted[2]);
            // 
            // DateTime current = DateTime.Now;
            // DateTime received = new DateTime(receivedYear, receivedMonth, receivedDay);
            // TimeSpan span = current - received;

            timeText.text = hourToSec;
        }
    }
}