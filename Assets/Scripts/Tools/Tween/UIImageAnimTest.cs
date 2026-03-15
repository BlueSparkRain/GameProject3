using System.Threading.Tasks;
using Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Image动画测试脚本（无报错终极版）
/// 核心：移除OnKill回调，修复空引用错误，支持打断无突变
/// </summary>
public class UIImageAnimTest : MonoBehaviour
{
    public Image imageNonSO;
    public float nonSOAnimDuration = 0.5f;
    public Ease nonSOEase = Ease.OutBack;
    public float nonSOEnterLocalY = 0f; // 入场Y值
    public float nonSOExitLocalY = -1000f; // 出场Y值

    // 动画状态+ID
    private bool _isNonSOShowing = false;
    private string _nonSOAnimId;
    private MagicAnimationManager _animManager;

    private void Start()
    {
        // 初始化管理器
        _animManager = GameRoot.Instance?.GetManager<MagicAnimationManager>();
        if (_animManager == null)
        {
            Debug.LogError("未找到MagicAnimationManager！");
            enabled = false;
            return;
        }

        // 初始化Image（仅改Y轴，保留X/Z）
        if (imageNonSO != null)
        {
            ResetImageY(imageNonSO, nonSOExitLocalY);
            imageNonSO.raycastTarget = false;
        }

        _nonSOAnimId = null;
    }

    private void Update()
    {
        // 1键：触发/打断动画（无冷却，测试打断）
        if (Input.GetKeyDown(KeyCode.Alpha1) && imageNonSO != null)
        {
            ToggleNonSOAnim();
        }

        // 0键：重置
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ResetAllAnims();
        }
    }

    #region 核心动画逻辑（无报错+打断优先）
    private void ToggleNonSOAnim()
    {
        // 步骤1：立即打断旧动画（核心：传false=不强制完成，不跳终点）
        if (!string.IsNullOrEmpty(_nonSOAnimId))
        {
            _animManager.InterruptAnimation(_nonSOAnimId, false);
        }

        // 步骤2：切换状态
        _isNonSOShowing = !_isNonSOShowing;

        // 步骤3：生成新ID并启动动画（异步无阻塞）
        _nonSOAnimId = _animManager.GenerateUniqueAnimId("NonSO_UI_");
        _ = PlayNonSOAnim(_isNonSOShowing, _nonSOAnimId);
    }

    private async Task PlayNonSOAnim(bool isShow, string animId)
    {
        // 构建参数
        var nonSOParams = new AnimParams
        {
            Duration = nonSOAnimDuration,
            Ease = nonSOEase,
            LoopMode = AnimationLoopType.None,
            Interruptible = true,
            TargetType = AnimationTargetType.UI,
            SpaceMode = AnimationSpaceMode.Local
        };

        try
        {
            await _animManager.PlayAnimationAsync(
                animId,
                imageNonSO.rectTransform,

                (p) =>
                {
                    // 核心：移除所有OnKill回调，避免空引用报错
                    float targetY = isShow ? nonSOEnterLocalY : nonSOExitLocalY;
                    float targetAlpha = isShow ? 1f : 0f;

                    // 仅Y轴移动Tween（保留X/Z）
                    var moveTween = imageNonSO.rectTransform.DOLocalMoveY(targetY, p.Duration)
                        .SetEase(p.Ease)
                        .SetAutoKill(false); // 由管理器控制生命周期

                    // 透明度Tween（移除OnKill回调，无报错）
                    var fadeTween = imageNonSO.DOFade(targetAlpha, p.Duration)
                        .SetEase(p.Ease)
                        .SetAutoKill(false);

                    // 合并为Sequence
                    var seq = DOTween.Sequence();
                    seq.Append(moveTween);
                    seq.Join(fadeTween);
                    seq.SetAutoKill(false);

                    return seq;
                },
                nonSOParams
            );

            //// 仅动画正常完成时更新射线检测
            //if (_nonSOAnimId == animId)
            //{
            //    imageNonSO.raycastTarget = isShow;
            //}
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"非SO动画失败：{ex.Message}");
            _nonSOAnimId = null;
        }
    }
    #endregion

    #region 辅助方法（极简+无突变）
    /// <summary>
    /// 仅重置Y轴，保留X/Z，强制Kill所有本地动画
    /// </summary>
    private void ResetImageY(Image img, float targetY)
    {
        if (img == null) return;

        // 强制Kill Image上的所有Tween（不完成）
        img.rectTransform.DOKill(false);
        img.DOKill(false);

        // 仅修改Y轴，保留X/Z
        Vector3 pos = img.rectTransform.localPosition;
        pos.y = targetY;
        img.rectTransform.localPosition = pos;

        // 直接改透明度，无动画
        Color c = img.color;
        c.a = 0;
        img.color = c;
    }

    /// <summary>
    /// 重置所有动画（无突变）
    /// </summary>
    public void ResetAllAnims()
    {
        // 打断旧动画（不完成）
        if (!string.IsNullOrEmpty(_nonSOAnimId))
        {
            _animManager.InterruptAnimation(_nonSOAnimId, false);
            _nonSOAnimId = null;
        }

        // 重置状态和位置
        _isNonSOShowing = false;
        if (imageNonSO != null)
        {
            ResetImageY(imageNonSO, nonSOExitLocalY);
            imageNonSO.raycastTarget = false;
        }

        Debug.Log("已重置所有动画，无突变！");
    }
    #endregion

    // 销毁/禁用时清理
    //private void OnDestroy() => ResetAllAnims();
    private void OnDisable() => ResetAllAnims();
}