using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责技能图标的生成和注册（为了解耦，从CharacterSkiller中拆分出来）
/// 负责需要产生技能图标的【技能配置UI界面】及【角色卡面】上生成SkillIcon预制件
/// </summary>
public class SkillIconSpawner : MonoBehaviour{
    public Transform slotsParent;
    private List<SkillSlot> slots = new List<SkillSlot>();
    private List<SkillData> skillDatas = new List<SkillData>();
    public List<SkillData> SkillDatas => skillDatas;
    private List<SkillIcon> currentIcons = new List<SkillIcon>();
    public List<SkillIcon> CurrentIcons => currentIcons;

    [Header("槽位配置")]
    public bool bornBornSlot = false;

    [Header("技能模式")]
    [Tooltip("决定SkillIcon的AutoMode/ATBMode")]
    public E_SkillMode skillMode = E_SkillMode.Auto;

    [Header("拖拽开关")]
    [Tooltip("生成时是否允许鼠标拖拽")]
    [SerializeField] bool _allowDrag = true;

    [Header("动画加载")]
    [Tooltip("逐个加载时每个槽位的生成间隔（秒）")]
    public float slotLoadInterval = 0.05f;

    /// <summary>
    /// 所有槽位+图标加载完成回调（仅协程版本触发）
    /// </summary>
    public System.Action onLoadComplete;
    public List<SkillData> GetSettledSkilldatas(){
        List<SkillData> icons = new List<SkillData>();
        for (int i = 0; i < slotsParent.childCount; i++){
            var icon = slotsParent.GetChild(i).GetComponentInChildren<SkillIcon>();
            if (icon != null)
                icons.Add(icon.SkillData);
        }
        return icons;
    }
    public void UnloadSkills(){
        foreach (var icon in currentIcons)
            SkillIconCaller.UnLoadSkillIcon(E_PoolType.SkillIcon_技能图标, icon.gameObject, 1, false);
        currentIcons.Clear();
        foreach (var slot in slots){
            slot.transform.SetParent(transform);
            SkillIconCaller.UnLoadSkillIcon(E_PoolType.SkillSlot_技能槽位, slot.gameObject, 1, false);
        }
        slots.Clear();
        for (int i = slotsParent.childCount - 1; i >= 0; i--){
            var child = slotsParent.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    /// <summary>只卸载图标，保留槽位对象（面板关闭时避免重复创建槽位）</summary>
    public void UnloadIconsOnly()
    {
        foreach (var icon in currentIcons)
            SkillIconCaller.UnLoadSkillIcon(E_PoolType.SkillIcon_技能图标, icon.gameObject, 1, false);
        currentIcons.Clear();
    }

    /// <summary>确保至少有指定数量的槽位（不足则补建，不删已有）</summary>
    public void EnsureSlots(int count)
    {
        while (slots.Count < count)
        {
            var slot = SkillIconCaller.LoadSkillSlot(slotsParent, false);
            slots.Add(slot);
        }
    }

    /// <summary>在已有空槽上刷新所有图标（槽位不足时自动扩展）</summary>
    public void RefreshIcons(List<SkillData> datas, bool canDrag)
    {
        UnloadIconsOnly();
        this.skillDatas = datas;
        EnsureSlots(datas.Count);
        bool canActuallyDrag = canDrag && _allowDrag;
        for (int i = 0; i < datas.Count; i++)
        {
            var newSkillIcon = SkillIconCaller.LoadSkillIcon(slots[i].transform, canActuallyDrag, false);
            newSkillIcon.InitSkillIcon(datas[i], slots[i], canActuallyDrag);
            newSkillIcon.PendingSkillMode = skillMode;
            currentIcons.Add(newSkillIcon);
        }
    }

    /// <summary>添加单个技能到下一个空槽（槽位不足时自动扩展）</summary>
    public bool AddSkill(SkillData data, bool canDrag)
    {
        int emptyIndex = currentIcons.Count;
        if (emptyIndex >= slots.Count)
            EnsureSlots(slots.Count + 1);
        bool canActuallyDrag = canDrag && _allowDrag;
        var newSkillIcon = SkillIconCaller.LoadSkillIcon(slots[emptyIndex].transform, canActuallyDrag, false);
        newSkillIcon.InitSkillIcon(data, slots[emptyIndex], canActuallyDrag);
        newSkillIcon.PendingSkillMode = skillMode;
        currentIcons.Add(newSkillIcon);
        return true;
    }

    /// <summary>
    /// 一次性加载所有槽位和技能（战斗中/需要立即就绪时使用）
    /// </summary>
    public List<SkillIcon> LoadSlotsAndSkills(int slotNum, List<SkillData> skillDatas, bool canDrag, bool isImmeditely = true)
    {
        UnloadSkills();
        for (int i = 0; i < slotNum; i++)
        {
            var slot = SkillIconCaller.LoadSkillSlot(slotsParent, false);
            slots.Add(slot);
        }
        this.skillDatas = skillDatas;
        skillIcons.Clear();
        bool canActuallyDrag = canDrag && _allowDrag;
        int iconCount = Mathf.Min(slotNum, skillDatas.Count);
        for (int i = 0; i < iconCount; i++)
        {
            var newSkillIcon = SkillIconCaller.LoadSkillIcon(slots[i].transform, canActuallyDrag, false);
            newSkillIcon.InitSkillIcon(skillDatas[i], slots[i], canActuallyDrag);
            newSkillIcon.PendingSkillMode = skillMode;
            skillIcons.Add(newSkillIcon);
        }
        currentIcons = skillIcons;
        return currentIcons;
    }
    List<SkillIcon> skillIcons = new List<SkillIcon>();
}
