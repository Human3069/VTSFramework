using _KMH_Framework;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    // �ַε� ����
    public class InMissionHotLoader : MonoBehaviour
    {
        [Header("HotLoad")]
        [SerializeField]
        protected VTSManager.LanguageType languageType;
        [SerializeField]
        protected VTSManager.MissionType missionType;

        [SerializeField]
        protected Enum languageTypeE;

        protected UniTask.Awaiter awater;

        protected virtual void Awake()
        {
            awater = HotLoad().GetAwaiter();
        }

        // �ַε� ���� :
        // 1. Config.xml �Ľ�
        // 2. ���ö����� ���̺� �Ľ�
        // 3. �ó����� ���̺� �Ľ�
        // 4. �˾� ���̺� �Ľ�
        // 5. �̼� ���� ���̺� �Ľ�
        // 6. �½�ũ ���� ���̺� �Ľ�

        // 7. �ʱ�ȭ
        // 8. UID ���

        protected virtual async UniTask HotLoad()
        {
            await UniTask.WaitWhile(PredicateFunc);
            bool PredicateFunc()
            {
                bool isInMissionHandlerNotLoaded = InMissionHandler.Instance == null;
                bool isVTSManagerNotLoaded = VTSManager.Instance == null;

                return isInMissionHandlerNotLoaded == true || isVTSManagerNotLoaded == true;
            }

            VTSManager.Instance._MissionType = missionType;
            VTSManager.Instance._LanguageType = languageType;

            await ConfigurationReader.Instance.WaitUntilReady();

            LocalizeTableReadHandler localize = VTSManager.Instance.LocalizeTable;
            localize.ReadExcel();
            await localize.WaitUntilReady();

            ScenarioTableReadHandler scenario = VTSManager.Instance.ScenarioTable;
            scenario.ReadExcel();
            await scenario.WaitUntilReady();

            PopupTableReadHandler popup = VTSManager.Instance.PopupTable;
            popup.ReadExcel();
            await popup.WaitUntilReady();

            MissionSettingTableReadHandler missionSetting = VTSManager.Instance.MissionSettingTable;
            missionSetting.ReadExcel();
            await missionSetting.WaitUntilReady();

            TaskSettingTableReadHandler taskSetting = VTSManager.Instance.TaskSettingTable;
            taskSetting.ReadExcel();
            await taskSetting.WaitUntilReady();

            InMissionHandler.Instance.InMissionUi.Initialize();

            await UIDManager.Instance.RegisterAllUidObjects();
        }

        public virtual async UniTask WaitUntilReady()
        {
            await UniTask.WaitUntil(PredicateFunc);
            bool PredicateFunc()
            {
                return awater.IsCompleted == true;
            }
        }
    }
}