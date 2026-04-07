using Core;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using static AmplifyShaderEditor.WindowsUtil;

/// <summary>
/// 负责技能图标的生成和注册（为了解耦，从CharacterSkiller中拆分出来）
/// 负责需要产生技能图标的【技能配置UI界面】及【角色卡面】上生成SkillIcon预制件
/// </summary>
public class SkillIconSpawner : MonoBehaviour
{
    //对于像技能配置UI，需挂三份：分别挂在三个区域。
    //对于角色卡面上，需挂两份分别挂在 底部Normal区域 和 右侧ATB区域。
    public Transform slotsParent;
    private List<SkillSlot> slots=new List<SkillSlot>();
    private WaitForSeconds iconDelay;
    private WaitForSeconds slotDelay;

    private WaitForSeconds unloadDelay;

    private List<SkillData> skillDatas=new List<SkillData>();
    public List<SkillData> SkillDatas =>skillDatas;

    //记录的所有Icon,用于回收
    private List<SkillIcon> currentIcons=new List<SkillIcon>();
    public List<SkillIcon> CurrentIcons=>currentIcons;

    [Header("槽位")]
    public bool bornBornSlot = false;
    private void Start()
    {
        slotDelay=new WaitForSeconds(0.04f);
        iconDelay=new WaitForSeconds(0.06f);
        unloadDelay = new WaitForSeconds(0.02f);
    }
    //在拖拽交换后，Slot会向spawner更新技能列表
    void UpdateDataList() { 
    
    }

    public void UnloadSkills() {
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(UnloadIcons());
    }
    IEnumerator UnloadIcons()
    {
        ObjectPoolManager pool = GameRoot.GetManager<ObjectPoolManager>();
        for (int i = currentIcons.Count - 1; i >= 0; i--)
        {
            var icon = currentIcons[i];
            currentIcons.Remove(icon);
            icon.transform.DOScale(0, 0.02f).From(1).SetEase(Ease.OutQuad);
            icon.transform.DORotate(new Vector3(0, 0, 360), 0.03f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad)
                .OnComplete(() => pool.ReturnPool(EPoolType.SkillIcon_技能图标, currentIcons[i].gameObject));
        }
        yield return unloadDelay;
        //将所有的槽位和技能返回池中
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            var slot = slots[i];
            slots.Remove(slot);

            slot.transform.DOScale(0, 0.2f).From(1).SetEase(Ease.OutQuad);
            slot.transform.DORotate(new Vector3(0, 0, 360), 0.3f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad)
                .OnComplete(() => pool.ReturnPool(EPoolType.SkillSlot_技能槽位, slots[i].gameObject));
            yield return unloadDelay;
        }
        Debug.Log("卸载完毕大师的话hi无敌");
    }

    public List<SkillIcon> LoadSlotsAndSkills(int slotNum, List<SkillData> skillDatas,bool canDrag) {
        Debug.Log("加载架子哎！！！");
        LoadWholeSlots(slotNum);
        this.skillDatas=skillDatas;
        currentIcons = LoadSkillIcons(skillDatas,canDrag);
        return currentIcons;
    }

    /// <summary>
    /// 有时可以选择直接生成所有槽位（比如Normal区域或ATB区域，槽位的数量是已知的）
    /// </summary>
    void LoadWholeSlots(int slotNum) {
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(slotsAnim(slotNum));

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
            pool.ReturnPool(EPoolType.SkillIcon_技能图标, currentIcons[i].gameObject);
        }

        //将所有的槽位和技能返回池中
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            var slot = slots[i];
            slot.transform.SetParent(transform);
            Debug.Log(i + "???" + slots.Count);
            pool.ReturnPool(EPoolType.SkillSlot_技能槽位, slots[i].gameObject);
            slots.Remove(slot);

        }

    }
    IEnumerator slotsAnim(int slotNum) {

        ObjectPoolManager pool = GameRoot.GetManager<ObjectPoolManager>();
        for (int i = 0; i < slotNum; i++)
        {
            var slot = pool.GetInstance(EPoolType.SkillSlot_技能槽位);
            slot.transform.SetParent(slotsParent);
            slot.transform.localPosition = Vector3.zero;
            slot.transform.localScale = Vector3.zero;
            slot.transform.DOScale(1, 0.4f).From(0).SetEase(Ease.OutQuad);
            slot.transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad);
            slots.Add(slot.GetComponent<SkillSlot>());
            yield return slotDelay;
        }
    }

    void LoadSkillSlots()
    {
        //首次加载技能图标前，先根据排布需求来生成所有技能槽位
    }
    
    List<SkillIcon> skillIcons=new List<SkillIcon>();


    List<SkillIcon> LoadSkillIcons(List<SkillData> skillDatas,bool canDrag)
    {
        skillIcons.Clear();
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(IconsAnim(skillDatas,canDrag));
        return skillIcons;
    }

    IEnumerator IconsAnim(List<SkillData> skillDatas, bool canDrag) {

        //yield return iconDelay;
        for (int i = 0; i < skillDatas.Count; i++)
        {
            //产生对应的skillIcon
            var newSkillIcon = GameRoot.GetManager<ObjectPoolManager>().
            GetInstance(EPoolType.SkillIcon_技能图标).GetComponent<SkillIcon>();
            try
            {
                newSkillIcon.transform.SetParent(slots[i].transform);
                var rect = newSkillIcon.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0);    // 锚点左下角
                rect.anchorMax = new Vector2(1, 1);    // 锚点右上角
                rect.offsetMin = Vector2.zero;         // 左/下偏移=0
                rect.offsetMax = Vector2.zero;         // 右/上偏移=0
                rect.pivot = new Vector2(0.5f, 0.5f);  // 中心点居中
                rect.anchoredPosition = Vector2.zero;  // 位置居中
                rect.localScale = Vector3.one;         // 重置缩放

                // 4. 关闭保持比例（强制拉伸铺满，需要比例就设为true）
                //childImg.preserveAspect = false;
                newSkillIcon.transform.localPosition = Vector3.zero;
                newSkillIcon.transform.localScale = Vector3.zero;
                newSkillIcon.transform.DOScale(1, 0.4f).From(0).SetEase(Ease.OutQuad);
                newSkillIcon.transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad);
                newSkillIcon.InitSkillIcon(skillDatas[i], canDrag);
                skillIcons.Add(newSkillIcon);
            }
            catch { 
            
            }
                yield return iconDelay;
        }

        //Debug.Log("所有Icon架子啊完毕！！");
    }

}
