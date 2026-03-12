using Core;
using Core.Interfaces;
using UnityEngine;

public abstract class MonoGlobalManager : MonoBehaviour, IGlobalManager
{
    protected GameRoot GameRoot { get; private set; }

    // 初始化（由GameRoot调用）
    public virtual void MgrInit(GameRoot root)
    {
        GameRoot = root;
        MgrOnInit();
    }

    // 由GameRoot统一驱动的Update（避免Mono自身的Update开销）
    public abstract void MgrUpdate(float deltaTime);

    // 资源释放
    public virtual void MgrDispose()
    {
        MgrOnDispose();
        if (gameObject != null) Destroy(gameObject);
    }

    // 子类可重写的生命周期钩子
    protected virtual void MgrOnInit() { }
    protected virtual void MgrOnDispose() { }

    // 单例校验：防止场景中存在多个实例
    protected virtual void Awake()
    {
        if (GameRoot.Instance == null) return;

        var existing = GameRoot.Instance.GetGlobalManager(GetType());
        if (existing != null && (object)existing != this)
        {
            Destroy(gameObject);
            Debug.LogWarning($"Duplicate global mono manager: {GetType().Name}, destroying this instance.");
        }
    }
}

