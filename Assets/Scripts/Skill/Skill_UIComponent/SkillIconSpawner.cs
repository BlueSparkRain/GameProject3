using Core;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责技能图标的生成和注册（为了解耦，从CharacterSkiller中拆分出来）
/// 负责需要产生技能图标的【技能配置UI界面】及【角色卡面】上生成SkillIcon预制件
/// </summary>
public class SkillIconSpawner : MonoBehaviour
{
    public Transform slotsParent;
    private List<SkillSlot> slots = new List<SkillSlot>();
    private List<SkillData> skillDatas = new List<SkillData>();
    public List<SkillData> SkillDatas => skillDatas;
    private List<SkillIcon> currentIcons = new List<SkillIcon>();
    public List<SkillIcon> CurrentIcons => currentIcons;

    [Header("槽位配置")]
    public bool bornBornSlot = false;

    [Header("动画加载")]
    [Tooltip("逐个加载时每个槽位的生成间隔（秒）")]
    public float slotLoadInterval = 0.05f;

    /// <summary>
    /// 所有槽位+图标加载完成回调（仅协程版本触发）
    /// </summary>
    public System.Action onLoadComplete;

    public List<SkillData> GetSettledSkilldatas()
    {
        List<SkillData> icons = new List<SkillData>();
        for (int i = 0; i < slotsParent.childCount; i++)
        {
            var icon = slotsParent.GetChild(i).GetComponentInChildren<SkillIcon>();
            if (icon != null)
                icons.Add(icon.SkillData);
        }
        return icons;
    }

    public void UnloadSkills()
    {
        foreach (var icon in currentIcons)
            SkillIconCaller.UnLoadSkillIcon(E_PoolType.SkillIcon_技能图标, icon.gameObject, 1);
        currentIcons.Clear();

        foreach (var slot in slots)
        {
            slot.transform.SetParent(transform);
            SkillIconCaller.UnLoadSkillIcon(E_PoolType.SkillSlot_技能槽位, slot.gameObject, 1);
        }
        slots.Clear();

        for (int i = slotsParent.childCount - 1; i >= 0; i--)
        {
            var child = slotsParent.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 一次性加载所有槽位和技能（战斗中/需要立即就绪时使用）
    /// </summary>
    public List<SkillIcon> LoadSlotsAndSkills(int slotNum, List<SkillData> skillDatas, bool canDrag, bool isImmeditely = true)
    {
        //if (!isImmeditely)
        //{
        //    StartCoroutine(LoadSlotsAndSkillsCoroutine(slotNum, skillDatas, canDrag));
        //    return null;
        //}

        UnloadSkills();

        for (int i = 0; i < slotNum; i++)
        {
            var slot = SkillIconCaller.LoadSkillSlot(slotsParent);
            slots.Add(slot);
        }

        this.skillDatas = skillDatas;
        skillIcons.Clear();
        for (int i = 0; i < skillDatas.Count; i++)
        {
            var newSkillIcon = SkillIconCaller.LoadSkillIcon(slots[i].transform, canDrag);
            newSkillIcon.InitSkillIcon(skillDatas[i], slots[i], canDrag);
            skillIcons.Add(newSkillIcon);
        }

        currentIcons = skillIcons;
        return currentIcons;
    }

    /// <summary>
    /// 逐个加载槽位+技能（协程动画版本，适合面板打开时的入场效果）
    /// 每隔 slotLoadInterval 秒生成一个槽位及其对应图标
    /// </summary>
    IEnumerator LoadSlotsAndSkillsCoroutine(int slotNum, List<SkillData> skillDatas, bool canDrag)
    {
        UnloadSkills();

        this.skillDatas = skillDatas;
        skillIcons.Clear();

        WaitForSeconds delay = new WaitForSeconds(slotLoadInterval);

        for (int i = 0; i < slotNum; i++)
        {
            // 生成槽位
            var slot = SkillIconCaller.LoadSkillSlot(slotsParent);
            slots.Add(slot);

            // 如果该位置有对应的技能数据，一并生成图标
            if (i < skillDatas.Count)
            {
                var newSkillIcon = SkillIconCaller.LoadSkillIcon(slots[i].transform, canDrag);
                newSkillIcon.InitSkillIcon(skillDatas[i], slots[i], canDrag);
                skillIcons.Add(newSkillIcon);
            }

            yield return delay;
        }

        currentIcons = skillIcons;
        onLoadComplete?.Invoke();
        Debug.Log($"[SkillIconSpawner] 动画加载完成: {slotNum}槽位, {skillIcons.Count}图标");
    }

    List<SkillIcon> skillIcons = new List<SkillIcon>();
}
