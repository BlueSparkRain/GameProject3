using Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Core
{
    /// <summary>
    /// 根管理器-管理维护所有全局或局内的管理器(单例)
    /// </summary>
    public class GameRoot : MonoBehaviour
    {
        private static GameRoot instance;
        public static GameRoot Instance => instance;

        private static List<ISceneManager> sceneManagers = new();  //局内管理器列表
        private static List<IGlobalManager> globalManagers = new();//全局管理器列表

        /// <summary>
        /// [推荐使用-对外查询懒人接口]查询目标管理器
        /// </summary>
        /// <typeparam name="T">目标管理器</typeparam>
        /// <returns>匹配的管理器实例，无则返回null</returns>
        public static T GetManager<T>() where T :class, IManager
        {
            var manager = sceneManagers.OfType<T>().FirstOrDefault();
            if (manager != null) return manager;

            return globalManagers.OfType<T>().FirstOrDefault();
        }

        #region 基础查找功能

        /// <summary>
        /// 精准获取全局管理器（支持子类）
        /// </summary>
        /// <typeparam name="T">全局管理器类型</typeparam>
        /// <returns>匹配实例，无则打印警告并返回null</returns>
        T GetGlobalManager<T>() where T : class, IGlobalManager{
            var manager = globalManagers.OfType<T>().FirstOrDefault();
            //if (manager == null)
                //Debug.LogWarning($"[GameRoot]---未找到或未创建GlobalManager：{typeof(T).Name}。");
            return manager;
        }

        /// <summary>
        /// 尝试获取一个局内单例管理器（支持子类）
        /// </summary>
        T GetSceneManager<T>() where T : class, ISceneManager{
            var manager = sceneManagers.OfType<T>().FirstOrDefault();
            //if (manager == null)
                //Debug.LogWarning($"[GameRoot]---未找到或未创建SceneManager：{typeof(T).Name}，请确认是否已注册。");
            return manager;
        }

        /// <summary>
        /// 非泛型版本：获取全局管理器
        /// </summary>
        public IGlobalManager GetGlobalManager(System.Type type){
            if (!typeof(IGlobalManager).IsAssignableFrom(type)){
                Debug.LogError($"[GameRoot]---类型 {type.Name} 未实现 IGlobalManager 接口，无法获取全局管理器。");
                return null;
            }
            return globalManagers.FirstOrDefault(m => type.IsInstanceOfType(m));
        }

        /// <summary>
        /// 非泛型版本：获取局内管理器
        /// </summary>
        public ISceneManager GetSceneManager(System.Type type){
            if (!typeof(ISceneManager).IsAssignableFrom(type)){
                Debug.LogError($"[GameRoot]---类型 {type.Name} 未实现 ISceneManager 接口，无法获取局内管理器。");
                return null;
            }
            return sceneManagers.FirstOrDefault(m => type.IsInstanceOfType(m));
        }
        #endregion

        #region Unity生命周期（仅用于初始化和Update驱动）
        void Awake(){
            //GameRoot唯一单例初始化
            if (instance != null && instance != this){
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            //添加全局单例管理器
            RegisterGlobal_CSManager(new EventCenterManager());
            GetManager<EventCenterManager>().EventCenterTest();

            //场景切换管理器
            RegisterGlobal_MonoManager<SceneSwitchManager>();
            //异步动画管理器
            RegisterGlobal_MonoManager<MagicAnimationManager>();
            //协程管理器
            RegisterGlobal_MonoManager<CoroutineManager>();
            //UI管理器
            RegisterGlobal_MonoManager<UIManager>();
            //Audio管理器
            RegisterGlobal_MonoManager<AudioManager>();
            //光标管理器
            RegisterGlobal_MonoManager<CursorManager>();
            //地图房间交互管理器
            RegisterGlobal_MonoManager<HexGridInteractManager>();
            //房间对象池管理器
            RegisterGlobal_MonoManager<HexRoomObjectPool>();
            //地图寻路管理器
            RegisterGlobal_MonoManager<HexPathFindingManager>();
        }

        void Update(){
            float deltaTime = Time.deltaTime;
            //遍历帧更新
            foreach (var manager in globalManagers) manager.MgrUpdate(deltaTime);
            foreach (var manager in sceneManagers) manager.MgrUpdate(deltaTime);
        }

        /// <summary>
        /// OnApplicationQuit(生命周期回调函数)在应用程序退出前调用。适用于所有激活的游戏对象，常用于保存数据或清理资源。
        /// </summary>
        void OnApplicationQuit(){
            //游戏进程退出，清理所有管理器
            DisposeGlobalManagers();
            DisposeSceneManagers();
            instance = null;
            Debug.Log("[GameRoot]---应用退出，所有管理器已清理！");
        }
        #endregion


        /// <summary>
        /// 注册全局非Mono管理器（不随场景销毁）
        /// </summary>
        /// <param name="new_manager">待注册的管理器实例</param>
        public void RegisterGlobal_CSManager(IGlobalManager new_manager){
            if (globalManagers.Contains(new_manager)) return;
            globalManagers.Add(new_manager);
            new_manager.MgrInit(this);
            //日志打印
            Debug.Log($"[GameRoot]---全局非Mono管理器注册成功：{new_manager.GetType().Name}");
        }

        /// <summary>
        /// 获取或创建全局Mono管理器（自动注册，不随场景销毁）
        /// </summary>
        public T RegisterGlobal_MonoManager<T>() where T : MonoGlobalManager{
            // 1. 优先从已注册列表获取
            var existing = GetGlobalManager<T>();
            if (existing != null) return existing;

            // 2. 查找场景中已存在的实例
            var sceneInstance = FindObjectOfType<T>(includeInactive: false);
            if (sceneInstance != null){
                RegisterGlobal_CSManager(sceneInstance);
                DontDestroyOnLoad(sceneInstance.gameObject);
                //日志打印
                //Debug.Log($"[GameRoot]---全局Mono管理器（场景已有）注册成功：{typeof(T).Name}");
                return sceneInstance;
            }

            // 3. 创建新实例
            var MgrObj = new GameObject($"[Global] {typeof(T).Name}");
            var newManager = MgrObj.AddComponent<T>();
            RegisterGlobal_CSManager(newManager);
            DontDestroyOnLoad(MgrObj);
            //日志打印
            //Debug.Log($"[GameRoot]---全局Mono管理器（新建）注册成功：{typeof(T).Name}");
            return newManager;
        }


        /// <summary>
        /// 注册局内非Mono管理器（随场景销毁）
        /// </summary>
        /// <param name="new_manager">待注册的管理器实例</param>
        public void RegisterScene_CSManager(ISceneManager new_manager){
            if (sceneManagers.Contains(new_manager)) return;

            sceneManagers.Add(new_manager);
            new_manager.MgrInit(this);
            //日志打印
            Debug.Log($"[GameRoot]---局内非Mono管理器注册成功：{new_manager.GetType().Name}");
        }

        /// <summary>
        /// 获取或创建局内Mono管理器（自动注册，随场景销毁）
        /// </summary>
        public T RegisterScene_MonoManager<T>() where T : MonoSceneManager{
            // 1. 优先从已注册列表获取
            var existing = GetSceneManager<T>();
            if (existing != null) return existing;

            // 2. 查找当前场景中已存在的实例
            var sceneInstance = FindObjectOfType<T>(includeInactive: false);
            if (sceneInstance != null){
                RegisterScene_CSManager(sceneInstance);
                //日志打印
                Debug.Log($"[GameRoot]---局内Mono管理器（场景已有）注册成功：{typeof(T).Name}");
                return sceneInstance;
            }

            // 3. 创建新实例
            var MgrObj = new GameObject($"[Scene] {typeof(T).Name}");
            var newManager = MgrObj.AddComponent<T>();
            RegisterScene_CSManager(newManager);
            //日志打印
            Debug.Log($"[GameRoot]---局内Mono管理器（新建）注册成功：{typeof(T).Name}");
            return newManager;
        }

        #region 管理器清理
        /// <summary>
        /// 回收所有全局单例管理器
        /// </summary>
        public void DisposeGlobalManagers(){
            foreach (var manager in globalManagers){
                manager.MgrDispose();
                //日志打印
                Debug.Log($"[GameRoot]---全局管理器已回收：{manager.GetType().Name}");
            }
            globalManagers.Clear();
        }

        /// <summary>
        /// 回收所有局内管理器（对外暴露，供场景切换时调用）
        /// </summary>
        public void DisposeSceneManagers(){
            foreach (var manager in sceneManagers){
                manager.MgrDispose();
                //日志打印
                Debug.Log($"[GameRoot]---局内管理器已回收：{manager.GetType().Name}");
            }
            sceneManagers.Clear();
            Debug.Log("[GameRoot]---当前所有局内管理器已清空！");
        }

        #endregion
    }
}
