using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _KMH_Framework.TodayMenu
{
    public class TM_UI_LunchHandler : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[TM_UI_LunchHandler]</b></color> {0}";

        [SerializeField]
        protected Transform lunchMenuParent;
        [SerializeField]
        protected TM_UI_Lunch lunchPrefab;

        [Space(10)]
        [SerializeField]
        protected TMP_InputField menuNameInputField;
        [SerializeField]
        protected Button registerButton;
        [SerializeField]
        protected Button deleteMyNameButton;

        public static List<TM_UI_Lunch> AllLunchList = new List<TM_UI_Lunch>();

        protected virtual void Awake()
        {
            TM_TCPClient.OnResponseAddLunch += OnResponseAddLunch;
            TM_TCPClient.OnResponseRemoveLunch += OnResponseRemoveLunch;
            TM_TCPClient.OnResponseAddReservedName += OnResponseAddReservedName;
            TM_TCPClient.OnResponseRemoveReservedName += OnResponseRemoveReservedName;
            TM_TCPClient.OnResponseClearReservedName += OnResponseClearReservedName;
        }

        protected virtual void OnDestroy()
        {
            TM_TCPClient.OnResponseClearReservedName -= OnResponseClearReservedName;
            TM_TCPClient.OnResponseRemoveReservedName -= OnResponseRemoveReservedName;
            TM_TCPClient.OnResponseAddReservedName -= OnResponseAddReservedName;
            TM_TCPClient.OnResponseRemoveLunch -= OnResponseRemoveLunch;
            TM_TCPClient.OnResponseAddLunch -= OnResponseAddLunch;
        }

        protected virtual void Start()
        {
            registerButton.onClick.AddListener(OnClickRegisterButton);
            void OnClickRegisterButton()
            {
                string lunchName = menuNameInputField.text;
                TM_UI_Lunch foundEqual = AllLunchList.Find(x => x.GetTitle().Equals(lunchName) == true);
                if (foundEqual == null)
                {
                    TM_TCPClient.Instance.RequestAddLunch(lunchName);
                }
            }

            deleteMyNameButton.onClick.AddListener(OnClickDeleteMyNameButton);
            void OnClickDeleteMyNameButton()
            {
                string userID = TM_TCPClient.Instance.UserName;
                TM_TCPClient.Instance.RequestClearReservedName(userID);
            }
        }

        public virtual void InstantiateCurrentLunches(Dictionary<string, List<string>> currentLunchDic)
        {
            Debug.LogFormat(LOG_FORMAT, "InstantiateCurrentLunch()");

            Dictionary<string, List<string>>.Enumerator enumerator = currentLunchDic.GetEnumerator();
            while (enumerator.MoveNext() == true)
            {
                KeyValuePair<string, List<string>> pair = enumerator.Current;

                TM_UI_Lunch lunchInstance = Instantiate(lunchPrefab, lunchMenuParent);
                lunchInstance.SetWithReserved(pair.Key, pair.Value);

                AllLunchList.Add(lunchInstance);
            }
        }

        protected virtual void OnResponseAddLunch(string lunchName)
        {
            Debug.LogFormat(LOG_FORMAT, "OnResponseAddLunch(), lunchName : " + lunchName);

            TM_UI_Lunch foundLunch = AllLunchList.Find(lunch => lunch.GetTitle().Equals(lunchName) == true);
            if (foundLunch == null && string.IsNullOrEmpty(lunchName) == false)
            {
                TM_UI_Lunch lunchInstance = Instantiate(lunchPrefab, lunchMenuParent);
                lunchInstance.Set(lunchName);

                AllLunchList.Add(lunchInstance);
              
                menuNameInputField.text = "";
            }
        }

        protected virtual void OnResponseRemoveLunch(string lunchName)
        {
            Debug.LogFormat(LOG_FORMAT, "OnResponseRemoveLunch(), lunchName : " + lunchName);

            TM_UI_Lunch foundLunch = AllLunchList.Find(x => x.GetTitle().Equals(lunchName) == true);
            if (foundLunch != null)
            {
                if (AllLunchList.Contains(foundLunch) == true)
                {
                    List<TM_UI_Name> currentNameList = foundLunch.GetNameInstanceList();
                    foreach (TM_UI_Name currentName in currentNameList)
                    {
                        if (TM_UI_Lunch.AllNameInstanceList.Contains(currentName) == true)
                        {
                            TM_UI_Lunch.AllNameInstanceList.Remove(currentName);
                        }
                    }

                    AllLunchList.Remove(foundLunch);
                    Destroy(foundLunch.gameObject);
                }
            }
        }

        protected virtual void OnResponseAddReservedName(string lunchName, string reservedName)
        {
            Debug.LogFormat(LOG_FORMAT, "OnResponseAddReservedName(), lunchName : " + lunchName + ", reservedName : " + reservedName);

            // 예약자에 기존 이름이 할당되어있다면 삭제 후 추가해야되기 때문에 콜백을 파라미터로 받음
            TM_UI_Name currentNameInstance = TM_UI_Lunch.AllNameInstanceList.Find(name => name.GetName().Equals(reservedName) == true);

            if (currentNameInstance != null)
            {
                string lunchNameToRemoveName = currentNameInstance.GetLunchTitle();

                TM_TCPClient.Instance.RequestRemoveReservedName(lunchNameToRemoveName, reservedName, OnResponseAfterRemoveReservedName);
                void OnResponseAfterRemoveReservedName(string reservedName)
                {
                    Debug.LogFormat(LOG_FORMAT, "AFTER Add(), lunchName : " + lunchName + ", reservedName : " + reservedName);

                    TM_UI_Lunch foundLunch = AllLunchList.Find(x => x.GetTitle().Equals(lunchName));
                    foundLunch.InstantiateReservedName(reservedName);
                }
            }
            else
            {
                TM_UI_Lunch foundLunch = AllLunchList.Find(x => x.GetTitle().Equals(lunchName));
                foundLunch.InstantiateReservedName(reservedName);
            }
        }

        protected virtual void OnResponseRemoveReservedName(string lunchName, string reservedName)
        {
            Debug.LogFormat(LOG_FORMAT, "OnResponseRemoveReservedName(), lunchName : " + lunchName + ", reservedName : " + reservedName);

            TM_UI_Lunch foundLunch = AllLunchList.Find(x => x.GetTitle().Equals(lunchName) == true);
            List<TM_UI_Name> nameInstanceList = foundLunch.GetNameInstanceList();

            foreach (TM_UI_Name nameInstance in nameInstanceList)
            {
                if (nameInstance.GetName().Equals(reservedName) == true)
                {
                    TM_UI_Lunch.AllNameInstanceList.Remove(nameInstance);
                    Destroy(nameInstance.gameObject);
                }
            }
        }

        protected virtual void OnResponseClearReservedName(string reservedName)
        {
            Debug.LogFormat(LOG_FORMAT, "OnResponseClearReservedName(), reservedName : " + reservedName);

            TM_UI_Name reservedNameInstance = TM_UI_Lunch.AllNameInstanceList.Find(name => name.GetName().Equals(reservedName) == true);
            if (reservedNameInstance != null)
            {
                TM_UI_Lunch.AllNameInstanceList.Remove(reservedNameInstance);
                Destroy(reservedNameInstance.gameObject);
            }
        }
    }
}