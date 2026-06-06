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
    /// <param name="skillSlot"></param>
    public static SkillIcon LoadSkillIcon(Transform skillSlot, bool canDrag){
        ObjectPoolManager = GameRoot.GetManager<ObjectPoolManager>();
        //产生对应的skillIcon
        var newSkillIcon = ObjectPoolManager.
        GetInstance(E_PoolType.SkillIcon_技能图标);

        newSkillIcon.transform.SetParent(skillSlot);
        var rect = newSkillIcon.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);    // 锚点左下角
        rect.anchorMax = new Vector2(1, 1);    // 锚点右上角
        rect.offsetMin = Vector2.zero;         // 左/下偏移=0
        rect.offsetMax = Vector2.zero;         // 右/上偏移=0
        rect.pivot = new Vector2(0.5f, 0.5f);  // 中心点居中
        rect.anchoredPosition = Vector2.zero;  // 位置居中
        rect.localScale = Vector3.one;         // 重置缩放

        newSkillIcon.transform.localPosition = Vector3.zero;
        newSkillIcon.transform.localScale = Vector3.zero;
        newSkillIcon.transform.DOScale(1, 0.6f).From(0).SetEase(Ease.OutCubic);
        newSkillIcon.transform.DORotate(new Vector3(0, 0, 360), 0.8f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic);
        return newSkillIcon.GetComponent<SkillIcon>();
    }
    public static SkillSlot LoadSkillSlot(Transform slotsParent){
        ObjectPoolManager = GameRoot.GetManager<ObjectPoolManager>();
        var slot = ObjectPoolManager.GetInstance(E_PoolType.SkillSlot_技能槽位);
        slot.transform.SetParent(slotsParent);
        slot.transform.localPosition = Vector3.zero;
        slot.transform.localScale = Vector3.zero;
        slot.transform.DOScale(1, 0.4f).From(0).SetEase(Ease.OutCubic);
        slot.transform.DORotate(new Vector3(0, 0, 360), 0.8f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic);
        return slot.GetComponent<SkillSlot>();
    }
    public static void UnLoadSkillIcon(E_PoolType ePoolType, GameObject obj, float animScale = 0.5f)
    {
        //ObjectPoolManager ??= GameRoot.GetManager<ObjectPoolManager>();
        obj.transform.DOScale(0, 0.6f * animScale).From(1).SetEase(Ease.OutQuad);
        obj.transform.DORotate(new Vector3(0, 0, 360), 0.4f * animScale, RotateMode.FastBeyond360).SetEase(Ease.OutCubic)
            .OnComplete(() => ObjectPoolManager.ReturnPool(ePoolType, obj));
    }
}
