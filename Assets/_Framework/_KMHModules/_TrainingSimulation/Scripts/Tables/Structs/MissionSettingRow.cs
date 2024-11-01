
namespace VTSFramework.TSModule
{
    [System.Serializable]
    public struct MissionSettingRow : IRow<MissionSettingRow>
    {
        public MissionSettingRow(string uid, string[] orders)
        {
            this.Uid = uid;
            this.Orders = orders;
            this.Order = "";
        }

        public MissionSettingRow Validated()
        {
            int missionIndex = (int)(VTSManager.MissionType)VTSManager.Instance._MissionType;

            this.Uid = Uid.ToInMissionText();
            this.Order = Orders[missionIndex].ToInMissionText();
            this.Orders = null;

            return this;
        }

        [ReadOnly]
        public string Uid;
        [ReadOnly]
        public string[] Orders; // ��� �̼��� ���ð����� ������
        [ReadOnly]
        public string Order; // ���� �̼��� ���ð��� ������
    }
}