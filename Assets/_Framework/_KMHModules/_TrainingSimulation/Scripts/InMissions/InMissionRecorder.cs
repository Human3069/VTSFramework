using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class InMissionRecorder : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[InMissionRecorder]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        protected List<InMissionRecord> inMissionRecordList = new List<InMissionRecord>();

        protected InteractionInMission interactionInMission;
        protected CameraInMission cameraInMission;
        protected PopupInMission popupInMission;

        public virtual void Initialize()
        {
            interactionInMission = this.GetComponentInChildren<InteractionInMission>();
            cameraInMission = this.GetComponentInChildren<CameraInMission>();
            popupInMission = this.GetComponentInChildren<PopupInMission>();

            List<TaskRow> rowList = VTSManager.Instance.ScenarioTable.GetSortedList();
            List<TaskRow> hasPointRowList = rowList.FindAll(row => row.Record_Point.Equals("o") == true);

            foreach (TaskRow row in rowList)
            {
                if (inMissionRecordList.Count == 0)
                {
                    // 첫 번째 레코드를 생성할 땐 TaskRow를 기준으로 할당 (생성자 오버로딩)
                    inMissionRecordList.Add(new InMissionRecord(row, interactionInMission, cameraInMission, popupInMission));
                }
                else
                {
                    // 두 번째 이상의 레코드를 생성할 땐 직전 인덱스의 레코드 및 TaskRow를 기준으로 할당 (생성자 오버로딩)
                    inMissionRecordList.Add(new InMissionRecord(inMissionRecordList[inMissionRecordList.Count - 1], row, interactionInMission, cameraInMission, popupInMission));
                }
            }

            // 해당 카메라 이동 단계에서는 이전 위치에 있을 것이므로 카메라 포인트는 한 단계씩 땡겨준다.
            for (int i = inMissionRecordList.Count - 1; i >= 0; i--)
            {
                if (i != 0)
                {
                    inMissionRecordList[i].CamPointUid = inMissionRecordList[i].Previous.CamPointUid;
                }

                if (i == inMissionRecordList.Count - 1)
                {
                    inMissionRecordList[i].Next = null;
                }
                else
                {
                    inMissionRecordList[i].Next = inMissionRecordList[i + 1];
                }
            }
        }

        public virtual void ReadRecord(int currentFirstIndex, int currentSecondIndex, bool isAscending, out int firstIndex, out int secondIndex)
        {
            List<InMissionRecord> currentRecordList = inMissionRecordList.FindAll(GetCurrentPredicate);
            bool GetCurrentPredicate(InMissionRecord record)
            {
                bool isFirstEquals = record.FirstIndex == currentFirstIndex;
                bool isSecondEquals = record.SecondIndex == currentSecondIndex;
                
                return isFirstEquals && isSecondEquals;
            }

            InMissionRecord edgeRecord;
            int startIndex;
            int endIndex;

            if (isAscending == true)
            {
                edgeRecord = currentRecordList[currentRecordList.Count - 1];
                startIndex = inMissionRecordList.IndexOf(edgeRecord) + 1;
                endIndex = inMissionRecordList.FindIndex(startIndex, record => record.HasRecordPoint == true);
            }
            else
            {
                edgeRecord = currentRecordList[0];
                startIndex = inMissionRecordList.IndexOf(edgeRecord) - 1;
                if (startIndex == -1)
                {
                    firstIndex = currentFirstIndex;
                    secondIndex = currentSecondIndex;

                    return;
                }

                endIndex = inMissionRecordList.FindLastIndex(startIndex, record => record.HasRecordPoint == true);
                if (endIndex == -1)
                {
                    firstIndex = currentFirstIndex;
                    secondIndex = currentSecondIndex;

                    return;
                }
            }

            List<InMissionRecord> targetRecordList = inMissionRecordList.GetRange(startIndex, startIndex - endIndex + 1);
            foreach (InMissionRecord record in targetRecordList)
            {
                cameraInMission.DoInMissionRecorded(record.CamPointUid);
            }

            firstIndex = inMissionRecordList[endIndex].FirstIndex;
            secondIndex = inMissionRecordList[endIndex].SecondIndex;
        }
    }

    [System.Serializable]
    public class InMissionRecord
    {
        public InMissionRecord(TaskRow row, InteractionInMission interaction, CameraInMission camera, PopupInMission popup)
        {
            this.Previous = null;

            this.HasRecordPoint = row.Record_Point.Equals("o");
            this.FirstIndex = row.First_Index;
            this.SecondIndex = row.Second_Index;

            if (interaction.IsDirectContains(row.Direct) == true)
            {
                this.InteractedUid = row.Uid;
            }
            else
            {
                this.InteractedUid = "";
            }

            if (camera.IsDirectContains(row.Direct) == true)
            {
                this.CamPointUid = row.Uid;
            }
            else
            {
                this.CamPointUid = InMissionHandler.StartData.GetStartPoint().UidValue;
            }

            if (popup.IsDirectContains(row.Direct) == true &&
                row.Direct.Contains("manual") == true)
            {
                if (row.Parameter.Contains("on") == true)
                {
                    this.ManualPopupUid = row.Uid;
                }
                else
                {
                    this.ManualPopupUid = "";
                }
            }
            else
            {
                this.ManualPopupUid = "";
            }
        }

        public InMissionRecord(InMissionRecord prevRecord, TaskRow row, InteractionInMission interaction, CameraInMission camera, PopupInMission popup) : this(row, interaction, camera, popup)
        {
            this.Previous = prevRecord;

            if (camera.IsDirectContains(row.Direct) == false)
            {
                this.CamPointUid = Previous.CamPointUid;
            }

            if (popup.IsDirectContains(row.Direct) == false)
            {
                this.ManualPopupUid = Previous.ManualPopupUid;
            }
        }

        [HideInInspector]
        public InMissionRecord Previous;
        [HideInInspector]
        public InMissionRecord Next;

        [ReadOnly]
        public bool HasRecordPoint;
        [ReadOnly]
        public int FirstIndex;
        [ReadOnly]
        public int SecondIndex;

        [Space(10)]
        [ReadOnly]
        public string InteractedUid;
        [ReadOnly]
        public string CamPointUid;
        [ReadOnly]
        public string ManualPopupUid;
    }
}