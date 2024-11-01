using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UI_FadeInOut : MonoBehaviour
{
    private const string LOG_FORMAT = "<color=white><b>[UI_FadeInOut]</b></color> {0}";

    protected static UI_FadeInOut _instance;
    public static UI_FadeInOut Instance
    {
        get
        {
            return _instance;
        }
        protected set
        {
            _instance = value;
        }
    }

    [SerializeField]
    protected Image image;

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogErrorFormat(LOG_FORMAT, "");
            Destroy(this.gameObject);
            return;
        }

        Debug.Assert(image != null);

        Color _color = image.color;
        _color.a = 0f;
        image.color = _color;
    }

    protected virtual void OnDestroy()
    {
        UniTaskEx.Cancel(this, 0);

        if (Instance != this)
        {
            return;
        }

        Instance = null;
    }

    public virtual async UniTask FadeInAsync(float duration)
    {
        Color targetColor = new Color(image.color.r, image.color.g, image.color.b, 0f);
        await FadeInAsync(duration, targetColor);
    }

    public virtual async UniTask FadeInAsync(float duration, Color color)
    {
        UniTaskEx.Cancel(this, 0);

        Color originColor = image.color;
        Color targetColor = new Color(color.r, color.g, color.b, 0f);
        image.color = new Color(color.r, color.g, color.b, 1f);

        float timer = 0f;
        float normalized = 0f;

        while (timer < duration)
        {
            image.color = Color.Lerp(originColor, targetColor, normalized);

            await UniTaskEx.WaitForFixedUpdate(this, 0);
            timer += Time.deltaTime;
            normalized = timer / duration;
        }
        image.color = targetColor;
    }

    public virtual async UniTask FadeOutAsync(float duration, Color color)
    {
        UniTaskEx.Cancel(this, 0);

        image.color = new Color(color.r, color.g, color.b, 0f);
        Color originColor = image.color;

        float timer = 0f;
        float normalized = 0f;

        while (timer < duration)
        {
            image.color = Color.Lerp(originColor, color, normalized);

            await UniTaskEx.WaitForFixedUpdate(this, 0);
            timer += Time.deltaTime;
            normalized = timer / duration;
        }
        image.color = color;
    }
}
