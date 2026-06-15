using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 生成8个随机事件SO（仅含展示信息，选项逻辑在 UnknownEventManager.BuildEventRegistry() 中定义）
/// </summary>
public static class UnknownEventSOGenerator
{
    const string outputDir = "Assets/Resources/SOData/UnknownEventSOData";

    [MenuItem("Tools/随机事件系统/生成示例UnknownSOData(8件)")]
    public static void GenerateSampleEvents()
    {
        EnsureDirectory();

        Create("潘多拉魔盒", "一个神秘的盒子出现在你面前，据说打开它会释放出未知的力量……", E_UnknownEventType.潘多拉魔盒,
            new List<string> { "将所有技能替换成随机技能", "获得一次技能选择奖励" });

        Create("锻造大师", "一位白发苍苍的老铁匠愿意为你打造一件武器，请选择你想要的类型。", E_UnknownEventType.锻造大师,
            new List<string> { "获得1件剑类型的随机装备", "获得1件枪类型的随机装备", "获得1件弓类型的随机装备" });

        Create("奥秘", "空气中弥漫着魔法的气息，你感受到某种古老知识的召唤……", E_UnknownEventType.奥秘,
            new List<string> { "获得一次技能选择奖励", "失去4点活力值，获得两次技能选择奖励" });

        Create("财富 权力 名望", "一位旅行的贤者向你提出了三个愿望，但你只能选择其中之一。", E_UnknownEventType.财富权力名望,
            new List<string> { "获得1500金币", "回复25%最大活力值&行动值", "获得5000经验值" });

        Create("贪婪", "地精商人露出了狡黠的笑容：\"想要更多金币？那就付出一点代价吧。\"", E_UnknownEventType.贪婪,
            new List<string> { "获得1000金币", "失去2点活力值，获得3000金币" });

        Create("精通", "一位武者向你展示了一种新的战斗技法，但这需要你付出一些代价来掌握。", E_UnknownEventType.精通,
            new List<string> { "失去6点活力值，获得1个自动化技能槽", "失去6点活力值，获得1个ATB技能槽", "回复2点活力值" });

        Create("祝福", "一道温暖的光芒笼罩了你，女神的声音在你耳边轻声低语……", E_UnknownEventType.祝福,
            new List<string> { "获得5点行动值", "获得1000经验值", "获得5点活力值" });

        Create("抉择", "命运的十字路口，你必须做出艰难的选择，每一次选择都将改变你的冒险之路。", E_UnknownEventType.抉择,
            new List<string> { "失去8点活力值，获得2个ATB技能槽", "失去8点活力值，获得三次技能选择奖励", "失去8点活力值，获得3件随机装备" });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[UnknownEventSOGenerator] 8件随机事件SO生成完毕");
    }

    static void EnsureDirectory()
    {
        if (!AssetDatabase.IsValidFolder(outputDir))
        {
            var parts = outputDir.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }

    static void Create(string eventName, string description, E_UnknownEventType eventType, List<string> optionDescriptions)
    {
        var so = ScriptableObject.CreateInstance<UnknownSOData>();
        so.eventType = eventType;
        so.description = description;
        so.optionDescriptions = optionDescriptions;
        AssetDatabase.CreateAsset(so, $"{outputDir}/UnknownEvent_{eventName}.asset");
        EditorUtility.SetDirty(so);
    }
}
