using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有 List<T> 的通用扩展方法工具类
/// 静态类 + 静态方法 = 扩展方法的必要条件
/// </summary>
public static class ListExtensionMethods
{
    /// <summary>
    /// 【泛型扩展方法】从任意 List<T> 中获取一个随机元素
    /// </summary>
    /// <param name="list">被扩展的 List</param>
    /// <typeparam name="T">List 的元素类型（自动推断，无需手动指定）</typeparam>
    /// <returns>随机元素；列表为空返回类型默认值</returns>
    public static T GetRandomElement<T>(this List<T> list)
    {
        // 安全校验：防止空列表/空引用导致游戏崩溃
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("获取随机元素失败：List 为 null 或空！");
            return default; // 自动适配所有类型：引用类型返回null，值类型返回0/false等
        }

        // Unity 专用随机数（int 重载：左闭右开，完美匹配列表索引）
        int randomIndex = UnityEngine.Random.Range(0, list.Count);
        return list[randomIndex];
    }
}