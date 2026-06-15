using Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家主动技能释放目标选择管理器 — 挂载到BattleScene中任意GameObject上。
/// 解耦：纯贝塞尔曲线渲染由 BezierArrowUI 负责，本Manager负责选择逻辑+标记。
/// 仅"敌方单体"类技能需要玩家手动选择目标，其余类型自动寻敌。
/// </summary>
public class ActiveSkillTargetSelectorManager : MonoBehaviour
{
    [Header("贝塞尔箭头预制件")]
    public GameObject bezierArrowPrefab;
    [Header("选中标记Image（放置在目标上方）")]
    public Image targetMarker;
    [Header("吸附距离（像素）")]
    public float snapDistance = 120f;
    [Header("标记在目标上方的偏移")]
    public Vector2 markerOffset = new Vector2(0, 60f);

    BezierArrowUI _activeArrow;
    RectTransform _skillIconRt;
    E_SkillTargetType_ATB _currentTargetType;
    IBattlable _confirmedTarget;
    RectTransform _snappedEnemyRt;
    bool _isSelecting;
    Canvas _parentCanvas;

    /// <summary>当前已确认的施法目标（ATBMode.Release优先使用）</summary>
    public IBattlable ConfirmedTarget => _confirmedTarget;

    void OnEnable()
    {
        EventCenter.AddEventListener<bool>(E_EventType.PrepareATBSkillExcute, OnPrepareATB);
        EventCenter.AddEventListener<RectTransform>(E_EventType.SkillIconATBSelected, OnSkillIconATBSelected);
        if (targetMarker != null)
            targetMarker.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        EventCenter.RemoveEventListener<bool>(E_EventType.PrepareATBSkillExcute, OnPrepareATB);
        EventCenter.RemoveEventListener<RectTransform>(E_EventType.SkillIconATBSelected, OnSkillIconATBSelected);
        Cleanup();
    }

    void OnSkillIconATBSelected(RectTransform skillIconRt)
    {
        _skillIconRt = skillIconRt;
    }

    void OnPrepareATB(bool entering)
    {
        if (entering)
            EnterSelection();
        else
            ExitSelection();
    }

    void EnterSelection()
    {
        var atbMode = ATBMode.CurrentSelected;
        if (atbMode == null) return;
        if (_skillIconRt == null) return;

        _currentTargetType = atbMode.SkillData.skill_ATBTargetType;

        // 仅"敌方单体"需要玩家手动箭头选择目标
        if (_currentTargetType == E_SkillTargetType_ATB.敌方单体)
        {
            _isSelecting = true;
            _confirmedTarget = null;
            ShowBezierArrow();
        }
    }

    void ExitSelection()
    {
        _isSelecting = false;
        HideBezierArrow();
        HideMarker();
        _confirmedTarget = null;
        _snappedEnemyRt = null;
        _skillIconRt = null;
    }

    void Update()
    {
        if (!_isSelecting) return;
        if (_activeArrow == null) return;

        Vector2 mousePos = Input.mousePosition;

        // 查找最近Enemy-tag UI并吸附
        RectTransform nearest = FindNearestEnemy(mousePos);
        Vector2 endPt = mousePos;

        if (nearest != null)
        {
            _snappedEnemyRt = nearest;
            endPt = RectTransformUtility.WorldToScreenPoint(null, nearest.position);
        }
        else
        {
            _snappedEnemyRt = null;
        }

        _activeArrow.UpdateCurve(_skillIconRt.position, endPt);

        // 鼠标点击确认目标
        if (Input.GetMouseButtonDown(0) && _snappedEnemyRt != null)
            ConfirmTarget(_snappedEnemyRt);
    }

    void ShowBezierArrow()
    {
        if (bezierArrowPrefab == null || _skillIconRt == null) return;

        _parentCanvas = _skillIconRt.GetComponentInParent<Canvas>();
        if (_parentCanvas == null) return;

        var arrowGo = Instantiate(bezierArrowPrefab, _parentCanvas.transform);
        _activeArrow = arrowGo.GetComponent<BezierArrowUI>();
        if (_activeArrow == null)
        {
            Destroy(arrowGo);
            return;
        }

        _activeArrow.SetStartPoint(_skillIconRt.position);
        _activeArrow.Show();
    }

    void HideBezierArrow()
    {
        if (_activeArrow != null)
        {
            _activeArrow.Hide();
            Destroy(_activeArrow.gameObject);
            _activeArrow = null;
        }
    }

    void ConfirmTarget(RectTransform enemyRt)
    {
        _confirmedTarget = GetBattlableFromRect(enemyRt);
        ShowMarker(enemyRt);
        _isSelecting = false;
        DebugManager.Log(EDebugCategory.General,
            $"[ActiveSkillTargetSelector] 已确认目标: {_confirmedTarget?.GetType().Name}");
    }

    void ShowMarker(RectTransform targetRt)
    {
        if (targetMarker == null) return;
        targetMarker.gameObject.SetActive(true);
        targetMarker.rectTransform.position = targetRt.position + (Vector3)markerOffset;
    }

    void HideMarker()
    {
        if (targetMarker != null)
            targetMarker.gameObject.SetActive(false);
    }

    RectTransform FindNearestEnemy(Vector2 mousePos)
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        RectTransform nearest = null;
        float nearestDist = snapDistance;

        foreach (var go in enemies)
        {
            var rt = go.transform as RectTransform;
            if (rt == null) continue;
            if (!rt.gameObject.activeInHierarchy) continue;

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, rt.position);
            float dist = Vector2.Distance(mousePos, screenPos);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = rt;
            }
        }
        return nearest;
    }

    IBattlable GetBattlableFromRect(RectTransform rt)
    {
        if (rt == null) return null;
        var handler = rt.GetComponentInParent<BattleHandler>();
        return handler?.Self;
    }

    void Cleanup()
    {
        HideBezierArrow();
        HideMarker();
        _isSelecting = false;
        _confirmedTarget = null;
        _snappedEnemyRt = null;
    }
}
