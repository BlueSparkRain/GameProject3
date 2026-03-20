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
        /// 场景卸载回调：旧场景卸载后，立即回收局内管理器
        /// </summary>
        /// <param name="unloadedScene">被卸载的场景</param>
        void OnSceneUnloaded(Scene unloadedScene)
        {
            Debug.Log($"[SceneSwitchManager]---场景 {unloadedScene.name} 已卸载，开始回收局内管理器！");
            // 调用GameRoot的清理方法，回收所有局内管理器
            GameRoot.Instance.DisposeSceneManagers();
        }

        /// <summary>
        /// 场景加载完成回调（可选：扩展场景初始化逻辑）
        /// </summary>
        void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
        {
            Debug.Log($"[SceneSwitchManager]---场景 {loadedScene.name} 加载完成（模式：{mode}）");
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

            // 第一步：停止当前场景所有协程（核心兜底）
            GameRoot.GetManager<CoroutineManager>().CleanupCoroutinesByScene(SceneManager.GetActiveScene());


            LoadSceneMode loadMode = mode == LoadMode.Single
                ? LoadSceneMode.Single
                : LoadSceneMode.Additive;

            try
            {
                Debug.Log($"[SceneSwitchManager]---开始加载场景：{sceneName}（模式：{mode}）");
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
            Debug.Log($"[SceneSwitchManager]---开始异步加载场景：{sceneName}");
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, mode);
            asyncOp.allowSceneActivation = true; // 立即激活场景（可改为加载完成后激活）

            // 等待加载完成
            while (!asyncOp.isDone)
            {
                float progress = Mathf.Clamp01(asyncOp.progress / 0.9f); // Unity加载进度到0.9即完成
                Debug.Log($"[SceneSwitchManager]---场景 {sceneName} 加载进度：{progress:P0}");
                yield return null;
            }

            Debug.Log($"[SceneSwitchManager]---场景 {sceneName} 异步加载完成！");
        }
        #endregion

        // 重写回收逻辑（全局管理器，仅在应用退出时回收）
        public override void MgrDispose()
        {
            OnDisable(); // 移除事件监听
            base.MgrDispose();
            Debug.Log("[SceneSwitchManager]---场景切换管理器已回收！");
        }

        public override void MgrUpdate(float deltaTime) { }
    }
}