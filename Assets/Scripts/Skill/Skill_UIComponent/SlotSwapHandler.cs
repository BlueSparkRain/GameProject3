using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;

/// <summary>
/// 实现SkillIcon拖拽交换 + 实时检测路径上的目标槽位
/// </summary>
public class SlotSwaperHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("编辑设置")]
    /// <summary>
    /// 是否可拖拽
    /// </summary>
    public bool canDrag = true;

    /// <summary>
    /// 当前所在的槽位
    /// </summary>
    private SkillSlot currentSlot;
    /// <summary>
    /// 拖拽时路径上的目标槽位
    /// </summary>
    private SkillSlot targetSlot;

    private RectTransform _rectTransform;
    private Vector3 _originPos;
    private Transform _originParent;

    /// <summary>
    /// 拖拽时图标的临时顶层父节点（Canvas根节点，保证渲染在最上层）
    /// </summary>
    private static Transform _dragRoot;

    public void InitSlot(SkillSlot slot) {
        _rectTransform = GetComponent<RectTransform>();
        currentSlot = slot;
        // 延迟查找Canvas根节点，确保UI已初始化
        if (_dragRoot == null)
            _dragRoot = GetTopCanvas(transform);
    }

    /// <summary>
    /// 向上查找Canvas根节点作为拖拽顶层
    /// </summary>
    static Transform GetTopCanvas(Transform t)
    {
        Canvas top = null;
        var current = t;
        while (current != null)
        {
            var canvas = current.GetComponent<Canvas>();
            if (canvas != null)
                top = canvas;
            current = current.parent;
        }
        return top != null ? top.transform : t.root;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag) return;

        _originPos = transform.position;
        _originParent = transform.parent;

        // 提升到Canvas根节点 + 置为最末层级 = 渲染在所有UI之上
        if (_dragRoot != null)
            transform.SetParent(_dragRoot);
        else
            transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        GetComponent<UnityEngine.UI.Graphic>().raycastTarget = false;

        Debug.Log("开始拖拽");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag) return;

        _rectTransform.position = eventData.position;
        DetectTargetSlot(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canDrag) return;

        GetComponent<UnityEngine.UI.Graphic>().raycastTarget = true;

        if (targetSlot != null && targetSlot != currentSlot)
        {
            targetSlot.SwapIcon(currentSlot);
            StartCoroutine(MoveToSlot(targetSlot));
            Debug.Log($"成功移动到槽位:{targetSlot.name}");
        }
        else
        {
            StartCoroutine(ReturnToOrigin());
            Debug.Log("未检测到目标槽位，返回原位置");
        }

        targetSlot = null;
    }

    IEnumerator MoveToSlot(SkillSlot slot)
    {
        // 先动画移动到目标槽位世界坐标（此时图标仍在Canvas顶层）
        yield return null;
        Tween moveTween = transform.DOMove(slot.transform.position, 0.2f);
        yield return moveTween.WaitForCompletion();

        // 动画结束后嵌入目标槽位
        transform.SetParent(slot.transform);
        transform.localPosition = Vector3.zero;
        currentSlot = slot;
        slot.SetIcon(GetComponent<SkillIcon>());
    }

    IEnumerator ReturnToOrigin()
    {
        yield return null;
        Tween moveTween = transform.DOMove(_originPos, 0.2f);
        yield return moveTween.WaitForCompletion();

        if (_originParent != null)
        {
            transform.SetParent(_originParent);
            transform.localPosition = Vector3.zero;
        }
    }

    public void MoveToTargetSlot(SkillSlot slot)
    {
        StartCoroutine(MoveToSlot(slot));
    }

    private void DetectTargetSlot(PointerEventData eventData)
    {
        targetSlot = null;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var hit in results)
        {
            SkillSlot slot = hit.gameObject.GetComponent<SkillSlot>();
            if (slot != null)
            {
                targetSlot = slot;
                Debug.Log($"当前穿过的槽位{slot.name}");
                break;
            }
        }
    }
}
