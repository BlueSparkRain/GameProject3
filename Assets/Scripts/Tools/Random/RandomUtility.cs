using System.Collections.Generic;
using UnityEngine;

public static class RandomUtility
{
    public static List<int> GetUniqueRandomList(int count, int min, int max)
    {
        return GetUniqueRandomList(count, min, max, null);
    }

    public static List<int> GetUniqueRandomList(int count, int min, int max, HashSet<int> excludeSet)
    {
        if (count <= 0)
        {
            Debug.LogError($"[RandomUtility] count={count} <= 0");
            return new List<int>();
        }
        if (max < min)
        {
            Debug.LogError($"[RandomUtility] max={max} < min={min}");
            return new List<int>();
        }

        int totalAvailable = max - min + 1 - (excludeSet?.Count ?? 0);
        if (totalAvailable < count)
        {
            Debug.LogWarning($"[RandomUtility] 排除后可用数量({totalAvailable})不足，回退到全范围");
            excludeSet = null;
        }

        HashSet<int> resultSet = new HashSet<int>();
        int safety = 0;
        while (resultSet.Count < count)
        {
            int randomNum = Random.Range(min, max + 1);
            if (excludeSet != null && excludeSet.Contains(randomNum))
                continue;
            resultSet.Add(randomNum);

            if (++safety > count * 100)
            {
                Debug.LogError($"[RandomUtility] 死循环！count={count}, min={min}, max={max}, excludeCount={excludeSet?.Count ?? 0}, resultCount={resultSet.Count}");
                break;
            }
        }

        var list = new List<int>(resultSet);
        return list;
    }
}
