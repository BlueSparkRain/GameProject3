using Core;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_PoolType{
    MapRoom_地图房间,
    RoomCloude_房间遮云,
    SkillIcon_技能图标,
    SkillSlot_技能槽位,
    RoomModel_房间纸片,
    HexFace_投影面片,
    HexRoomIcon_房间图标,
    FloatingText_跳字,
    ATBDot_ATB点数,
}

public class ObjectPoolManager : MonoGlobalManager
{
    public int batchSize = 5;
    private Dictionary<E_PoolType, PoolData> poolDataDic = new Dictionary<E_PoolType, PoolData>();

    Transform HexRoomsTrans;
    Transform RoomCloudesTrans;
    Transform SkillIconsTrans;
    Transform SkillSlotsTrans;
    Transform HexFacesTrans;
    Transform HexRoomIconsTrans;
    Transform FloatingTextsTrans;
    Transform ATBDotsTrans;

    void CreatePoolParent(){
        // 重复运行时，先销毁旧的父物体，避免残留
        if (HexRoomsTrans != null) Destroy(HexRoomsTrans.gameObject);
        if (RoomCloudesTrans != null) Destroy(RoomCloudesTrans.gameObject);
        if (SkillIconsTrans != null) Destroy(SkillIconsTrans.gameObject);
        if (SkillSlotsTrans != null) Destroy(SkillSlotsTrans.gameObject);
        if (HexFacesTrans != null) Destroy(HexFacesTrans.gameObject);
        if (HexRoomIconsTrans != null) Destroy(HexRoomIconsTrans.gameObject);
        if (FloatingTextsTrans != null) Destroy(FloatingTextsTrans.gameObject);
        if (ATBDotsTrans != null) Destroy(ATBDotsTrans.gameObject);

        HexRoomsTrans = new GameObject("HexRooms").transform;
        HexRoomsTrans.SetParent(transform);

        RoomCloudesTrans = new GameObject("RoomCloudes").transform;
        RoomCloudesTrans.SetParent(transform);

        SkillIconsTrans = new GameObject("SkillIcons").transform;
        SkillIconsTrans.SetParent(transform);

        SkillSlotsTrans = new GameObject("SkillSlots").transform;
        SkillSlotsTrans.SetParent(transform);

        HexFacesTrans = new GameObject("HexFaces").transform;
        HexFacesTrans.SetParent(transform);

        HexRoomIconsTrans = new GameObject("HexRoomIcons").transform;
        HexRoomIconsTrans.SetParent(transform);

        FloatingTextsTrans = new GameObject("FloatingTexts").transform;
        FloatingTextsTrans.SetParent(transform);

        ATBDotsTrans = new GameObject("ATBDots").transform;
        ATBDotsTrans.SetParent(transform);
    }
    protected override void MgrOnInit(){
        base.MgrOnInit();
        CreatePoolParent();

        // 初始化前清空字典，防止重复添加
        poolDataDic.Clear();

        poolDataDic.Add(E_PoolType.MapRoom_地图房间, new PoolData(HexRoomsTrans, ResourcesLoader.FindHexRoomObj(), 1925));
        poolDataDic.Add(E_PoolType.RoomCloude_房间遮云, new PoolData(RoomCloudesTrans, ResourcesLoader.FindRoomCloudeObj(), 1000));
        poolDataDic.Add(E_PoolType.SkillIcon_技能图标, new PoolData(SkillIconsTrans, ResourcesLoader.FindSkillIconObj(), 30));
        poolDataDic.Add(E_PoolType.SkillSlot_技能槽位, new PoolData(SkillSlotsTrans, ResourcesLoader.FindSkillSlotObj(), 30));
        poolDataDic.Add(E_PoolType.RoomModel_房间纸片, new PoolData(SkillSlotsTrans, ResourcesLoader.FindSkillSlotObj(), 30));
        poolDataDic.Add(E_PoolType.HexFace_投影面片, new PoolData(HexFacesTrans, ResourcesLoader.FindHexFaceObj(), 2700));
        poolDataDic.Add(E_PoolType.HexRoomIcon_房间图标, new PoolData(HexRoomIconsTrans, ResourcesLoader.FindHexRoomIconObj(), 100));
        poolDataDic.Add(E_PoolType.FloatingText_跳字, new PoolData(FloatingTextsTrans, ResourcesLoader.FindFloatingTextObj(), 40));
        poolDataDic.Add(E_PoolType.ATBDot_ATB点数, new PoolData(ATBDotsTrans, ResourcesLoader.FindATBDotObj(), 20));

        // 注册事件
        EventCenter.AddEventListener<E_PoolType>(E_EventType.LoadObjPool, LoadOnePool);
    }

    protected override void Awake(){
        base.Awake();
    }

    /// <summary>
    /// 【核心修复】重写销毁方法，彻底清空对象池
    /// </summary>
    public override void MgrDispose(){
        base.MgrDispose();
        EventCenter.RemoveEventListener<E_PoolType>(E_EventType.LoadObjPool, LoadOnePool);

        // 遍历所有对象池，销毁物体 + 清空列表
        foreach (var data in poolDataDic.Values){
            if (data.pool != null){
                foreach (var obj in data.pool){
                    if (obj != null) Destroy(obj);
                }
                data.pool.Clear();
            }
        }
        // 清空字典
        poolDataDic.Clear();

        // 销毁父物体
        if (HexRoomsTrans != null) Destroy(HexRoomsTrans.gameObject);
        if (RoomCloudesTrans != null) Destroy(RoomCloudesTrans.gameObject);
        if (SkillIconsTrans != null) Destroy(SkillIconsTrans.gameObject);
        if (SkillSlotsTrans != null) Destroy(SkillSlotsTrans.gameObject);
        if (HexFacesTrans != null) Destroy(HexFacesTrans.gameObject);
        if (HexRoomIconsTrans != null) Destroy(HexRoomIconsTrans.gameObject);
        if (FloatingTextsTrans != null) Destroy(FloatingTextsTrans.gameObject);
        if (ATBDotsTrans != null) Destroy(ATBDotsTrans.gameObject);
    }

    void LoadOnePool(E_PoolType poolType){
        var coroutineMgr = GameRoot.GetManager<CoroutineManager>();
        if (coroutineMgr != null){
            coroutineMgr.StartCoroutine(StartFillPool(poolType), this);
        }
    }

    public IEnumerator StartFillPool(E_PoolType poolType){
        // 空值防护
        if (!poolDataDic.ContainsKey(poolType)) yield break;

        int createdCount = 0;
        int poolSize = poolDataDic[poolType].poolSize;

        while (createdCount < poolSize){
            int createNumThisFrame = Mathf.Min(batchSize, poolSize - createdCount);
            for (int i = 0; i < createNumThisFrame; i++){
                CreateNewInstance(poolType);
                createdCount++;
            }
            yield return null;
        }
    }

    GameObject CreateNewInstance(E_PoolType poolType){
        if (!poolDataDic.ContainsKey(poolType)) return null;

        PoolData poolData = poolDataDic[poolType];
        // 预制体空值校验
        if (poolData.prefab == null) return null;

        GameObject instance;
        if (poolType == E_PoolType.MapRoom_地图房间 || poolType == E_PoolType.RoomCloude_房间遮云 || poolType == E_PoolType.HexRoomIcon_房间图标)
            instance = Instantiate(poolData.prefab, transform.position + new Vector3(-10, 0, 0), Quaternion.Euler(-90, 0, 0), poolData.parent);
        else if (poolType == E_PoolType.HexFace_投影面片)
            instance = Instantiate(poolData.prefab, transform.position + new Vector3(-10, 0, 0), Quaternion.identity, poolData.parent);
        else
            instance = Instantiate(poolData.prefab, transform.position + new Vector3(-10, 0, 0), Quaternion.identity, poolData.parent);

        instance.SetActive(false);
        poolData.pool.Add(instance);
        return instance;
    }

    Dictionary<E_PoolType, int> _nextSearchIndex = new Dictionary<E_PoolType, int>();

    /// <summary>
    /// 获取池中可用物体。使用缓存的搜索起点避免 O(n²) 线性扫描，批量取出时接近 O(1)。
    /// </summary>
    public GameObject GetInstance(E_PoolType poolType){
        if (!poolDataDic.ContainsKey(poolType)) return null;

        var pool = poolDataDic[poolType].pool;
        var poolParent = poolDataDic[poolType].parent;
        int count = pool.Count;
        if (count == 0) goto CreateNew;

        if (!_nextSearchIndex.TryGetValue(poolType, out int start))
            start = 0;
        if (start >= count) start = 0;

        // 从缓存位置开始搜索（最多一圈）
        for (int n = 0; n < count; n++)
        {
            int i = (start + n) % count;
            var obj = pool[i];
            if (obj == null)
            {
                pool.RemoveAt(i);
                count--;
                if (count == 0) break;
                n--;
                continue;
            }
            if (!obj.activeSelf && obj.transform.parent == poolParent)
            {
                obj.SetActive(true);
                _nextSearchIndex[poolType] = (i + 1) % pool.Count;
                return obj;
            }
        }

        CreateNew:
        var instance = CreateNewInstance(poolType);
        if (instance != null) instance.SetActive(true);
        return instance;
    }

    public void ReturnPool(E_PoolType poolType, GameObject obj){
        if (obj == null || !poolDataDic.ContainsKey(poolType)) return;
        obj.transform.DOKill();
        obj.transform.SetParent(poolDataDic[poolType].parent);
        obj.SetActive(false);
    }

    /// <summary>场景切换前强制回收指定类型所有活跃实例（防止跨场景SetParent报错）</summary>
    public void ReclaimAll(E_PoolType poolType)
    {
        if (!poolDataDic.ContainsKey(poolType)) return;

        var pool = poolDataDic[poolType].pool;
        var poolParent = poolDataDic[poolType].parent;

        for (int i = pool.Count - 1; i >= 0; i--)
        {
            var obj = pool[i];
            if (obj == null)
            {
                pool.RemoveAt(i);
                continue;
            }
            if (obj.activeSelf || obj.transform.parent != poolParent)
            {
                obj.transform.DOKill();
                obj.transform.SetParent(poolParent);
                obj.SetActive(false);
            }
        }
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