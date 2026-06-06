using System;
using System.Collections.Generic;
using Core;
using UnityEngine;

/// <summary>
/// 装备背包管理器——全局唯一，管理玩家拥有的所有装备(仓库)
/// 负责装备的增删查和JSON存档
/// </summary>
public class EquipBacketManager : MonoGlobalManager, ICanSave_And_Load
{
    List<EquipData> ownedEquipments = new List<EquipData>();
    public IReadOnlyList<EquipData> OwnedEquipments => ownedEquipments;

    public System.Action onEquipListChanged;

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        JsonSaver.InitData<Save_EquipBacket>(this);
    }

    public override void MgrUpdate(float deltaTime) { }

    public void AddEquipment(EquipData data)
    {
        if (data == null || !data.IsValid()) return;
        ownedEquipments.Add(data);
        SaveEquipBacket();
        onEquipListChanged?.Invoke();
    }

    public void RemoveEquipment(string equipId)
    {
        for (int i = ownedEquipments.Count - 1; i >= 0; i--)
        {
            if (ownedEquipments[i].equipId == equipId)
            {
                ownedEquipments.RemoveAt(i);
                SaveEquipBacket();
                onEquipListChanged?.Invoke();
                return;
            }
        }
    }

    /// <summary>按部位筛选装备</summary>
    public List<EquipData> GetBySlot(E_EquipmentSlot slot)
    {
        var result = new List<EquipData>();
        // Accessory1和Accessory2共享同一个饰品牌组
        bool isAccessory = slot == E_EquipmentSlot.Accessory1 || slot == E_EquipmentSlot.Accessory2;
        foreach (var eq in ownedEquipments)
        {
            if (isAccessory)
            {
                if (eq.slot == E_EquipmentSlot.Accessory1 || eq.slot == E_EquipmentSlot.Accessory2)
                    result.Add(eq);
            }
            else if (eq.slot == slot)
            {
                result.Add(eq);
            }
        }
        return result;
    }

    /// <summary>获取所有部位为指定值的装备(精确匹配)</summary>
    public List<EquipData> GetByExactSlot(E_EquipmentSlot slot)
    {
        var result = new List<EquipData>();
        foreach (var eq in ownedEquipments)
        {
            if (eq.slot == slot)
                result.Add(eq);
        }
        return result;
    }

    public void InitBySaveData()
    {
        var data = JsonSaver.Load<Save_EquipBacket>();
        if (data != null && data.equipmentList != null)
            ownedEquipments = data.equipmentList;
        else
            ownedEquipments = new List<EquipData>();
    }

    public void InitBySelf()
    {
        ownedEquipments = new List<EquipData>();
    }

    void SaveEquipBacket()
    {
        JsonSaver.Save(new Save_EquipBacket(ownedEquipments));
    }
}

[Serializable]
public class Save_EquipBacket : IValidatable
{
    public List<EquipData> equipmentList;

    public Save_EquipBacket() { }

    public Save_EquipBacket(List<EquipData> list)
    {
        equipmentList = list;
    }

    public bool IsValid() => true;
}
