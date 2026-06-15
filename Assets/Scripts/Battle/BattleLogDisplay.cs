using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Core;

/// <summary>
/// 战斗中无限向下滚动的战斗日志显示器。
/// 点击展开按钮弹出可拖拽小面板，关闭后停止接收日志以优化性能。
/// </summary>
public class BattleLogDisplay : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [Header("展开按钮(始终可见)")]
    [SerializeField] Button _expandButton;

    [Header("小面板根节点(可拖拽)")]
    [SerializeField] RectTransform _panelRoot;

    [Header("关闭按钮(面板上)")]
    [SerializeField] Button _closeButton;

    [Header("日志文本")]
    [SerializeField] TMP_Text _logText;

    [Header("滚动视图")]
    [SerializeField] ScrollRect _scrollRect;

    [Header("最大行数")]
    [SerializeField] int _maxLines = 300;

    [SerializeField] float _flushInterval = 0.12f;

    readonly List<string> _lineBuffer = new List<string>(64);
    readonly StringBuilder _sb = new StringBuilder(8192);
    float _flushTimer;
    bool _dirty;
    bool _autoScroll = true;
    bool _isExpanded;

    void Start()
    {
        _panelRoot.gameObject.SetActive(false);
        _expandButton.gameObject.SetActive(true);

        _expandButton.onClick.AddListener(OnExpandClicked);
        _closeButton.onClick.AddListener(OnCloseClicked);

        if (_scrollRect != null)
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
    }

    void OnExpandClicked()
    {
        _isExpanded = true;
        _panelRoot.gameObject.SetActive(true);
        _expandButton.gameObject.SetActive(false);

        var mgr = GameRoot.GetManager<BattleDebugManager>();
        if (mgr != null)
        {
            mgr.OnNewEntry += OnNewEntry;
            foreach (var entry in mgr.Entries)
                _lineBuffer.Add(entry);
            _dirty = true;
        }
    }

    void OnCloseClicked()
    {
        _isExpanded = false;
        _panelRoot.gameObject.SetActive(false);
        _expandButton.gameObject.SetActive(true);

        var mgr = GameRoot.GetManager<BattleDebugManager>();
        if (mgr != null)
            mgr.OnNewEntry -= OnNewEntry;

        _lineBuffer.Clear();
        _sb.Clear();
        if (_logText != null)
            _logText.text = "";
        _dirty = false;
    }

    void Update()
    {
        if (!_isExpanded || !_dirty) return;

        _flushTimer -= Time.unscaledDeltaTime;
        if (_flushTimer <= 0f)
            FlushText();
    }

    void OnNewEntry(string entry)
    {
        _lineBuffer.Add(entry);
        if (_lineBuffer.Count > _maxLines)
            _lineBuffer.RemoveRange(0, _lineBuffer.Count - _maxLines);
        _dirty = true;
    }

    void FlushText()
    {
        _flushTimer = _flushInterval;

        _sb.Clear();
        for (int i = 0; i < _lineBuffer.Count; i++)
            _sb.AppendLine(_lineBuffer[i]);
        _logText.text = _sb.ToString();

        if (_scrollRect != null && _scrollRect.content != null)
        {
            float preferredH = _logText.preferredHeight;
            if (preferredH > 0f)
                _scrollRect.content.sizeDelta = new Vector2(_scrollRect.content.sizeDelta.x, preferredH);
        }

        if (_autoScroll && _scrollRect != null)
            _scrollRect.verticalNormalizedPosition = 0f;

        _dirty = false;
    }

    void OnScrollValueChanged(Vector2 pos)
    {
        _autoScroll = pos.y <= 0.01f;
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isExpanded)
            _panelRoot.anchoredPosition += eventData.delta;
    }

    void OnDestroy()
    {
        if (_isExpanded)
        {
            var mgr = GameRoot.GetManager<BattleDebugManager>();
            if (mgr != null)
                mgr.OnNewEntry -= OnNewEntry;
        }
    }
}
