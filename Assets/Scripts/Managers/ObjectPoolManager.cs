using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EPoolType
{
    MapRoom_地图房间,
    RoomCloude_房间遮云,
    SkillIcon_技能图标,
    SkillSlot_技能槽位,
}

public class ObjectPoolManager : MonoGlobalManager
{
    public int batchSize = 5;
    private Dictionary<EPoolType, PoolData> poolDataDic = new Dictionary<EPoolType, PoolData>();

    Transform HexRoomsTrans;
    Transform RoomCloudesTrans;
    Transform SkillIconsTrans;
    Transform SkillSlotsTrans;

    void CreatePoolParent()
    {
        HexRoomsTrans = new GameObject("HexRooms").transform;
        HexRoomsTrans.SetParent(transform);

        RoomCloudesTrans = new GameObject("RoomCloudes").transform;
        RoomCloudesTrans.SetParent(transform);

        SkillIconsTrans = new GameObject("SkillIcons").transform;
        SkillIconsTrans.SetParent(transform);

        SkillSlotsTrans = new GameObject("SkillSlots").transform;
        SkillSlotsTrans.SetParent(transform);
    }

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        CreatePoolParent();

        poolDataDic.Add(EPoolType.MapRoom_地图房间, new PoolData(HexRoomsTrans, ResourcesLoader.FindHexRoomObj(), 400));
        poolDataDic.Add(EPoolType.RoomCloude_房间遮云, new PoolData(RoomCloudesTrans, ResourcesLoader.FindRoomCloudeObj(), 400));
        poolDataDic.Add(EPoolType.SkillIcon_技能图标, new PoolData(SkillIconsTrans, ResourcesLoader.FindSkillIconObj(), 20));
        poolDataDic.Add(EPoolType.SkillSlot_技能槽位, new PoolData(SkillSlotsTrans, ResourcesLoader.FindSkillSlotObj(), 20));

        // 注册事件
        EventCenter.AddEventListener<EPoolType>(E_EventType.LoadObjPool, LoadOnePool);
    }

    protected override void Awake()
    {
        base.Awake();
    }
    public override void MgrDispose()
    {
        base.MgrDispose();
        EventCenter.RemoveEventListener<EPoolType>(E_EventType.LoadObjPool, LoadOnePool);
    }



    void LoadOnePool(EPoolType poolType)
    {
        // 安全获取协程管理器
        var coroutineMgr = GameRoot.GetManager<CoroutineManager>();
        if (coroutineMgr != null)
        {
            coroutineMgr.StartCoroutine(StartFillPool(poolType));
        }
    }

    public IEnumerator StartFillPool(EPoolType poolType)
    {
        int createdCount = 0;
        int poolSize = poolDataDic[poolType].poolSize;

        while (createdCount < poolSize)
        {
            int createNumThisFrame = Mathf.Min(batchSize, poolSize - createdCount);
            for (int i = 0; i < createNumThisFrame; i++)
            {
                CreateNewInstance(poolType);
                createdCount++;
            }
            yield return null;
        }
    }

    GameObject CreateNewInstance(EPoolType poolType)
    {
        if (!poolDataDic.ContainsKey(poolType)) return null;

        PoolData poolData = poolDataDic[poolType];
        GameObject instance;

        if (poolType == EPoolType.MapRoom_地图房间 || poolType == EPoolType.RoomCloude_房间遮云)
            instance = Instantiate(poolData.prefab, transform.position + new Vector3(-10, 0, 0), Quaternion.Euler(-90, 0, 0), poolData.parent);
        else
            instance = Instantiate(poolData.prefab, transform.position + new Vector3(-10, 0, 0), Quaternion.identity, poolData.parent);

        instance.SetActive(false);
        poolData.pool.Add(instance);
        return instance;
    }

    public GameObject GetInstance(EPoolType poolType)
    {
        var pool = poolDataDic[poolType].pool;
        foreach (var obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }
        var instance = CreateNewInstance(poolType);
        instance.SetActive(true);
        return instance;
    }

    public void ReturnPool(EPoolType poolType, GameObject obj)
    {
        Debug.Log("得吃了！"+poolType);
        obj.transform.SetParent(poolDataDic[poolType].parent);
        obj.SetActive(false);
    }

    public override void MgrUpdate(float deltaTime) { }
}

public class PoolData
{
    public Transform parent;
    public GameObject prefab;
    public int poolSize;
    public List<GameObject> pool = new List<GameObject>();
    public PoolData(Transform _parent, GameObject _prefab, int _poolSize)
    {
        parent = _parent;
        prefab = _prefab;
        poolSize = _poolSize;
    }
}