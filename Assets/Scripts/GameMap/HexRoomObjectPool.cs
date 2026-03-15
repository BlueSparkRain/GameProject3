using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexRoomObjectPool : MonoGlobalManager
{
    // 预制件
    public GameObject roomObject;
    // 池容量
    public int roomCount = 100;
    // 每帧创建的实例数量（核心优化点：分散开销）
    public int batchSize = 5;

    private List<GameObject> pool = new List<GameObject>();
    // 标记池子是否还在异步填充中
    private bool isPoolFilling = false;

    void Start()
    {
        roomObject = Resources.Load<GameObject>("Prefabs/MapRoom");
        // 启动异步填充池子的协程
        StartCoroutine(StartFillPool());
        Debug.Log("开启协程创建房间");
    }

    /// <summary>
    /// 异步分批填充池子（核心优化方法）
    /// </summary>
    IEnumerator StartFillPool()
    {
        if (roomObject == null)
        {
            Debug.LogError("HexRoomObjectPool: roomObject预制件未赋值！");
            yield break;
        }

        isPoolFilling = true;
        int createdCount = 0;

        // 分批创建，每帧创建batchSize个，直到达到池容量
        while (createdCount < roomCount)
        {
            // 计算当前帧需要创建的数量（避免最后一批超出总数）
            int createNumThisFrame = Mathf.Min(batchSize, roomCount - createdCount);

            for (int i = 0; i < createNumThisFrame; i++)
            {
                CreateNewInstance();
                createdCount++;
            }

            // 每批创建完成后，让出当前帧，等待下一帧再继续（关键：避免单帧卡顿）
            yield return null;
        }

        isPoolFilling = false;
        Debug.Log($"HexRoomObjectPool: 池子异步初始化完成，总数量：{pool.Count}");
    }

    /// <summary>
    /// 创造一个实例并加入池中
    /// </summary>
    GameObject CreateNewInstance()
    {
        var instance = GameObject.Instantiate(roomObject, transform.position, Quaternion.Euler(-90, 0, 0),transform);
        instance.SetActive(false);
        pool.Add(instance);
        return instance;
    }

    /// <summary>
    /// 从池中取出一个实例（兼容异步初始化过程）
    /// </summary>
    public GameObject GetInstance()
    {
        // 优先从池中取闲置实例
        foreach (var obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // 即使池子还没初始化完/池子里没有闲置，也临时创建（保证功能不中断）
        Debug.LogWarning("HexRoomObjectPool: 池子无闲置实例，临时创建新实例");
        var instance = CreateNewInstance();
        instance.SetActive(true);
        return instance;
    }

    /// <summary>
    /// 返回池中
    /// </summary>
    public void ReturnPool(GameObject obj)
    {
        if (obj.TryGetComponent<HexRoom>(out var hexRoom))
        {
            hexRoom.ResetSelf();
            obj.SetActive(false); // 补充：归还时记得禁用对象（原代码可能漏了）
        }
        else
        {
            Debug.LogError("HexRoomObjectPool: 归还的对象没有HexRoom组件！");
        }
    }

    public override void MgrUpdate(float deltaTime) { }

    // 可选：外部获取池子状态
    public bool IsPoolReady() => !isPoolFilling;
}