using Cysharp.Threading.Tasks;
using GoogleSheetsToUnity;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class PopupInMission : BaseInMission
    {
        // private const string LOG_FORMAT = "<color=white><b>[PopupInMission]</b></color> {0}";
      
        protected List<UI_InMissionPopup> popupList = new List<UI_InMissionPopup>();

        [Header("=== PopupInMission ===")]
        [SerializeField]
        protected float popupShowDuration = 0.5f;
        [SerializeField]
        protected float popupHideDuration = 0.1f;

        [Space(10)]
        [SerializeField]
        protected UI_InMissionPopup[] inMissionPopupConfigs;

        protected override void Awake()
        {
            base.Awake();

            PostAwake().Forget();
        }

        protected virtual async UniTaskVoid PostAwake()
        {
            await UniTask.WaitWhile(() => VTSManager.Instance == null);
            await UniTask.WaitWhile(() => UI_PopupsPanel.Instance == null);
            await VTSManager.Instance.PopupTable.WaitUntilReady();

            foreach (PopupRow row in VTSManager.Instance.PopupTable.FirstSheet.sortedTableList)
            {
                UI_InMissionPopup foundPopupConfigPrefab = Array.Find<UI_InMissionPopup>(inMissionPopupConfigs, PredicateFunc);
                bool PredicateFunc(UI_InMissionPopup inMissionPopupConfig)
                {
                    string _name = inMissionPopupConfig.name;
                    _name = _name.Replace("Popup_", "");
                    _name = _name.ToInMissionText();

                    return _name.Equals(row.Popup_Config) == true;
                }

                UI_PopupsPanel.Instance.InstantiatePopupConfig(foundPopupConfigPrefab, row);
            }
        }

        public override async UniTask DoInMissionAsync(TaskRow row)
        {
            UIDObject uidObj = UIDManager.Instance.GetUIDObject(row.Uid);
            UI_InMissionPopup popup = uidObj.GetComponent<UI_InMissionPopup>();

            if (row.Direct.Contains("manual") == true)
            {
                Debug.Assert(row.Parameter.Equals("on") == true || row.Parameter.Equals("off") == true);

                popup._InMissionType = InMissionType.Manual;
                popup.gameObject.SetActive(row.Parameter.Equals("on") == true);

                float duration;
                if (row.Parameter.Equals("on") == true)
                {
                    duration = popupShowDuration;
                }
                else
                {
                    duration = popupHideDuration;
                }

                await UniTask.WaitForSeconds(duration);
            }
            else
            {
                popup._InMissionType = InMissionType.Auto;
                popup.gameObject.SetActive(true);

                await UniTask.WaitForSeconds(popupShowDuration);
            }

            if (popupList.Contains(popup) == false)
            {
                popupList.Add(popup);
            }
        }

        protected override void OnBeforeInMissionStepChange(int firstIndex, int secondIndex)
        {
            for (int i = popupList.Count - 1; i >= 0; i--)
            {
                if (popupList[i]._InMissionType == InMissionType.Auto)
                {
                    popupList[i].gameObject.SetActive(false);
                    popupList.Remove(popupList[i]);
                }
            }
        }
    }
}