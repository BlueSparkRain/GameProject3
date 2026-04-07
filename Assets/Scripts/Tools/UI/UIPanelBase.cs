using Core;
using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


[RequireComponent(typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster))]
/// <summary>
/// UI面板基类，所有面板需继承此类
/// </summary>
public class UIPanelBase : MonoBehaviour
{
    [SerializeField]
    [Header("面板根")]
    protected RectTransform panelRoot;

    [Space(10)]
    [Header("AnimInfo")]
    [SerializeField]
    [Header("动画-时长")]
    protected float Anim_Duration = 0.8f;
    [SerializeField]
    [Header("动画-Dotween缓动类型")]
    protected Ease Anim_EaseType = Ease.OutBack;
    [SerializeField]
    [Header("动画-出生位置")]
    protected Vector3 Anim_BornPos = new Vector3(0, -1000, 0);
    [SerializeField]
    [Header("动画-过渡位移")]
    protected Vector3 Anim_TargetTrans = new Vector3(0, 1000, 0);

    [SerializeField]
    [Header("动画-入场状态标识")]
    protected bool Anim_DoFadeIn = true;

    [SerializeField]
    [Header("动画-需要透明渐变")]
    protected bool Anim_NeedAlphaFadeIn;


    public bool canOpen=true;


    /// <summary>
    /// 面板唯一标识（类型+序号，如TestPanel_1）
    /// </summary>
    public string PanelUniqueID { get; set; }

    /// <summary>
    /// 面板类型
    /// </summary>

    public E_UIPanelType PanelType { get; protected set; }


    public string PanelID { get; private set; }

    /// <summary>
    /// 面板的Canvas组件（用于控制层级）
    /// </summary>
    protected Canvas panelCanvas;

    /// <summary>
    /// 面板的CanvasGroup（用于动画/交互控制）
    /// </summary>
    protected CanvasGroup canvasGroup;

    WaitForSeconds animDelay;

    #region 生命周期方法
    /// <summary>
    /// 初始化面板（仅第一次创建时调用）
    /// </summary>
    /// <param name="panelType">面板类型</param>
    /// <param name="uniqueID">唯一标识</param>
    public virtual void Init(E_UIPanelType type, string uniqueID)
    {
        PanelType = type;
        PanelID = uniqueID;
        animDelay = new WaitForSeconds(Anim_Duration);
        panelRoot = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 初始状态：隐藏
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        MagicAnimExtens.ResetRecTransPos(panelRoot, Anim_BornPos);

        // 子类重写此方法实现自定义初始化
        OnInit();
    }

    /// <summary>
    /// 显示面板（每次打开时调用）
    /// </summary>
    /// <param name="sortingOrder">面板层级</param>
    public virtual void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        if (!Anim_DoFadeIn)
            BeforeFadeOutAnimCallBack();
        else
            BeforeFadeInAnimCallBack();

        UnitBeforeAnimCallBack();
        
        // 执行入场动画
        PlayEnterAnim(()=>{ 
        EnterAnimCallBack();
        });
        UnitEndAnimCallBack();
    }

    /// <summary>
    /// 隐藏面板（可复用，不销毁）
    /// </summary>
    public virtual void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        if (!Anim_DoFadeIn)
            BeforeFadeOutAnimCallBack();
        else
            BeforeFadeInAnimCallBack();

        UnitBeforeAnimCallBack();

        PlayExitAnim(() =>
        {
            gameObject.SetActive(false);
            ExitAnimCallBack();
        });

        UnitEndAnimCallBack();
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    public virtual void Close()
    {
        Hide();
        OnClose();
        GameRoot.GetManager<UIManager>().ClosePanel(PanelType);

        GameRoot.GetManager<CoroutineManager>().StartDelayedCoroutine(Anim_Duration,
            () => DestroyImmediate(gameObject));
    }
    #endregion

    #region 动画接口（子类重写实现具体动画）
    /// <summary>
    /// 播放入场动画
    /// </summary>
    protected virtual void PlayEnterAnim(System.Action onComplete)
    {
        StartCoroutine(WaitCallBack(onComplete));

        MagicAnimExtens.DoLocal_UIAnim(
         panelRoot, Anim_Duration, Anim_EaseType,
         Anim_BornPos, Anim_TargetTrans,
         Anim_DoFadeIn, Anim_NeedAlphaFadeIn);
    }
    IEnumerator WaitCallBack(System.Action onComplete) {
        yield return animDelay;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 播放出场动画
    /// </summary>
    /// <param name="onComplete">动画完成回调</param>
    protected virtual void PlayExitAnim(System.Action onComplete)
    {
        StartCoroutine(WaitCallBack(onComplete));

        MagicAnimExtens.DoLocal_UIAnim(
       panelRoot, Anim_Duration, Anim_EaseType,
       Anim_BornPos, Anim_TargetTrans,
       Anim_DoFadeIn, Anim_NeedAlphaFadeIn);
    }
    #endregion

    #region 子类重写的自定义方法
    /// <summary>
    /// 自定义初始化逻辑
    /// </summary>
    protected virtual void OnInit() { }

    /// <summary>
    /// 入场动画后回调
    /// </summary>
    protected virtual void EnterAnimCallBack() {
    }

    /// <summary>
    /// 离场动画后回调
    /// </summary>
    protected virtual void ExitAnimCallBack() { }

    /// <summary>
    /// 淡出前执行的委托
    /// </summary>
    protected virtual void BeforeFadeInAnimCallBack() { }

    /// <summary>
    /// 淡出前执行的委托
    /// </summary>
    protected virtual void BeforeFadeOutAnimCallBack(){}
    protected virtual void UnitBeforeAnimCallBack()
    {
 
    }

    protected virtual void UnitEndAnimCallBack() {
        Anim_DoFadeIn = !Anim_DoFadeIn;
    }
    /// <summary>
    /// 自定义关闭逻辑
    /// </summary>
    protected virtual void OnClose()
    {

    }
    #endregion

    #region 层级控制
    /// <summary>
    /// 更新面板层级（置顶时调用）
    /// </summary>
    /// <param name="newSortingOrder">新层级</param>
    public void UpdateSortingOrder(int newSortingOrder)
    {
        panelCanvas.sortingOrder = newSortingOrder;
    }
    #endregion
}