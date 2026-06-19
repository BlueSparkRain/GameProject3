using UnityEngine;

/// <summary>
/// 玩家进入 BoxTrigger 时切换目标物体的 Y 轴高度。
/// 第一次进入：下降；再次进入：上升回原位。循环往复。
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class YAxisToggleTrigger : MonoBehaviour
{
    [Header("目标物体")]
    public GameObject targetObject;

    [Header("高度偏移量")]
    public float heightOffset = 5f;

    [Header("检测标签")]
    public string playerTag = "Player";

    bool _isLowered;

    void Awake()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (targetObject == null) return;
        if (!other.CompareTag(playerTag)) return;

        _isLowered = !_isLowered;
        Vector3 pos = targetObject.transform.position;
        pos.y += _isLowered ? -heightOffset : heightOffset;
        targetObject.transform.position = pos;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        var col = GetComponent<BoxCollider>();
        if (col != null) col.isTrigger = true;
    }
#endif
}
