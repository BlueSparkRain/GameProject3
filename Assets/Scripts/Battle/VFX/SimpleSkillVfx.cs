using DG.Tweening;
using UnityEngine;

/// <summary>
/// 简易技能特效 — 挂载到预制件上，提供开箱即用的入场/驻留/退场动画。
/// 模式：Impact(缩放爆发) / Rise(上升消散) / Pulse(心跳缩放) / Trail(拖尾)
/// </summary>
public class SimpleSkillVfx : MonoBehaviour
{
    public enum VfxMode { Impact, Rise, Pulse, Trail }

    [SerializeField] VfxMode _mode = VfxMode.Impact;
    [SerializeField] float _duration = 0.4f;
    [SerializeField] float _startScale = 0.3f;
    [SerializeField] float _peakScale = 1.2f;
    [SerializeField] Color _color = new Color(1, 0.9f, 0.3f, 0.9f);
    [SerializeField] bool _autoDestroy = true;

    MeshRenderer _mr;
    float _elapsed;

    void Awake()
    {
        _mr = GetComponent<MeshRenderer>();
        if (_mr == null)
            _mr = gameObject.AddComponent<MeshRenderer>();

        // 确保有材质实例
        if (_mr.material == null || _mr.sharedMaterial == null)
            _mr.material = new Material(Shader.Find("Sprites/Default"));
        _mr.material.color = _color;

        transform.localScale = Vector3.one * _startScale;
        Play();
    }

    void Play()
    {
        switch (_mode)
        {
            case VfxMode.Impact:
                transform.DOScale(_peakScale, _duration * 0.4f).SetEase(Ease.OutBack)
                    .OnComplete(() => transform.DOScale(0.01f, _duration * 0.6f).SetEase(Ease.InQuad));
                _mr.material.DOColor(new Color(_color.r, _color.g, _color.b, 0), _duration);
                break;

            case VfxMode.Rise:
                var seq = DOTween.Sequence();
                seq.Join(transform.DOMoveY(transform.position.y + 2.5f, _duration).SetEase(Ease.OutCubic));
                seq.Join(transform.DOScale(_peakScale, _duration * 0.5f).SetEase(Ease.OutBack));
                seq.Join(_mr.material.DOColor(new Color(_color.r, _color.g, _color.b, 0), _duration));
                break;

            case VfxMode.Pulse:
                var ps = DOTween.Sequence();
                ps.Append(transform.DOScale(_peakScale, _duration * 0.3f).SetEase(Ease.OutSine));
                ps.Append(transform.DOScale(_startScale, _duration * 0.3f).SetEase(Ease.InSine));
                ps.SetLoops(-1);
                _mr.material.DOColor(new Color(_color.r, _color.g, _color.b, 0.3f), _duration)
                    .SetEase(Ease.InQuad);
                break;

            case VfxMode.Trail:
                transform.DOScale(_peakScale, _duration * 0.3f).SetEase(Ease.OutSine);
                _mr.material.DOColor(new Color(_color.r, _color.g, _color.b, 0), _duration * 1.2f);
                break;
        }

        if (_autoDestroy)
            DOVirtual.DelayedCall(_duration + 0.05f, () =>
            {
                if (this != null && gameObject != null)
                    Destroy(gameObject);
            });
    }
}
