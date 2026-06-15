using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 标记怪物AI组件适用的角色类型。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class MonsterAIForAttribute : Attribute
{
    public E_CharacterType CharacterType { get; }
    public MonsterAIForAttribute(E_CharacterType type) => CharacterType = type;
}

/// <summary>
/// 怪物AI组件工厂——根据E_CharacterType创建对应的IMonsterAIComponent。
/// 新增AI行为只需：实现接口 + 标记[MonsterAIFor]特性，无需修改工厂代码。
/// </summary>
public static class MonsterAIFactory
{
    static Dictionary<E_CharacterType, Func<IMonsterAIComponent>> _creators;

    static MonsterAIFactory()
    {
        _creators = new Dictionary<E_CharacterType, Func<IMonsterAIComponent>>();
        ScanAllAIComponents();
    }

    static void ScanAllAIComponents()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var aiTypes = assembly.GetTypes()
            .Where(t => typeof(IMonsterAIComponent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in aiTypes)
        {
            var attr = type.GetCustomAttribute<MonsterAIForAttribute>();
            if (attr == null) continue;

            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
            {
                Debug.LogError($"怪物AI {type.Name} 缺少无参构造函数");
                continue;
            }

            _creators[attr.CharacterType] = () => (IMonsterAIComponent)ctor.Invoke(null);
            DebugManager.Log(EDebugCategory.BattleAI,$"[MonsterAIFactory] 注册怪物AI: {attr.CharacterType} → {type.Name}");
        }
    }

    /// <summary>
    /// 尝试为指定角色类型创建AI组件。无匹配时返回null（表示该类型无特殊AI）。
    /// </summary>
    public static IMonsterAIComponent Create(E_CharacterType characterType)
    {
        if (_creators.TryGetValue(characterType, out var creator))
            return creator();
        return null;
    }

    /// <summary>
    /// 手动注册AI组件（用于运行时动态添加，无需特性标记）。
    /// </summary>
    public static void Register(E_CharacterType type, Func<IMonsterAIComponent> creator)
    {
        _creators[type] = creator;
    }
}
