using System;
using System.Collections.Generic;

public static class RandomUtility
{
    /// <summary>
    /// 生成指定长度、指定范围、不重复的随机整数列表
    /// </summary>
    /// <param name="count">需要生成的数字数量</param>
    /// <param name="min">左边界（包含）</param>
    /// <param name="max">右边界（包含）</param>
    /// <returns>不重复随机整数列表</returns>
    public static List<int> GetUniqueRandomList(int count, int min, int max)
    {
        // 1. 边界参数校验（必加，防止报错）
        if (count <= 0) throw new ArgumentException("生成长度必须大于0");
        if (max < min) throw new ArgumentException("右边界不能小于左边界");

        int totalNumbers = max - min + 1;
        if (count > totalNumbers) throw new ArgumentException($"生成数量不能超过范围总数！范围最多有{totalNumbers}个不重复数字");

        // 2. 用HashSet自动去重
        HashSet<int> resultSet = new HashSet<int>();
        Random random = new Random();

        // 3. 循环生成直到数量达标
        while (resultSet.Count < count)
        {
            int randomNum = random.Next(min, max + 1); // Next左闭右开，+1包含右边界
            resultSet.Add(randomNum);
        }

        // 4. 转List返回
        return new List<int>(resultSet);
    }
}