using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _KMH_Framework.TodayMenu
{
    public class TM_UI_ToastMessageHandler : MonoBehaviour
    {
        [SerializeField]
        protected TextMeshProUGUI contentText;
        [SerializeField]
        protected GameObject contentObj;
        [SerializeField]
        protected Button hideButton;

        protected virtual void Awake()
        {
            contentObj.SetActive(false);
        }

        public void Show(string text, float duration = 3f, bool inUnclickable = false)
        {
            hideButton.interactable = !inUnclickable;

            contentText.text = text;

            UniTaskEx.Cancel(this, 0);
            ShowAsync(duration).Forget();
        }

        protected async UniTask ShowAsync(float duration)
        {
            contentObj.SetActive(true);
            await UniTaskEx.WaitForSeconds(this, 0, duration);

            contentObj.SetActive(false);
        }

        public void HideForcelly()
        {
            UniTaskEx.Cancel(this, 0);
            contentObj.SetActive(false);

            hideButton.interactable = true;
        }
    }
}