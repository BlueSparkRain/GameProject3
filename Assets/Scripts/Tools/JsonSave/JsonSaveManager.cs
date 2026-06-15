using System;
using System.IO;
using UnityEngine;
/// <summary>
/// 数据验证接口（所有存档类必须实现，自定义合法规则）
/// </summary>
public interface IValidatable
{
    bool IsValid();
}

public interface ICanSave_And_Load
{
    void InitBySaveData();
    void InitBySelf();
}

/// <summary>
/// JSON 存档管理器
/// 扩展：支持【单个角色+唯一ID】独立存档
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
    public static bool HasValidData<T>() where T : class, IValidatable, new()
    {
        try
        {
            string path = GetSavePath<T>();
            string oldPath = Path.Combine(SaveRoot, typeof(T).Name + FileExtension);
            if (!File.Exists(path) && !File.Exists(oldPath)) return false;
            string actualPath = File.Exists(path) ? path : oldPath;
            string json = File.ReadAllText(actualPath);
            T data = JsonUtility.FromJson<T>(json);
            return data != null && data.IsValid();
        }
        catch { return false; }
    }

    // 按ID判断是否有有效存档
    public static bool HasValidData<T>(string uniqueId) where T : class, IValidatable, new()
    {
        try
        {
            string path = GetSavePath<T>(uniqueId);
            string oldPath = Path.Combine(SaveRoot, $"{typeof(T).Name}_{uniqueId}{FileExtension}");
            if (!File.Exists(path) && !File.Exists(oldPath))
                return false;
            string actualPath = File.Exists(path) ? path : oldPath;
            string json = File.ReadAllText(actualPath);
            T data = JsonUtility.FromJson<T>(json);
            return data != null && data.IsValid();
        }
        catch { return false; }
    }
    #endregion

    #region 初始化数据
    public static void InitData<T>(ICanSave_And_Load file,Func<bool> additive=null) where T : class, IValidatable, new()
    {
        if (additive != null) {
            if (HasValidData<T>() && additive()) { file.InitBySaveData();return;}
            else { file.InitBySelf(); return; }
        }
        if (HasValidData<T>()) { file.InitBySaveData(); }
        else { file.InitBySelf(); }
    }

    // ✅ 新增：按ID初始化角色数据
    public static void InitData<T>(ICanSave_And_Load file, string uniqueId) where T : class, IValidatable, new()
    {
        if (HasValidData<T>(uniqueId)) { file.InitBySaveData(); }
        else { file.InitBySelf(); }
    }
    #endregion

    #region 加载数据
    static string OldGetSavePath<T>() => Path.Combine(SaveRoot, typeof(T).Name + FileExtension);

    public static T Load<T>() where T : class, IValidatable, new()
    {
        try
        {
            string path = GetSavePath<T>();
            string oldPath = OldGetSavePath<T>();
            // 迁移旧版平铺存档 → 新子文件夹
            if (!File.Exists(path) && File.Exists(oldPath))
            {
                EnsureDir(Path.GetDirectoryName(path));
                File.Move(oldPath, path);
            }

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                T data = JsonUtility.FromJson<T>(json);
                if (data != null && data.IsValid()) return data;
                DebugManager.LogWarning(EDebugCategory.General, $"存档损坏，自动重置: {typeof(T).Name}");
            }
            T defaultData = new T();
            return defaultData;
        }
        catch (Exception e)
        {
            Debug.LogError($"读取失败 {typeof(T).Name}: {e.Message}");
            T fallback = new T();
            return fallback;
        }
    }

    // 按【唯一ID】加载单个角色的数据
    public static T Load<T>(string uniqueId) where T : class, IValidatable, new()
    {
        try
        {
            string path = GetSavePath<T>(uniqueId);
            string oldPath = Path.Combine(SaveRoot, $"{typeof(T).Name}_{uniqueId}{FileExtension}");
            // 迁移旧版
            if (!File.Exists(path) && File.Exists(oldPath))
            {
                EnsureDir(Path.GetDirectoryName(path));
                File.Move(oldPath, path);
            }

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                T data = JsonUtility.FromJson<T>(json);
                if (data != null && data.IsValid())
                    return data;
                DebugManager.LogWarning(EDebugCategory.General, $"角色存档损坏 ID:{uniqueId}，自动重置");
            }
            T defaultData = new T();
            Save(defaultData, uniqueId);
            return defaultData;
        }
        catch (Exception e)
        {
            Debug.LogError($"读取角色失败 ID:{uniqueId}: {e.Message}");
            T fallback = new T();
            Save(fallback, uniqueId);
            return fallback;
        }
    }
    #endregion

    #region 保存数据
    public static void Save<T>(T data) where T : class, IValidatable{
        try{
            if (data == null || !data.IsValid()){

                Debug.LogError($"拒绝保存无效数据: {typeof(T).Name}");
                return;
            }
            string path = GetSavePath<T>();
            EnsureDir(Path.GetDirectoryName(path));
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }
        catch (Exception e){
            Debug.LogError($"保存失败 {typeof(T).Name}: {e.Message}");
        }
    }

    // 按【唯一ID】保存单个角色的数据
    public static void Save<T>(T data, string uniqueId) where T : class, IValidatable{
        try{
            if (data == null || !data.IsValid()){
                Debug.LogError($"拒绝保存无效角色数据 ID:{uniqueId}");
                return;
            }
            string path = GetSavePath<T>(uniqueId);
            EnsureDir(Path.GetDirectoryName(path));
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }
        catch (Exception e){
            Debug.LogError($"保存角色失败 ID:{uniqueId}: {e.Message}");
        }
    }
    #endregion

    #region 新游戏（清空所有存档）
    public static void StartNewGame()
    {
        try
        {
            DebugManager.Log(EDebugCategory.General, "新游戏：清空所有存档");
            if (Directory.Exists(SaveRoot))
            {
                foreach (var dir in Directory.GetDirectories(SaveRoot))
                    Directory.Delete(dir, true);
                foreach (var file in Directory.GetFiles(SaveRoot))
                    File.Delete(file);
            }
            DebugManager.Log(EDebugCategory.General, "新游戏数据初始化完成");
        }
        catch (Exception e)
        {
            Debug.LogError($"新游戏初始化失败: {e.Message}");
        }
    }
    #endregion

    #region 工具方法
    // 按类型存档（存入类型子文件夹: GameSaves/TypeName/TypeName.xjson）
    public static string GetSavePath<T>() => Path.Combine(SaveRoot, typeof(T).Name, typeof(T).Name + FileExtension);

    // 按【类型+唯一ID】生成独立存档路径
    public static string GetSavePath<T>(string uniqueId) => Path.Combine(SaveRoot, typeof(T).Name, $"{typeof(T).Name}_{uniqueId}{FileExtension}");

    static void EnsureDir(string dir)
    {
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    //public static string GetSaveFilePath<T>() where T : class
    //{
    //    string fullPath = GetSavePath<T>();
    //    if (!File.Exists(fullPath)) Debug.LogWarning($"未找到存档：{fullPath}");
    //    return fullPath;
    //}
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