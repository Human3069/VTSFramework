using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _KMH_Framework.TodayMenu
{
    public class TM_UI_Name : MonoBehaviour
    {
        [SerializeField]
        protected TextMeshProUGUI nameText;

        public string GetName()
        {
            return nameText.text;
        }

        public string GetLunchTitle()
        {
            TM_UI_Lunch lunch = this.transform.parent.parent.GetComponent<TM_UI_Lunch>();
            return lunch.GetTitle();
        }

        public void Set(string text)
        {
            nameText.text = text;
        }
    }
}