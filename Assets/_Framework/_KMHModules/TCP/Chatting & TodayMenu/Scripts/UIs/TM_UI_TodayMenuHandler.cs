using _KMH_Framework._TS_Module;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _KMH_Framework.TodayMenu
{
    public class TM_UI_TodayMenuHandler : MonoBehaviour
    {
        [SerializeField]
        protected Toggle chattingToggle;
        [SerializeField]
        protected Toggle lunchToggle;

        [Space(10)]
        [SerializeField]
        protected GameObject chattingPanelObj;
        [SerializeField]
        protected GameObject lunchPanelObj;

        protected virtual void Awake()
        {
            // Screen.SetResolution(800, 600, FullScreenMode.FullScreenWindow);
        }

        protected virtual void Start()
        {
            chattingToggle.onValueChanged.AddListener(OnValueChangedChattingToggle);
            void OnValueChangedChattingToggle(bool isOn)
            {
                chattingPanelObj.SetActive(isOn);
            }

            lunchToggle.onValueChanged.AddListener(OnValueChangedLunchToggle);
            void OnValueChangedLunchToggle(bool isOn)
            {
                lunchPanelObj.SetActive(isOn);
            }

            chattingPanelObj.SetActive(true);
            lunchPanelObj.SetActive(false);
        }
    }
}