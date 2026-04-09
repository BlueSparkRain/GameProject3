using System.Collections.Generic;

public static class DictionaryExtensions
{
    /// <summary>
    /// 从字典中随机取一个元素
    /// </summary>
    public static KeyValuePair<TKey, TValue> GetRandomElement<TKey, TValue>(this Dictionary<TKey, TValue> dict)
    {
        if (dict == null || dict.Count == 0)
            return default;

        // 随机索引
        int randomIndex = UnityEngine.Random.Range(0, dict.Count);
        // 直接按顺序取第 N 个元素（最省性能）
        using var enumerator = dict.GetEnumerator();
        for (int i = 0; enumerator.MoveNext() && i < randomIndex; i++) { }
        return enumerator.Current;
    }
}