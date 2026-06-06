using Core;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 六边形跳动动画组件（抽离动画逻辑，可挂载到任意物体）
/// </summary>
[RequireComponent(typeof(Transform))]
public class HexJumpAnimHandler : MonoBehaviour
{
    // 基础配置（可在Inspector面板调整）
    [Header("跳动配置")]
    [Tooltip("基础跳动高度（距离为0时的最大高度）")]
    public float baseJumpHeight = 0.6f;
    [Tooltip("基础动画时长")]
    public float baseDuration = 0.15f;
    [Tooltip("动画缓动曲线")]
    public Ease jumpEase = Ease.InOutBounce;

    [Header("悬浮配置")]
    [Tooltip("鼠标悬浮时子物体上浮高度")]
    public float hoverHeight = 0.5f;
    [Tooltip("悬浮动画时长")]
    public float hoverDuration = 0.1f;
    [Tooltip("悬浮时透明度渐入时长（应比hoverDuration长约0.2s）")]
    public float hoverFadeInDuration = 0.4f;
    [Tooltip("悬浮结束时透明度渐出时长")]
    public float hoverFadeOutDuration = 0.3f;

    [Header("(可行走)地形高度差")]
    float heightDistance = 0.2f;
    float baseHeightDuration = 0.2f;
    private Transform _selfTrans;
    private Vector3 _originalPos; // 记录初始位置，避免跳动后偏移

    private Transform cloudeTrans;
    public void InitPos(Vector3 pos){
        _selfTrans = transform;
        transform.position = pos;
        _originalPos = _selfTrans.localPosition;
    }

    /// <summary>
    /// 触发跳动动画（外部调用）
    /// </summary>
    /// <param name="distanceRatio">距离系数（0~1，越远越小）</param>
    /// <param name="delay">延迟执行时间</param>
    public void TriggerJump(float distanceRatio, float delay = 0f)
    {
        // 重置位置，避免多次触发导致偏移
        _selfTrans.localPosition = _originalPos;

        // 计算实际跳动高度（距离越远，高度越小）
        float actualHeight = baseJumpHeight * (1 - distanceRatio);
        if (actualHeight < 0.01f) actualHeight = 0.01f; // 避免高度为0

        float rand_Height = 2.0f;
        //float rand_Height = Random.Range(0.8f, 1.5f);
        float rand_Duration = 0.5f;

        // 执行跳动动画
        _selfTrans.DOLocalMoveY(_originalPos.y + actualHeight * rand_Height, baseDuration * rand_Duration * 0.5f)
            .SetEase(jumpEase)
            .SetDelay(delay)
            .OnComplete(() =>{
                // 回落动画
                _selfTrans.DOLocalMoveY(_originalPos.y, baseDuration * 0.4f)
                    .SetEase(jumpEase);
            });
    }
    public void WalkableUpAnim(){
        float rand_Height =0.5f; 
        float rand_Duration = Random.Range(0.5f, 0.8f);
        _selfTrans.DOLocalMoveY(_originalPos.y + heightDistance * rand_Height,
            baseHeightDuration * rand_Duration).SetEase(jumpEase);
        _originalPos.y = _originalPos.y + heightDistance * rand_Height;
    }

    public void CloudeAppear(Transform _cloudeTrans) {
        cloudeTrans= _cloudeTrans;
        Vector3 cloudeStartPos = cloudeTrans.position;
        cloudeTrans.DOLocalMoveY(-9f, baseHeightDuration * 3)
            .SetEase(jumpEase)
            .OnComplete(()=> cloudeTrans.GetComponent<CloudFloatOptimized>().DoCloudeAnim());
    }
    bool cloudeDisappear=false;
    public void CloudeDisAppear() {
        if (cloudeTrans == null)
            return;
        if (!cloudeDisappear)
        {
            cloudeDisappear = true;
            cloudeTrans.GetComponent<CloudFloatOptimized>().StopCloudAnim();
            Vector3 cloudeStartPos = cloudeTrans.position;
            cloudeTrans.DOScale(0f, baseHeightDuration * 3);
            cloudeTrans.DOLocalMoveY(10f, baseHeightDuration * 10).SetEase(jumpEase).OnComplete(
                ()=> GameRoot.GetManager<ObjectPoolManager>().ReturnPool(E_PoolType.RoomCloude_房间遮云,cloudeTrans.gameObject));
        }
    }

    Transform _firstChild;
    SpriteRenderer _firstChildSprite;
    Vector3 _childOriginalPos;
    bool _childCached;

    void Awake(){
        CacheFirstChild();
    }

    void CacheFirstChild(){
        if (_childCached && _firstChild != null) return;
        if (transform.childCount > 0){
            _firstChild = transform.GetChild(0);
            _firstChildSprite = _firstChild.GetComponent<SpriteRenderer>();
            _childOriginalPos = _firstChild.localPosition;
            // 初始透明度设为0
            if (_firstChildSprite != null)
            {
                var c = _firstChildSprite.color;
                c.a = 0f;
                _firstChildSprite.color = c;
            }
            _childCached = true;
        }
    }

    public void HoverUp(){
        CacheFirstChild();
        if (_firstChild == null) return;
        _firstChild.DOKill();
        if (_firstChildSprite != null)
        {
            _firstChildSprite.DOKill();
            var c = _firstChildSprite.color;
            c.a = 0f;
            _firstChildSprite.color = c;
            _firstChildSprite.DOFade(1f, hoverFadeInDuration).SetEase(Ease.OutQuad);
        }
        _firstChild.DOLocalMoveZ(_childOriginalPos.z + hoverHeight * 10, hoverDuration).SetEase(Ease.OutQuad);
    }

    public void HoverDown(){
        CacheFirstChild();
        if (_firstChild == null) return;
        _firstChild.DOKill();
        if (_firstChildSprite != null){
            _firstChildSprite.DOKill();
            _firstChildSprite.DOFade(0f, hoverFadeOutDuration).SetEase(Ease.OutQuad)
                .OnComplete(() =>{
                    _firstChild.DOLocalMoveZ(_childOriginalPos.z, hoverDuration).SetEase(Ease.OutQuad);
                });
        }
        else{
            _firstChild.DOLocalMoveZ(_childOriginalPos.z, hoverDuration).SetEase(Ease.OutQuad);
        }
    }
}