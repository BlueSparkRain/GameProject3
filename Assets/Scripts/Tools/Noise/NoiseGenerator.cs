using UnityEngine;

/// <summary>
/// 六边形地图噪声生成器
/// </summary>
public static class NoiseGenerator
{
    /// <summary>
    /// 生成六边形地形数组（核心方法）
    /// </summary>
    public static bool[,] GenerateHexTerrain(HexMapNoiseData data)
    {
        int width = data.mapWidth;
        int height = data.mapHeight;
        bool[,] terrain = new bool[width, height];

        // 1. 生成柏林噪声基础图
        float[,] noiseMap = GenerateNoiseMap(width, height, data.noiseScale, data.octaves, data.persistence, data.lacunarity);

        // 2. 动态阈值过滤：严格匹配75%可行走占比
        float threshold = CalculateThreshold(noiseMap, data.walkableRate);
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                terrain[x, y] = noiseMap[x, y] > threshold;

        // 3. 细胞自动机平滑：消除细碎地块，让平坦变成孤岛
        for (int i = 0; i < data.smoothIterations; i++)
            terrain = SmoothHexTerrain(terrain);

        // 4. 六边形连通性校验：保留最大连续可行走区域（核心！保证凸起连续）
        terrain = KeepLargestConnectedArea(terrain);

        data.ConvertTwoDimToOneDim(terrain);

        return terrain;
    }

    // 生成柏林噪声
    private static float[,] GenerateNoiseMap(int width, int height, float scale, int octaves, float persistence, float lacunarity)
    {
        float[,] map = new float[width, height];
        if (scale <= 0) scale = 0.0001f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noiseHeight = 0;
                float frequency = 1;
                float amplitude = 1;

                for (int o = 0; o < octaves; o++)
                {
                    float sampleX = x / scale * frequency;
                    float sampleY = y / scale * frequency;
                    noiseHeight += Mathf.PerlinNoise(sampleX, sampleY) * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                map[x, y] = Mathf.InverseLerp(0, octaves, noiseHeight);
            }
        }
        return map;
    }

    // 计算阈值：保证目标占比
    private static float CalculateThreshold(float[,] noiseMap, float targetRate)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        float[] values = new float[width * height];

        int index = 0;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                values[index++] = noiseMap[x, y];

        System.Array.Sort(values);
        return values[Mathf.RoundToInt(values.Length * (1 - targetRate))];
    }

    // 六边形地形平滑（6邻域细胞自动机）
    private static bool[,] SmoothHexTerrain(bool[,] terrain)
    {
        int width = terrain.GetLength(0);
        int height = terrain.GetLength(1);
        bool[,] newTerrain = (bool[,])terrain.Clone();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighborCount = GetHexNeighborCount(terrain, x, y);
                // 规则：孤立地块自动反转（平坦变凸起，凸起变平坦）
                if (neighborCount < 2) newTerrain[x, y] = false;
                else if (neighborCount > 4) newTerrain[x, y] = true;
            }
        }
        return newTerrain;
    }

    // 获取六边形6邻域可行走数量
    private static int GetHexNeighborCount(bool[,] terrain, int x, int y)
    {
        int count = 0;
        int width = terrain.GetLength(0);
        int height = terrain.GetLength(1);

        // 六边形6邻域偏移（偏移坐标适配）
        int[][] directions = new int[][]
        {
            new int[] { 1, 0 }, new int[] { -1, 0 },
            new int[] { 0, 1 }, new int[] { 0, -1 },
            new int[] { 1, -1 }, new int[] { -1, 1 }
        };

        foreach (var dir in directions)
        {
            int nx = x + dir[0];
            int ny = y + dir[1];
            if (nx >= 0 && nx < width && ny >= 0 && ny < height && terrain[nx, ny])
                count++;
        }
        return count;
    }

    // 优化：找最大连通区域（无需完整排序，直接遍历找最大）
    private static bool[,] KeepLargestConnectedArea(bool[,] terrain)
    {
        int width = terrain.GetLength(0);
        int height = terrain.GetLength(1);
        bool[,] visited = new bool[width, height];
        System.Collections.Generic.List<Vector2Int> largestArea = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (terrain[x, y] && !visited[x, y])
                {
                    System.Collections.Generic.List<Vector2Int> currentArea = new();
                    FloodFillHex(terrain, visited, x, y, currentArea);
                    // 直接保留最大的区域，省去排序开销
                    if (currentArea.Count > largestArea.Count)
                        largestArea = currentArea;
                }
            }
        }

        // 生成最终地形
        bool[,] newTerrain = new bool[width, height];
        foreach (var pos in largestArea)
            newTerrain[pos.x, pos.y] = true;

        return newTerrain;
    }

    // 六边形洪水填充（连通性检测）
    // 🔥 修复：迭代式BFS洪水填充（替换原递归方法，无栈溢出）
    private static void FloodFillHex(bool[,] terrain, bool[,] visited, int startX, int startY, System.Collections.Generic.List<Vector2Int> area)
    {
        int width = terrain.GetLength(0);
        int height = terrain.GetLength(1);

        // 六边形6邻域偏移（不变）
        int[][] directions = new int[][]
        {
        new int[] { 1, 0 }, new int[] { -1, 0 },
        new int[] { 0, 1 }, new int[] { 0, -1 },
        new int[] { 1, -1 }, new int[] { -1, 1 }
        };

        // 用队列实现BFS，替代递归（核心修复）
        System.Collections.Queue queue = new System.Collections.Queue();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int pos = (Vector2Int)queue.Dequeue();
            area.Add(pos);

            // 遍历6个邻域
            foreach (var dir in directions)
            {
                int nx = pos.x + dir[0];
                int ny = pos.y + dir[1];

                // 边界+可行走+未访问 校验
                if (nx >= 0 && nx < width && ny >= 0 && ny < height
                    && terrain[nx, ny] && !visited[nx, ny])
                {
                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }
    }
    // 生成预览纹理
    public static Texture2D GeneratePreviewTexture(bool[,] terrain)
    {
        int width = terrain.GetLength(0);
        int height = terrain.GetLength(1);
        Texture2D tex = new Texture2D(width, height);
        Color[] colors = new Color[width * height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                colors[y * width + x] = terrain[x, y] ? Color.white : Color.black;

        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return tex;
    }
}