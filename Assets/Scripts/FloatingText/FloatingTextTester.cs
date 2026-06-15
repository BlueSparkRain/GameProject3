using UnityEngine;

/// <summary>
/// 浮字系统测试。1/2/3/4 键分别测试 HP/SP/Shield/AG 类型的跳字。
/// 需要在场景中配置对应的 FloatingTextSpawner 引用。
/// </summary>
public class FloatingTextTester : MonoBehaviour
{
    [SerializeField] FloatingTextSpawner _hpSpawner;
    [SerializeField] FloatingTextSpawner _spSpawner;
    [SerializeField] FloatingTextSpawner _shieldSpawner;
    [SerializeField] FloatingTextSpawner _agSpawner;

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1) && _hpSpawner != null)
            _hpSpawner.Show(Random.Range(-200, 200));
        if (Input.GetKeyDown(KeyCode.Alpha2) && _spSpawner != null)
            _spSpawner.Show(Random.Range(-80, -20));
        if (Input.GetKeyDown(KeyCode.Alpha3) && _shieldSpawner != null)
            _shieldSpawner.Show(Random.Range(1, 28));
        if (Input.GetKeyDown(KeyCode.Alpha4) && _agSpawner != null)
            _agSpawner.Show(Random.Range(5, 20));
#else
        Destroy(this);
#endif
    }
}
