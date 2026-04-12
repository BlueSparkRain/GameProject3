using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// 数据验证接口（所有存档类必须实现，自定义合法规则）
/// </summary>
public interface IValidatable
{
    /// <summary>
    /// 自定义：数据是否合法有效
    /// </summary>
    bool IsValid();
}

public interface ISaveable {

 

    public void InitBySaveData();
    public void InitBySelf();
}

/// <summary>
/// JSON 存档管理器
/// 1. 正常启动 = 加载本地已有存档（续玩）
/// 2. 手动点击【开始新游戏】= 清空并重置存档
/// 3. 文件仅创建一次，不存在时才自动生成
/// 
/// 判断存档 = 验证数据是否有效，而非仅判断文件
/// </summary>
public static class JsonSaver
{

    private static readonly string SaveRoot = Application.persistentDataPath + "/GameSaves/";
    private const string FileExtension = ".xjson";

    static JsonSaver()
    {
        if (!Directory.Exists(SaveRoot))
            Directory.CreateDirectory(SaveRoot);
    }

    #region 核心：判断【数据是否有效】
    /// <summary>
    /// 检测：本地是否存在【有效、可正常使用】的存档数据
    /// 满足：文件存在 + 解析成功 + 数据合法
    /// </summary>
    public static bool HasValidData<T>() where T : class, IValidatable, new()
    {
        try
        {
            string path = GetSavePath<T>();
            // 1. 无文件 → 无效
            if (!File.Exists(path)) return false;

            // 2. 读取并反序列化
            string json = File.ReadAllText(path);
            T data = JsonUtility.FromJson<T>(json);

            // 3. 验证数据是否合法（自定义规则）
            return data != null && data.IsValid();
        }
        catch
        {
            // 解析失败/异常 → 数据无效
            return false;
        }
    }
    #endregion

    /// <summary>
    /// 依据存档数据来初始化
    /// </summary>
    public static void InitData<T>(ISaveable file) where T : class, IValidatable, new()
    {

        if (JsonSaver.HasValidData<T>())
        {
            Debug.Log("加载了存档数据");
            file.InitBySaveData();
        }
        else
        {
            Debug.Log("加载了初始数据");
            file.InitBySelf();
        }
    }

    #region 加载数据（自动验证，无效则返回默认）
    public static T Load<T>() where T : class, IValidatable, new()
    {
        try
        {
            string path = GetSavePath<T>();
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                T data = JsonUtility.FromJson<T>(json);

                // 数据有效 → 返回
                if (data != null && data.IsValid())
                {
                    Debug.Log($"加载【有效存档】: {typeof(T).Name}");
                    return data;
                }

                // 数据损坏/无效 → 重置
                Debug.LogWarning($"存档损坏，自动重置: {typeof(T).Name}");
            }

            // 无文件 / 数据无效 → 创建默认有效数据
            T defaultData = new T();
            Save(defaultData);
            Debug.Log($"创建【有效默认数据】: {typeof(T).Name}");
            return defaultData;
        }
        catch (Exception e)
        {
            Debug.LogError($"读取失败 {typeof(T).Name}: {e.Message}");
            T fallback = new T();
            Save(fallback);
            return fallback;
        }
    }
    #endregion

    #region 保存（自动保证数据有效）
    public static void Save<T>(T data) where T : class, IValidatable
    {
        try
        {
            if (data == null || !data.IsValid())
            {
                Debug.LogError($"拒绝保存无效数据: {typeof(T).Name}");
                return;
            }

            string json = JsonUtility.ToJson(data, true);//true:完美排布，便于人类阅读
            File.WriteAllText(GetSavePath<T>(), json);
        }
        catch (Exception e)
        {
            Debug.LogError($"保存失败 {typeof(T).Name}: {e.Message}");
        }
    }
    #endregion

    #region 开始新游戏（清空重置）
    public static void StartNewGame()
    {
        try
        {
            Debug.Log("新游戏：清空所有存档");
            if (Directory.Exists(SaveRoot))
            {
                foreach (var file in Directory.GetFiles(SaveRoot))
                    File.Delete(file);
            }

            //Load<Save_CharacterData>();
            //Load<MapSaveData>();
            Debug.Log("新游戏数据初始化完成");
        }
        catch (Exception e)
        {
            Debug.LogError($"新游戏初始化失败: {e.Message}");
        }
    }
    #endregion

    #region 工具
    private static string GetSavePath<T>()
    {
        return Path.Combine(SaveRoot, typeof(T).Name + FileExtension);
    }

    /// <summary>
    /// 获取目标数据类型的存档文件完整路径
    /// 无存档文件时自动打印提示
    /// </summary>
    public static string GetSaveFilePath<T>() where T : class
    {
        string fullPath = GetSavePath<T>();

        // 无文件 → 打印警告
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"【存档查询】未找到 {typeof(T).Name} 的存档文件\n路径：{fullPath}");
        }

        return fullPath;
    }
    #endregion
}

//// ====================== 角色数据（核心示例）======================
//[System.Serializable]
//public class PlayerData : IValidatable
//{
//    // 默认属性（保证初始就是有效数据）
//    public int level = 1;
//    public int hp = 100;
//    public int attack = 10;
//    public string playerName = "冒险者";

//    /// <summary>
//    /// 【自定义】角色数据有效规则
//    /// 你可以自由修改：等级>0、血量>0、名字不为空 才算有效
//    /// </summary>
//    public bool IsValid()
//    {
//        return level > 0 && hp > 0 && attack > 0 && !string.IsNullOrEmpty(playerName);
//    }
//}

//// ====================== 地图数据（适配你的六边形）======================
//[System.Serializable]
//public class MapSaveData : IValidatable
//{
//    public int mapRadius = 30;
//    public SerializedHexCell[] cells;

//    public bool IsValid()
//    {
//        // 地图有效：半径合法 + 地块数组不为空
//        return mapRadius > 0 && cells != null && cells.Length > 0;
//    }
//}


//// 六边形地块结构
//[System.Serializable]
//public class SerializedHexCell
//{
//    public int row;
//    public int col;
//    public E_HexTerrainType type;
//}