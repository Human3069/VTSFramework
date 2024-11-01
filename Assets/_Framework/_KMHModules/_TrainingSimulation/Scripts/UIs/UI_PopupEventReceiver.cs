using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    // �� Ŭ������ ���� ��Ʈ�� ������ �޼ҵ带 ȣ���մϴ�.
    public class UI_PopupEventReceiver : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_PopupEventReceiver]</b></color> {0}";

        private void OnClickVisionGuide_A_0_Popup_1_Button()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickVisionGuide_A_0_Popup_1_Button()");
        }

        private void OnClickVisionGuide_A_0_Popup_2_Button()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickVisionGuide_A_0_Popup_2_Button()");
        }

        private void OnClickVisionGuide_A_1_Popup_1_Button()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickVisionGuide_A_1_Popup_1_Button()");
        }

        private void OnClickVisionGuide_A_1_Popup_2_Button()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickVisionGuide_A_1_Popup_2_Button()");
        }

        private void OnClickVisionGuide_B_Popup_1_Button()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickVisionGuide_B_Popup_1_Button()");
        }
    }
}