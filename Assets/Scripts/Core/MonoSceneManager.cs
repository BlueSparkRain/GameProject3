// Core/Managers/MonoSceneManager.cs（局内Mono管理器基类）
using UnityEngine;
using Core.Interfaces;
using Core;


    public abstract class MonoSceneManager : MonoBehaviour, ISceneManager
    {
        protected GameRoot GameRoot { get; private set; }
        public virtual void MgrInit(GameRoot root){
            GameRoot = root;
            MgrOnInit();
        }
        public abstract void MgrUpdate(float deltaTime);

        public virtual void MgrDispose(){
            MgrOnDispose(); 
            // 局内管理器无需手动Destroy，场景切换会自动销毁
        }
        // 子类可重写的生命周期钩子
        protected virtual void MgrOnInit() { }
        protected virtual void MgrOnDispose() { }


        protected virtual void Awake(){
            if (GameRoot.Instance == null) return;

            var existing = GameRoot.Instance.GetSceneManager(GetType());
            if (existing != null && (object)existing != this){
                Destroy(gameObject);
                Debug.LogWarning($"Duplicate scene mono manager: {GetType().Name}, destroying this instance.");
            }
        }
    }
