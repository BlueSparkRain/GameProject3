using System.Collections;
using DG.Tweening;
using UnityEngine;

public class DelayActive : MonoBehaviour
{
    [Header("激活前的等待时间")]
    public float waitDelay = 6;
    [Header("使用回弹动画")]
    public bool useBounceAnim = true;
    [Header("弹入动画时长")]
    public float scaleInDuration = 0.5f;
    [Header("橡皮回弹超调量(0=无_, 越大越弹)")]
    public float overshoot = 1.2f;
    SpriteRenderer _spriteRenderer;
    Vector3 _originalScale;

    void Start(){
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalScale = transform.localScale;
        StartCoroutine(WaitActive());
    }
    IEnumerator WaitActive()
    {
        _spriteRenderer.enabled = false;
        yield return new WaitForSeconds(waitDelay);

        _spriteRenderer.enabled = true;
        if (useBounceAnim)
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(_originalScale, scaleInDuration)
                .SetEase(Ease.OutBack, overshoot)
                .SetLink(gameObject);
        }
    }

    void OnDestroy()
    {
        transform.DOKill();
    }
}
