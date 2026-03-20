using Core;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// UI面板类型枚举（可根据项目拓展）
/// </summary>
public enum UIPanelType
{
    TestPanel,    // 测试面板
    MessagePanel, // 提示面板
    ShopPanel,    // 商店面板
    // 新增面板类型直接在这里添加即可
}


/// <summary>
/// UI面板基类，所有面板需继承此类
/// </summary>
public class UIPanelBase : MonoBehaviour
{
    /// <summary>
    /// 面板唯一标识（类型+序号，如TestPanel_1）
    /// </summary>
    public string PanelUniqueID { get; set; }

    /// <summary>
    /// 面板类型
    /// </summary>
    public UIPanelType PanelType { get; protected set; }

    /// <summary>
    /// 面板的Canvas组件（用于控制层级）
    /// </summary>
    protected Canvas panelCanvas;

    /// <summary>
    /// 面板的CanvasGroup（用于动画/交互控制）
    /// </summary>
    protected CanvasGroup canvasGroup;

    #region 生命周期方法
    /// <summary>
    /// 初始化面板（仅第一次创建时调用）
    /// </summary>
    /// <param name="panelType">面板类型</param>
    /// <param name="uniqueID">唯一标识</param>
    public virtual void Init(UIPanelType panelType, string uniqueID)
    {
        PanelType = panelType;
        PanelUniqueID = uniqueID;

        // 获取核心组件（预制件需提前挂载Canvas和CanvasGroup）
        panelCanvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 初始化Canvas（合批优化：启用RenderMode=ScreenSpace-Overlay，禁用PixelPerfect）
        if (panelCanvas == null)
        {
            panelCanvas = gameObject.AddComponent<Canvas>();
            panelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            panelCanvas.pixelPerfect = false;
            panelCanvas.overrideSorting = true; // 启用自定义层级
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // 子类重写此方法实现自定义初始化
        OnInit();
    }

    /// <summary>
    /// 显示面板（每次打开时调用）
    /// </summary>
    /// <param name="sortingOrder">面板层级</param>
    public virtual void Show(int sortingOrder)
    {
        gameObject.SetActive(true);
        panelCanvas.sortingOrder = sortingOrder;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // 执行入场动画
        PlayEnterAnimation();

        // 子类重写此方法实现自定义显示逻辑
        OnShow();
    }

    /// <summary>
    /// 隐藏面板（可复用，不销毁）
    /// </summary>
    public virtual void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // 执行出场动画，动画结束后隐藏
        PlayExitAnimation(() =>
        {
            gameObject.SetActive(false);
            OnHide();
        });
    }

    /// <summary>
    /// 关闭面板（销毁/回收到对象池）
    /// </summary>
    public virtual void Close()
    {
        OnClose();
        GameRoot.GetManager<UIManager>().RecyclePanel(this);
    }
    #endregion

    #region 动画接口（子类重写实现具体动画）
    /// <summary>
    /// 播放入场动画
    /// </summary>
    protected virtual void PlayEnterAnimation()
    {
        // 默认简单淡入动画（子类可重写为缩放、位移等动画）
        //LeanTween.alphaCanvas(canvasGroup, 1, 0.2f).setEase(LeanTweenType.easeOutQuad);
    }

    /// <summary>
    /// 播放出场动画
    /// </summary>
    /// <param name="onComplete">动画完成回调</param>
    protected virtual void PlayExitAnimation(System.Action onComplete)
    {
        //// 默认简单淡出动画（子类可重写为缩放、位移等动画）
        //LeanTween.alphaCanvas(canvasGroup, 0, 0.2f)
        //    .setEase(LeanTweenType.easeInQuad)
        //    .setOnComplete(onComplete);
    }
    #endregion

    #region 子类重写的自定义方法
    /// <summary>
    /// 自定义初始化逻辑
    /// </summary>
    protected virtual void OnInit() { }

    /// <summary>
    /// 自定义显示逻辑
    /// </summary>
    protected virtual void OnShow() { }

    /// <summary>
    /// 自定义隐藏逻辑
    /// </summary>
    protected virtual void OnHide() { }

    /// <summary>
    /// 自定义关闭逻辑
    /// </summary>
    protected virtual void OnClose() { }
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