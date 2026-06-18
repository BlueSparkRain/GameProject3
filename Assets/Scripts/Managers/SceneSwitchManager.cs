using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// 场景切换管理器（全局Mono，自动监听场景卸载并回收局内管理器）
    /// </summary>
    public class SceneSwitchManager : MonoGlobalManager
    {
        public enum LoadMode
        {
            Single,    // 单场景加载（卸载当前所有场景）
            Additive   // 叠加加载（保留当前场景，新增场景）
        }

        void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            // 移除监听，避免内存泄漏
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// 场景卸载回调：只回收被卸载场景中的局内管理器，保留其他场景的管理器
        /// </summary>
        void OnSceneUnloaded(Scene unloadedScene)
        {
            DebugManager.Log(EDebugCategory.General, $"[SceneSwitchManager]---场景 {unloadedScene.name} 已卸载，回收该场景局内管理器");
            GameRoot.Instance.DisposeSceneManagers(unloadedScene);  // 仅回收该场景的管理器
        }

        /// <summary>
        /// 场景加载完成回调（可选：扩展场景初始化逻辑）
        /// </summary>
        void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
        {
            DebugManager.Log(EDebugCategory.General, $"[SceneSwitchManager]---场景 {loadedScene.name} 加载完成（模式：{mode}）");
            // 可扩展：场景加载完成后自动初始化局内管理器等逻辑
        }

        #region 对外场景切换API
        /// <summary>
        /// 同步切换场景（核心API）
        /// 
        /// </summary>
        /// <param name="sceneName">场景名称（需在Build Settings中注册）</param>
        /// <param name="mode">加载模式</param>
        public void SwitchScene(string sceneName, LoadMode mode = LoadMode.Single)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneSwitchManager]---场景切换失败：场景名称为空！");
                return;
            }

            // 同步加载无法等待淡出，直接停止BGM
            GameRoot.GetManager<AudioManager>()?.StopAllBGM(fadeOut: false);
            // 停止当前场景所有协程（核心兜底）
            GameRoot.GetManager<CoroutineManager>().CleanupCoroutinesByScene(SceneManager.GetActiveScene());


            LoadSceneMode loadMode = mode == LoadMode.Single
                ? LoadSceneMode.Single
                : LoadSceneMode.Additive;

            try
            {
                DebugManager.Log(EDebugCategory.General, $"[SceneSwitchManager]---开始加载场景：{sceneName}（模式：{mode}）");
                SceneManager.LoadScene(sceneName, loadMode);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SceneSwitchManager]---场景 {sceneName} 加载失败：{e.Message}");
            }

        }

        /// <summary>
        /// 异步切换场景（推荐：避免卡顿）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="mode">加载模式</param>
        public void SwitchSceneAsync(string sceneName, LoadMode mode = LoadMode.Single)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneSwitchManager]---异步场景切换失败：场景名称为空！");
                return;
            }

            // 异步加载期间淡出BGM（协程在 DontDestroyOnLoad 的 AudioManager 上运行，不受场景卸载影响）
            GameRoot.GetManager<AudioManager>()?.StopAllBGM(fadeOut: true);

            LoadSceneMode loadMode = mode == LoadMode.Single
                ? LoadSceneMode.Single
                : LoadSceneMode.Additive;

            StartCoroutine(LoadSceneAsyncCoroutine(sceneName, loadMode));
        }

        // 异步加载协程
        private IEnumerator LoadSceneAsyncCoroutine(string sceneName, LoadSceneMode mode)
        {
            // 第一步：停止当前场景所有协程（核心兜底）
            yield return GameRoot.GetManager<CoroutineManager>().CleanupCoroutinesByScene(SceneManager.GetActiveScene());
            DebugManager.Log(EDebugCategory.General, $"[SceneSwitchManager]---开始异步加载场景：{sceneName}");
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, mode);
            asyncOp.allowSceneActivation = true; // 立即激活场景（可改为加载完成后激活）

            // 等待加载完成
            while (!asyncOp.isDone)
            {
                float progress = Mathf.Clamp01(asyncOp.progress / 0.9f); // Unity加载进度到0.9即完成
                DebugManager.Log(EDebugCategory.General, $"[SceneSwitchManager]---场景 {sceneName} 加载进度：{progress:P0}");
                yield return null;
            }

            DebugManager.Log(EDebugCategory.General, $"[SceneSwitchManager]---场景 {sceneName} 异步加载完成！");
        }
        /// <summary>
        /// 异步卸载场景（Additive 模式下返回时用）
        /// </summary>
        public void UnloadSceneAsync(string sceneName)
        {
            GameRoot.GetManager<AudioManager>()?.StopAllBGM(fadeOut: true);
            StartCoroutine(UnloadSceneCoroutine(sceneName));
        }

        IEnumerator UnloadSceneCoroutine(string sceneName)
        {
            var op = SceneManager.UnloadSceneAsync(sceneName);
            if (op == null)
            {
                Debug.LogWarning($"[SceneSwitchManager] 场景 {sceneName} 未加载，跳过卸载");
                yield break;
            }
            while (!op.isDone)
                yield return null;
            DebugManager.Log(EDebugCategory.General, $"[SceneSwitchManager] 场景 {sceneName} 已异步卸载");
        }

        #endregion

        // 重写回收逻辑（全局管理器，仅在应用退出时回收）
        public override void MgrDispose()
        {
            OnDisable(); // 移除事件监听
            base.MgrDispose();
        }

        public override void MgrUpdate(float deltaTime) { }
    }
}