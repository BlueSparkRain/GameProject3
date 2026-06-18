using Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 主动技能敌方目标选择。SkillIcon → ActiveSkillCastTarget 事件 → A/D 切换 / 左键确认。
/// </summary>
public class ActiveSkillTargetSelectorManager : MonoBehaviour
{
    public Image targetMarker;
    public Vector2 markerOffset = new Vector2(0, 80f);

    IBattlable _confirmedTarget;
    bool _isSelecting;
    int _currentIndex;

    public IBattlable ConfirmedTarget => _confirmedTarget;

    void Awake()
    {
        if (targetMarker == null)
            targetMarker = GameObject.Find("TargetMarker")?.GetComponent<Image>();
    }

    void OnEnable()
    {
        EventCenter.AddEventListener(E_EventType.ActiveSkillCastTarget, OnActiveSkillCastTarget);
        EventCenter.AddEventListener<bool>(E_EventType.PrepareATBSkillExcute, OnPrepareATBExit);
        if (targetMarker != null)
            targetMarker.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        EventCenter.RemoveEventListener(E_EventType.ActiveSkillCastTarget, OnActiveSkillCastTarget);
        EventCenter.RemoveEventListener<bool>(E_EventType.PrepareATBSkillExcute, OnPrepareATBExit);
        if (targetMarker != null)
            targetMarker.gameObject.SetActive(false);
        _isSelecting = false;
        _confirmedTarget = null;
    }

    void OnActiveSkillCastTarget()
    {
        var stateMgr = GameRoot.GetManager<BattleStateManager>();
        var enemies = stateMgr?.EnemyControllers;
        if (enemies == null || enemies.Count == 0) return;
        _isSelecting = true;
        _confirmedTarget = null;
        _currentIndex = 0;
        RefreshMarker();
    }

    void OnPrepareATBExit(bool entering)
    {
        if (!entering)
        {
            _isSelecting = false;
            _confirmedTarget = null;
            if (targetMarker != null)
                targetMarker.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!_isSelecting) return;
        var enemies = GameRoot.GetManager<BattleStateManager>()?.EnemyControllers;
        if (enemies == null || enemies.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            _currentIndex = (_currentIndex - 1 + enemies.Count) % enemies.Count;
            RefreshMarker();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            _currentIndex = (_currentIndex + 1) % enemies.Count;
            RefreshMarker();
        }

        if (Input.GetMouseButtonDown(0))
            ConfirmCurrentTarget();
    }

    void RefreshMarker()
    {
        if (targetMarker == null) return;

        var t = GetEnemyTransform(_currentIndex);
        if (t == null)
        {
            targetMarker.gameObject.SetActive(false);
            return;
        }

        Vector2 pos;
        var rt = t as RectTransform;
        if (rt != null)
            pos = rt.position;
        else
            pos = Camera.main.WorldToScreenPoint(t.position);

        targetMarker.gameObject.SetActive(true);
        targetMarker.rectTransform.position = pos + (Vector2)markerOffset;
    }

    void ConfirmCurrentTarget()
    {
        var t = GetEnemyTransform(_currentIndex);
        if (t == null) return;
        var handler = t.GetComponent<BattleHandler>();
        if (handler != null)
        {
            _confirmedTarget = handler.Self;
            _isSelecting = false;
        }
    }

    Transform GetEnemyTransform(int index)
    {
        var stateMgr = GameRoot.GetManager<BattleStateManager>();
        var enemies = stateMgr?.EnemyControllers;
        if (enemies == null || index < 0 || index >= enemies.Count) return null;
        return stateMgr.GetEnemyTransform(enemies[index]);
    }
}
