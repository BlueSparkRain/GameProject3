using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 控制 URP Volume 上 Bloom 强度的不对称 ping-pong 浮动。
/// 上升和下降使用独立的随机速度区间，方向切换时择速，始终平滑。
/// </summary>
public class BloomBreathing : MonoBehaviour
{
    [Header("目标")]
    public Volume targetVolume;

    [Header("Bloom 强度范围")]
    public float minIntensity = 0.2f;
    public float maxIntensity = 1.5f;

    [Header("整体速度倍率")]
    public float overallSpeed = 1f;

    [Header("上升速度区间 (min → max)")]
    public float riseSpeedMin = 0.5f;
    public float riseSpeedMax = 1f;

    [Header("下降速度区间 (max → min)")]
    public float fallSpeedMin = 1f;
    public float fallSpeedMax = 1.5f;

    Bloom _bloom;
    float _t;
    float _currentSpeed;
    bool _goingUp = true;

    void Start()
    {
        _currentSpeed = PickRiseSpeed();
        ResolveBloom();
    }

    void Update()
    {
        if (_bloom == null)
        {
            ResolveBloom();
            if (_bloom == null) return;
        }

        float dt = Time.deltaTime * overallSpeed;

        if (_goingUp){
            _t += _currentSpeed * dt;
            if (_t >= 1f)
            {
                _t = 1f;
                _goingUp = false;
                _currentSpeed = PickFallSpeed();
            }
        }
        else
        {
            _t -= _currentSpeed * dt;
            if (_t <= 0f)
            {
                _t = 0f;
                _goingUp = true;
                _currentSpeed = PickRiseSpeed();
            }
        }

        _bloom.intensity.value = Mathf.Lerp(minIntensity, maxIntensity, _t);
    }

    float PickRiseSpeed()  => Random.Range(riseSpeedMin, riseSpeedMax);
    float PickFallSpeed()  => Random.Range(fallSpeedMin, fallSpeedMax);

    void ResolveBloom()
    {
        if (targetVolume == null) return;
        var profile = targetVolume.sharedProfile;
        if (profile == null) return;
        if (!profile.TryGet(out _bloom))
            _bloom = profile.Add<Bloom>(false);
        if (_bloom != null)
        {
            _bloom.active = true;
            _bloom.intensity.overrideState = true;
        }
    }
}
