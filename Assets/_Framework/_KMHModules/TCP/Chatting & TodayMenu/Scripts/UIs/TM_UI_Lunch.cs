using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _KMH_Framework.TodayMenu
{
    public class TM_UI_Lunch : MonoBehaviour
    {
        [SerializeField]
        protected Transform namesParent;
        [SerializeField]
        protected TM_UI_Name namePrefab;

        [Space(10)]
        [SerializeField]
        protected TextMeshProUGUI lunchTitleText;

        [Space(10)]
        [SerializeField]
        protected Button deleteThisButton;
        [SerializeField]
        protected Button addMyNameButton;

        public static List<TM_UI_Name> AllNameInstanceList = new List<TM_UI_Name>();

        protected virtual void Start()
        {
            deleteThisButton.onClick.AddListener(OnClickDeleteThisButton);
            void OnClickDeleteThisButton()
            {
                TM_TCPClient.Instance.RequestRemoveLunch(GetTitle());
            }

            addMyNameButton.onClick.AddListener(OnClickAddMyNameButton);
            void OnClickAddMyNameButton()
            {
                string myName = TM_TCPClient.Instance.UserName;
                TM_UI_Name foundName = GetNameInstanceList().Find(x => x.GetName().Equals(myName) == true);

                if (foundName == null)
                {
                    Debug.Log("OnClickAddMyNameButton, GetTitle() : " + GetTitle());
                    TM_TCPClient.Instance.RequestAddReservedName(GetTitle(), myName);
                }
            }
        }

        public virtual void InstantiateReservedName(string reservedName)
        {
            TM_UI_Name newName = Instantiate(namePrefab, namesParent);
            newName.Set(reservedName);

            AllNameInstanceList.Add(newName);
        }

        public virtual string GetTitle()
        {
            return lunchTitleText.text;
        }

        public List<TM_UI_Name> GetNameInstanceList()
        {
            TM_UI_Name[] nameInstances = namesParent.GetComponentsInChildren<TM_UI_Name>();
            List<TM_UI_Name> nameInstanceList = nameInstances.ToList();

            return nameInstanceList;
        }

        public virtual void Set(string lunchName)
        {
            lunchTitleText.text = lunchName;
        }

        public virtual void SetWithReserved(string lunchName, List<string> reservedNameList)
        {
            Set(lunchName);
            foreach (string reservedName in reservedNameList)
            {
                InstantiateReservedName(reservedName);
            }
        }
    }
}