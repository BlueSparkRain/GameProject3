using Core;
using Core.Interfaces;
using UnityEngine;

public abstract class MonoGlobalManager : MonoBehaviour, IGlobalManager
{
    protected GameRoot GameRoot { get; private set; }

    public virtual void MgrInit(GameRoot root)
    {
        GameRoot = root;
        MgrOnInit();
    }

    public abstract void MgrUpdate(float deltaTime);
    public virtual void MgrDispose()
    {
        MgrOnDispose();
    }

    protected virtual void MgrOnInit() { }
    protected virtual void MgrOnDispose() { }

    protected virtual void Awake()
    {
        // ✅ 第一优先级：设为全局不销毁
        DontDestroyOnLoad(gameObject);

        if (GameRoot.Instance == null) return;

        var existing = GameRoot.Instance.GetGlobalManager(GetType());
        if (existing != null && existing != this)
        {
            Destroy(gameObject);
            return;
        }
    }
}