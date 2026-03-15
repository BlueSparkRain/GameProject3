using Core;
using DG.Tweening;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class ObjAnimTest : MonoBehaviour
{
    [Header("动画目标")]
    public GameObject nonSOTarget; // 拖入一个测试物体
    public Ease nonSOEase = Ease.OutBack;
    [Tooltip("上下摆动距离")]
    public float swingDistance = 1f;
    [Tooltip("单次摆动时长")]
    public float swingDuration = 1f;
    MagicAnimationManager animManager;

    private async void Start(){
        // 空值校验
        if (nonSOTarget == null){
            Debug.LogError("请拖入测试物体！");
            return;
        }
        animManager = GameRoot.Instance.GetManager<MagicAnimationManager>();

    }

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) {
            await PlayNonSOSwingAnim();
        }
    }

    /// <summary>
    /// 一个物体摆动委托
    /// </summary>
    private async Task PlayNonSOSwingAnim(){
        //制定动画参数
        var swingParams = new AnimParams{
            Duration = swingDuration,

            Ease = nonSOEase,
            LoopMode = AnimationLoopType.Yoyo,
            LoopCount = 2, // 无限循环
            Interruptible = true
        };
        //
        await animManager.PlayAnimationAsync(
            MagicAnimationManager.GetAnimID(E_TweenType.Swing_Box),
            nonSOTarget.transform,

            (p) => nonSOTarget.transform.DOLocalMoveY(
                nonSOTarget.transform.localPosition.y + swingDistance,
                p.Duration
            ).SetRelative(false),
            swingParams
        );
    }

    // 可选：快速停止所有动画（可绑定UI按钮）
    public void StopAllAnims(){
        animManager.InterruptAnimation(MagicAnimationManager.GetAnimID(E_TweenType.Swing_Box));
        Debug.Log("已停止所有摆动动画");
    }

    private void OnDestroy()
    {
        // 自动清理动画
        StopAllAnims();
    }
}