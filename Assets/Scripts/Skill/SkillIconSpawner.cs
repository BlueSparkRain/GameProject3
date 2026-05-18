using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责技能图标的生成和注册（为了解耦，从CharacterSkiller中拆分出来）
/// 负责需要产生技能图标的【技能配置UI界面】及【角色卡面】上生成SkillIcon预制件
/// </summary>
public class SkillIconSpawner : MonoBehaviour
{
    //对于像技能配置UI，需挂三份：分别挂在三个区域。
    //对于角色卡面上，需挂两份分别挂在 底部Normal区域 和 右侧ATB区域。
    public Transform slotsParent;
    private List<SkillSlot> slots = new List<SkillSlot>();

    private WaitForSeconds iconDelay;
    private WaitForSeconds slotDelay;
    private WaitForSeconds unloadDelay;

    private List<SkillData> skillDatas = new List<SkillData>();
    public List<SkillData> SkillDatas => skillDatas;

    //记录的所有Icon,用于回收
    private List<SkillIcon> currentIcons = new List<SkillIcon>();
    public List<SkillIcon> CurrentIcons => currentIcons;

    [Header("槽位")]
    public bool bornBornSlot = false;
    private void Start()
    {
        slotDelay = new WaitForSeconds(0.04f);
        iconDelay = new WaitForSeconds(0.06f);
        unloadDelay = new WaitForSeconds(0.02f);
    }


    //最好时每次交换后，直接更新skilldata数据。
    public List<SkillData> GetSettledSkilldatas() { 
        List<SkillData> icons=new List<SkillData>();

        for (int i = 0; i < slotsParent.childCount; i++)
        {
            var icon = slotsParent.GetChild(i).GetComponentInChildren<SkillIcon>();
            if (icon!=null) {
                icons.Add(icon.SkillData);
            }
        }
        return icons;
    }

    public void UnloadSkills()
    {
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(UnloadIcons());
    }
    IEnumerator UnloadIcons()
    {
        ObjectPoolManager pool = GameRoot.GetManager<ObjectPoolManager>();
        for (int i = currentIcons.Count - 1; i >= 0; i--)
        {
            var icon = currentIcons[i];
            currentIcons.Remove(icon);
            SkillIconCaller.UnLoadSkillIcon(E_PoolType.SkillIcon_技能图标, icon.gameObject, 1);
        }

        yield return unloadDelay;
        //将所有的槽位和技能返回池中

        for (int i = slots.Count - 1; i >= 0; i--)
        {
            var slot = slots[i];
            slots.Remove(slot);
            slot.transform.SetParent(transform);//脱离GridLayout的束缚
            SkillIconCaller.UnLoadSkillIcon(E_PoolType.SkillSlot_技能槽位, slot.gameObject, 1);
            yield return unloadDelay;
        }
    }

    public List<SkillIcon> LoadSlotsAndSkills(int slotNum, List<SkillData> skillDatas, bool canDrag, bool isImmeditely = false)
    {
        if (isImmeditely) LoadWholeSlotsImmeditely(slotNum); else LoadWholeSlots(slotNum);
        this.skillDatas = skillDatas;
        currentIcons = isImmeditely ? LoadSkillIconsImmeditely(skillDatas, canDrag) : LoadSkillIcons(skillDatas, canDrag);
        return currentIcons;
    }


    Coroutine unload;
    public void UnloadIconsImmeditle()
    {
        if (unload == null)
            return;
        StopCoroutine(unload);
        ObjectPoolManager pool = GameRoot.GetManager<ObjectPoolManager>();

        for (int i = currentIcons.Count - 1; i >= 0; i--)
        {
            var icon = currentIcons[i];
            currentIcons.Remove(icon);
            pool.ReturnPool(E_PoolType.SkillIcon_技能图标, currentIcons[i].gameObject);
        }

        //将所有的槽位和技能返回池中
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            var slot = slots[i];
            slot.transform.SetParent(transform);//脱离GridLayout的束缚
            pool.ReturnPool(E_PoolType.SkillSlot_技能槽位, slots[i].gameObject);
            slots.Remove(slot);

        }
    }
    IEnumerator LoadAllSlots(int slotNum)
    {

        ObjectPoolManager pool = GameRoot.GetManager<ObjectPoolManager>();
        for (int i = 0; i < slotNum; i++)
        {
            var slot = SkillIconCaller.LoadSkillSlot(slotsParent);
            slots.Add(slot);
            yield return slotDelay;
        }
    }


    List<SkillIcon> skillIcons = new List<SkillIcon>();
    /// <summary>
    /// 有时可以选择直接生成所有槽位（比如Normal区域或ATB区域，槽位的数量是已知的）
    /// </summary>
    void LoadWholeSlots(int slotNum)
    {
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(LoadAllSlots(slotNum));

    }

    void LoadWholeSlotsImmeditely(int slotNum)
    {
        ObjectPoolManager pool = GameRoot.GetManager<ObjectPoolManager>();
        for (int i = 0; i < slotNum; i++)
        {
            var slot = SkillIconCaller.LoadSkillSlot(slotsParent);
            slots.Add(slot);
        }
    }

    List<SkillIcon> LoadSkillIcons(List<SkillData> skillDatas, bool canDrag)
    {
        skillIcons.Clear();
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(LoadAllIcons(skillDatas, canDrag));
        return skillIcons;
    }

    List<SkillIcon> LoadSkillIconsImmeditely(List<SkillData> skillDatas, bool canDrag)
    {
        skillIcons.Clear();
        for (int i = 0; i < skillDatas.Count; i++)
        {
            var newSkillIcon = SkillIconCaller.LoadSkillIcon(slots[i].transform, canDrag);
            newSkillIcon.InitSkillIcon(skillDatas[i], slots[i],canDrag);
            skillIcons.Add(newSkillIcon);
        }
        return skillIcons;
    }

    IEnumerator LoadAllIcons(List<SkillData> skillDatas, bool canDrag)
    {
        for (int i = 0; i < skillDatas.Count; i++)
        {
            var newSkillIcon = SkillIconCaller.LoadSkillIcon(slots[i].transform, canDrag);

            Debug.Log("酱味大鸡:" + skillDatas[i].skill_Name);
            newSkillIcon.InitSkillIcon(skillDatas[i], slots[i], canDrag);
            skillIcons.Add(newSkillIcon);
            yield return iconDelay;
        }
    }

}
