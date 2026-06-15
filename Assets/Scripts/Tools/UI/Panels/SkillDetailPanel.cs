using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SkillDetailPanel : UIPanelBase
{
    [Header("技能描述文本")]
    public Text descriptionText;

    [Header("缩放动画参数")]
    [SerializeField] float scaleDuration = 0.25f;
    [SerializeField] Ease scaleEase = Ease.OutBack;

    RectTransform _rect;
    Tweener _tween;
    static SkillDetailPanel _instance;

    protected override void OnInit()
    {
        _instance = this;
        _rect = GetComponent<RectTransform>();
        var canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _rect.localScale = Vector3.zero;
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    /// <summary>拖拽等操作时强制立即关闭tooltip（无动画）</summary>
    public static void ForceHide()
    {
        if (_instance == null) return;
        _instance._tween?.Kill();
        _instance.canvasGroup.alpha = 0;
        _instance.gameObject.SetActive(false);
    }

    public override void Show()
    {
    }

    public override void Hide()
    {
    }

    public void ShowTooltip(Vector2 screenPosition, string desc)
    {
        if (_rect == null) return;
        _tween?.Kill();
        _rect.localScale = Vector3.zero;

        descriptionText.text = desc;
        _rect.position = screenPosition;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        canvasGroup.alpha = 1;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        _tween = _rect.DOScale(1f, scaleDuration).SetEase(scaleEase).SetUpdate(true);
    }

    public void HideTooltip()
    {
        if (_rect == null) return;
        _tween?.Kill();
        _rect.localScale = Vector3.one;

        _tween = _rect.DOScale(0f, scaleDuration).SetEase(scaleEase).SetUpdate(true)
            .OnComplete(() =>
            {
                if (_rect == null) return;
                canvasGroup.alpha = 0;
                gameObject.SetActive(false);
            });
    }
}
