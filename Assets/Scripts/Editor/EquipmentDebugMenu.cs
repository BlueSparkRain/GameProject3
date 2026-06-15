using System.Text;
using Core;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 装备系统调试菜单——编辑器顶部 Tools/装备系统 下的调试按钮
/// </summary>
public static class EquipmentDebugMenu
{
    const string MenuRoot = "Tools/装备系统/";

    [MenuItem(MenuRoot + "清除装备存档", false, 10)]
    static void ClearSaveData()
    {
        string path = System.IO.Path.Combine(
            Application.persistentDataPath, "GameSaves", "Save_EquipBacket.xjson");

        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
            Debug.Log($"[装备调试] 已删除存档: {path}");
        }
        else
            Debug.Log("[装备调试] 存档文件不存在，无需清除");

        if (Application.isPlaying)
        {
            var mgr = GameRoot.GetManager<EquipBacketManager>();
            if (mgr != null)
            {
                // 清空运行时内存数据并重新保存空存档
                var removeList = new System.Collections.Generic.List<EquipData>(mgr.OwnedEquipments);
                foreach (var eq in removeList)
                    mgr.RemoveEquipment(eq.equipId);
                Debug.Log($"[装备调试] 已清空运行时背包 ({removeList.Count}件 → 0件)");
            }
        }

        AssetDatabase.Refresh();
    }

    [MenuItem(MenuRoot + "打印背包装备", false, 11)]
    static void PrintBackpack()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[装备调试] 请在运行模式下使用此功能");
            return;
        }

        var mgr = GameRoot.GetManager<EquipBacketManager>();
        if (mgr == null)
        {
            Debug.LogError("[装备调试] EquipBacketManager 未注册");
            return;
        }

        var list = mgr.OwnedEquipments;
        Debug.Log($"========== 背包装备 (共{list.Count}件) ==========");

        for (int i = 0; i < list.Count; i++)
        {
            var eq = list[i];
            var sb = new StringBuilder();
            sb.AppendLine($"[{i}] {eq.GetEquipName()}");
            sb.AppendLine($"    ID: {eq.equipId}");
            sb.AppendLine($"    部位: {eq.GetSlotName()}");
            sb.AppendLine($"    混沌等级: {eq.chaosLevel}");
            sb.AppendLine($"    售价: {eq.price}G");
            if (eq.affixes != null)
            {
                sb.Append("    词条:");
                foreach (var affix in eq.affixes)
                    sb.Append($" +{affix.value} {EquipData.GetAffixTypeName(affix.type)}");
                sb.AppendLine();
            }
            sb.AppendLine($"    弱点: {eq.weakness}");
            Debug.Log(sb.ToString());
        }

        Debug.Log("========== 打印完毕 ==========");
    }

    [MenuItem(MenuRoot + "打印已装备信息", false, 12)]
    static void PrintEquipped()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[装备调试] 请在运行模式下使用此功能");
            return;
        }

        var tags = Object.FindObjectsOfType<CharacterHandler>();
        CharacterHandler playerTag = null;
        foreach (var tag in tags)
        {
            if (tag.isPlayer)
            {
                playerTag = tag;
                break;
            }
        }

        if (playerTag == null)
        {
            Debug.LogWarning("[装备调试] 未找到玩家 CharacterHandler");
            return;
        }

        var handler = playerTag.CharacterData?.EquipHandler;
        if (handler == null)
        {
            Debug.LogWarning("[装备调试] EquipHandler 未初始化");
            return;
        }

        Debug.Log("========== 当前已装备 ==========");
        foreach (var kv in handler.CurrentEquips)
        {
            var eq = kv.Value;
            Debug.Log($"[{kv.Key}] {eq.GetEquipName()} | 弱点:{eq.weakness} | Lv{ eq.chaosLevel}");
        }

        // 绿值汇总
        Debug.Log("--- 绿值加成汇总 ---");
        Debug.Log($"  生命值: +{handler.GetGreenBonus(E_CharacterPropertyType.Maximum_Health)}");
        Debug.Log($"  法力值: +{handler.GetGreenBonus(E_CharacterPropertyType.Maximum_Mana)}");
        Debug.Log($"  物攻:   +{handler.GetGreenBonus(E_CharacterPropertyType.Phy_Attack)}");
        Debug.Log($"  魔攻:   +{handler.GetGreenBonus(E_CharacterPropertyType.Mag_Attack)}");
        Debug.Log($"  物抗:   +{handler.GetGreenBonus(E_CharacterPropertyType.Phy_Resistance)}");
        Debug.Log($"  魔抗:   +{handler.GetGreenBonus(E_CharacterPropertyType.Mag_Resistance)}");
        Debug.Log($"  护盾:   +{handler.GetShieldBonus()}");
        Debug.Log("========== 打印完毕 ==========");
    }

    [MenuItem(MenuRoot + "生成示例EquipmentSO(40件)", false, 5)]
    static void GenerateSampleSOs()
    {
        string dir = "Assets/Resources/SOData/EquipmentSOData";
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);

        // (部位, 名称, 售价)[]
        var data = new (E_EquipmentSlot slot, string name, int price)[]
        {
            // ===== 剑 (Sword) =====
            (E_EquipmentSlot.Sword, "铁剑",     100),
            (E_EquipmentSlot.Sword, "钢剑",     250),
            (E_EquipmentSlot.Sword, "秘银剑",   500),
            (E_EquipmentSlot.Sword, "龙鳞剑",   800),
            (E_EquipmentSlot.Sword, "暗影之刃", 1200),

            // ===== 枪 (Spear) =====
            (E_EquipmentSlot.Spear, "铁枪",     100),
            (E_EquipmentSlot.Spear, "钢枪",     250),
            (E_EquipmentSlot.Spear, "秘银枪",   500),
            (E_EquipmentSlot.Spear, "龙牙枪",   800),
            (E_EquipmentSlot.Spear, "雷击之矛", 1200),

            // ===== 弓 (Bow) =====
            (E_EquipmentSlot.Bow, "短弓",     100),
            (E_EquipmentSlot.Bow, "长弓",     250),
            (E_EquipmentSlot.Bow, "秘银弓",   500),
            (E_EquipmentSlot.Bow, "凤翼弓",   800),
            (E_EquipmentSlot.Bow, "暴风之弓", 1200),

            // ===== 盾 (Shield) =====
            (E_EquipmentSlot.Shield, "木盾",     100),
            (E_EquipmentSlot.Shield, "铁盾",     250),
            (E_EquipmentSlot.Shield, "秘银盾",   500),
            (E_EquipmentSlot.Shield, "龙鳞盾",   800),
            (E_EquipmentSlot.Shield, "圣光壁垒", 1200),

            // ===== 头部防具 (Head) =====
            (E_EquipmentSlot.Head, "布帽",     80),
            (E_EquipmentSlot.Head, "皮盔",     200),
            (E_EquipmentSlot.Head, "铁盔",     400),
            (E_EquipmentSlot.Head, "秘银盔",   650),
            (E_EquipmentSlot.Head, "智慧之冠", 1000),

            // ===== 身体防具 (Body) =====
            (E_EquipmentSlot.Body, "布衣",     80),
            (E_EquipmentSlot.Body, "皮甲",     200),
            (E_EquipmentSlot.Body, "铁甲",     400),
            (E_EquipmentSlot.Body, "秘银甲",   650),
            (E_EquipmentSlot.Body, "龙鳞铠",   1000),

            // ===== 饰品 (Accessory) =====
            (E_EquipmentSlot.Accessory1, "铜戒指",   150),
            (E_EquipmentSlot.Accessory1, "银项链",   300),
            (E_EquipmentSlot.Accessory1, "金手镯",   500),
            (E_EquipmentSlot.Accessory1, "宝石耳环", 750),
            (E_EquipmentSlot.Accessory1, "暗影之戒", 1100),
        };

        int created = 0;
        int skipped = 0;

        foreach (var (slot, name, price) in data)
        {
            string fileName = name.Replace(" ", "").Replace("-", "");
            string assetPath = $"{dir}/Equip_{slot}_{fileName}.asset";

            if (System.IO.File.Exists(assetPath))
            {
                skipped++;
                continue;
            }

            var so = ScriptableObject.CreateInstance<EquipmentSO>();
            so.slot = slot;
            so.equipName = name;
            so.basePrice = price;
            // icon 和 iconResourcePath 留给用户配置

            AssetDatabase.CreateAsset(so, assetPath);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[装备调试] 生成完成: 创建{created}件, 跳过{skipped}件(已存在) → {dir}");
    }

    [MenuItem(MenuRoot + "打开存档目录", false, 20)]
    static void OpenSaveDirectory()
    {
        string dir = System.IO.Path.Combine(Application.persistentDataPath, "GameSaves");
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);
        EditorUtility.RevealInFinder(dir);
    }
}
