using System.Collections;
using Core;
using UnityEngine;

/// <summary>
/// 技能悬停工具（纯C#类，无需挂载）——管理"鼠标悬停→延迟→呼叫SkillDetailPanel"的完整生命周期。
/// 用法：new SkillTooltipHover(owner, anchor, description) 后调用 Enter() / Exit() / Dispose()
/// </summary>
public class SkillTooltipHover
{
    readonly MonoBehaviour _owner;
    readonly Transform _anchor;
    readonly float _hoverDelay;
    readonly Vector2 _tooltipOffset;
    string _description;
    Coroutine _hoverCoroutine;
    SkillDetailPanel _detailPanel;

    /// <param name="owner">协程宿主（任意MonoBehaviour）</param>
    /// <param name="anchor">屏幕坐标锚点（通常传 transform）</param>
    /// <param name="description">技能描述文本</param>
    /// <param name="hoverDelay">悬停触发延迟（秒）</param>
    /// <param name="tooltipOffset">面板偏移（屏幕像素）</param>
    public SkillTooltipHover(MonoBehaviour owner, Transform anchor, string description,
        float hoverDelay = 0.3f, Vector2? tooltipOffset = null)
    {
        _owner = owner;
        _anchor = anchor;
        _description = description;
        _hoverDelay = hoverDelay;
        _tooltipOffset = tooltipOffset ?? new Vector2(0, 80f);
    }

    /// <summary>运行时更新描述文本（如技能数据变更时）</summary>
    public void SetDescription(string description) => _description = description;

    /// <summary>指针进入时调用</summary>
    public void Enter()
    {
        if (string.IsNullOrEmpty(_description)) return;
        _hoverCoroutine = _owner.StartCoroutine(ShowDelayed());
    }

    /// <summary>指针离开时调用</summary>
    public void Exit()
    {
        if (_hoverCoroutine != null)
        {
            _owner.StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = null;
        }
        _detailPanel?.HideTooltip();
    }

    /// <summary>销毁时调用（清理面板引用和协程）</summary>
    public void Dispose()
    {
        Exit();
        _detailPanel = null;
        _description = null;
    }

    IEnumerator ShowDelayed()
    {
        yield return new WaitForSecondsRealtime(_hoverDelay);
        if (_owner == null || _owner.transform.parent == null) yield break;
        if (_detailPanel == null)
            _detailPanel = GameRoot.GetManager<UIManager>().OpenPanel<SkillDetailPanel>(E_UIPanelType.SkillDetailPanel);

        var screenPos = (Vector2)_anchor.position + _tooltipOffset;
        _detailPanel.ShowTooltip(screenPos, _description);
    }
}
