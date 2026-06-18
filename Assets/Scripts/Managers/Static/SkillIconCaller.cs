using Core;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 负责应对“游戏内所有需要加载技能图标的地方”的需求
/// </summary>
public static class SkillIconCaller{
    static ObjectPoolManager ObjectPoolManager;
    /// <summary>
    /// 将一个技能图标加载到一个技能槽位上
    /// </summary>
    public static SkillIcon LoadSkillIcon(Transform skillSlot, bool canDrag, bool animateRotation = true){
        ObjectPoolManager = GameRoot.GetManager<ObjectPoolManager>();
        var newSkillIcon = ObjectPoolManager.
        GetInstance(E_PoolType.SkillIcon_技能图标);

        newSkillIcon.transform.SetParent(skillSlot.GetChild(0));
        var rect = newSkillIcon.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;

        newSkillIcon.transform.localPosition = Vector3.zero;
        newSkillIcon.transform.localScale = Vector3.zero;
        newSkillIcon.transform.DOKill();
        newSkillIcon.transform.DOScale(1, 0.3f).SetEase(Ease.OutCubic).SetUpdate(true);
        if (animateRotation)
            newSkillIcon.transform.DORotate(new Vector3(0, 0, 360), 0.3f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic).SetUpdate(true);
        return newSkillIcon.GetComponent<SkillIcon>();
    }
    public static SkillSlot LoadSkillSlot(Transform slotsParent, bool animateRotation = true){
        ObjectPoolManager = GameRoot.GetManager<ObjectPoolManager>();
        var slot = ObjectPoolManager.GetInstance(E_PoolType.SkillSlot_技能槽位);
        slot.transform.SetParent(slotsParent);
        slot.transform.localPosition = Vector3.zero;
        slot.transform.localScale = Vector3.zero;
        slot.transform.DOKill();
        slot.transform.DOScale(1, 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);
        if (animateRotation)
            slot.transform.DORotate(new Vector3(0, 0, 360), 0.2f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic).SetUpdate(true);
        return slot.GetComponent<SkillSlot>();
    }
    public static void UnLoadSkillIcon(E_PoolType ePoolType, GameObject obj, float animScale = 0.5f, bool animateRotation = true){
        ObjectPoolManager ??= GameRoot.GetManager<ObjectPoolManager>();
        obj.transform.localScale = Vector3.one;
        obj.transform.DOKill();
        var scaleTween = obj.transform.DOScale(0, 0.2f * animScale).SetEase(Ease.OutQuad).SetUpdate(true);
        if (animateRotation)
            obj.transform.DORotate(new Vector3(0, 0, 360), 0.2f * animScale, RotateMode.FastBeyond360).SetEase(Ease.OutCubic).SetUpdate(true)
                .OnComplete(() => ObjectPoolManager.ReturnPool(ePoolType, obj));
        else
            scaleTween.OnComplete(() => ObjectPoolManager.ReturnPool(ePoolType, obj));
    }
}
