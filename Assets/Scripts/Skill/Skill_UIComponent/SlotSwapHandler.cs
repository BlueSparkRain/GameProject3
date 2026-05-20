using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
/// <summary>
/// 负责实现Slot可拖拽功能 + 实时检测下方目标槽位
/// </summary>
public class SlotSwaperHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("基础设置")]
    /// <summary>
    /// 是否可拖拽
    /// </summary>
    public bool canDrag = true;

    /// <summary>
    /// 当前技能所在的槽位
    /// </summary>
    private SkillSlot currentSlot;
    /// <summary>
    /// 拖拽时鼠标下方的目标槽位
    /// </summary>
    private SkillSlot targetSlot;

    // 缓存组件
    private RectTransform _rectTransform;
    // 拖拽时的原始位置
    private Vector3 _originPos;
    // 原始父物体
    private Transform _originParent;

    public void InitSlot(SkillSlot slot) {
        _rectTransform = GetComponent<RectTransform>();
        currentSlot = slot;
    }
    /// <summary>
    /// 开始拖拽（只执行一次）
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 不可拖拽直接拦截
        if (!canDrag) return;

        // 记录原始状态
        _originPos = transform.position;
        _originParent = transform.parent;

        // 1. 脱离原父物体，放到Canvas根节点
        //transform.SetParent(transform.root);
        // 2. 强制设为同级最后一个 → 渲染优先级最高（最前方）


        transform.SetParent(transform.parent.parent.parent);
        transform.SetAsLastSibling();
        GetComponent<UnityEngine.UI.Graphic>().raycastTarget = false;

        Debug.Log("开始拖拽");
    }

    /// <summary>
    /// 拖拽中（每帧执行，必须实现！否则拖不动）
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag) return;

        // 【核心】UI跟随鼠标移动
        _rectTransform.position = eventData.position;

        // 【核心】实时检测鼠标下方的Slot
        DetectTargetSlot(eventData);
    }

    /// <summary>
    /// 结束拖拽（只执行一次）
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canDrag) return;

        GetComponent<UnityEngine.UI.Graphic>().raycastTarget = true;

        if (targetSlot != null && targetSlot != currentSlot)
        {
            targetSlot.SwapIcon(currentSlot);
            StartCoroutine(MoveToSlot(targetSlot));
            Debug.Log($"成功放置到槽位：{targetSlot.name}");
        }
        else
        {
            transform.SetParent(_originParent);
            transform.DOLocalMove(Vector3.zero, 0.2f);
            Debug.Log("未检测到目标槽位，回归原位");
        }

        // 清空目标槽位
        targetSlot = null;
    }

    /// <summary>
    /// 移动到目标槽的位置并放入槽内
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    IEnumerator MoveToSlot(SkillSlot slot) {
        yield return null;
        if (transform.parent.parent.parent)
        {
            Debug.Log(transform.parent.parent.parent.gameObject.name + "撒低级的");
            transform.SetParent(transform.parent.parent.parent.parent.parent.parent);
        }
        transform.DOMove(slot.transform.position,0.2f);
        yield return new WaitForSeconds(0.2f);
        transform.SetParent(slot.transform);
        currentSlot= slot;
        slot.SetIcon(GetComponent<SkillIcon>());
    }

    public void MoveToTargetSlot(SkillSlot slot) {

        StartCoroutine(MoveToSlot(slot));
    }
    /// <summary>
    /// 【核心功能】UI射线检测：查找鼠标下方的SkillSlot
    /// </summary>
    private void DetectTargetSlot(PointerEventData eventData)
    {
        targetSlot = null;

        // UI射线检测结果列表
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // 遍历所有击中的UI，找到第一个槽位
        foreach (var hit in results)
        {
            SkillSlot slot = hit.gameObject.GetComponent<SkillSlot>();
            if (slot != null)
            {
                targetSlot = slot;
                Debug.Log($"当前靠近的槽位：{slot.name}");
                break;
            }
        }
    }
}