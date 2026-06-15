using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 角色属性展示组件 — 显示白值(基础+等级)和绿值(装备加成)。
/// 挂载到预制体上，在Inspector中选择要显示的属性类型。
/// </summary>
public class PropertyItem : MonoBehaviour
{
    [Header("属性类型")]
    [SerializeField] E_CharacterPropertyType _propertyType;
    public E_CharacterPropertyType PropertyType => _propertyType;

    [Header("UI组件")]
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _baseValueText;
    [SerializeField] TMP_Text _greenBonusText;

    void Awake()
    {
        if (_nameText != null)
            _nameText.text = PropertyNames.TryGetValue(_propertyType, out var name) ? name : _propertyType.ToString();
    }

    static readonly Dictionary<E_CharacterPropertyType, string> PropertyNames = new()
    {
        { E_CharacterPropertyType.Phy_Flat_Penetration, "物理穿透" },
        { E_CharacterPropertyType.Mag_Flat_Penetration, "法术穿透" },
        { E_CharacterPropertyType.Phy_Resistance, "物理抗性" },
        { E_CharacterPropertyType.Mag_Resistance, "魔法抗性" },
        { E_CharacterPropertyType.Phy_Attack, "物理攻击" },
        { E_CharacterPropertyType.Mag_Attack, "法术强度" },
        { E_CharacterPropertyType.Maximum_Mana, "最大法力值" },
        { E_CharacterPropertyType.Mana_Regeneration, "法力回复" },
        { E_CharacterPropertyType.Maximum_Health, "最大生命值" },
        { E_CharacterPropertyType.Health_Regeneration, "生命回复" },
        { E_CharacterPropertyType.Life_Steal, "生命偷取" },
        { E_CharacterPropertyType.Tenacity, "韧性" },
        { E_CharacterPropertyType.Endurance, "耐力" },
        { E_CharacterPropertyType.Dodge_Rate, "闪避率" },
        { E_CharacterPropertyType.Heal_Amplification, "治疗强化" },
        { E_CharacterPropertyType.Shield_Amplification, "护盾强化" },
        { E_CharacterPropertyType.Maximum_ATB, "ATB上限" },
        { E_CharacterPropertyType.CurrentLevel, "当前等级" },
        { E_CharacterPropertyType.CritRate, "暴击率" },
        { E_CharacterPropertyType.CritDamage, "暴击伤害" },
    };

    static readonly E_CharacterPropertyType[] PercentageTypes =
    {
        E_CharacterPropertyType.Dodge_Rate,
        E_CharacterPropertyType.Heal_Amplification,
        E_CharacterPropertyType.Shield_Amplification,
        E_CharacterPropertyType.Life_Steal,
        E_CharacterPropertyType.Tenacity,
        E_CharacterPropertyType.CritRate,
        E_CharacterPropertyType.CritDamage,
    };

    public void Refresh(CharacterData data)
    {
        if (data == null)
        {
            Clear();
            return;
        }

        float white = data.GetProperty(_propertyType);
        float green = data.GetGreenBonus(_propertyType);

        if (_baseValueText != null)
            _baseValueText.text = FormatValue(white);

        if (_greenBonusText != null)
        {
            if (green > 0.001f || green < -0.001f)
                _greenBonusText.text = $"+{FormatValue(green)}";
            else
                _greenBonusText.text = "";
        }
    }

    void Clear()
    {
        if (_baseValueText != null) _baseValueText.text = "-";
        if (_greenBonusText != null) _greenBonusText.text = "";
    }

    string FormatValue(float value)
    {
        foreach (var t in PercentageTypes)
            if (t == _propertyType)
                return $"{value * 100f:F1}%";
        return $"{value:F0}";
    }
}
