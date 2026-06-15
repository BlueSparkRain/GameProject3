using System;
using UnityEngine;

/// <summary>
/// 战斗角色数据模型。所有核心数值的权威来源。
/// 使用脏标记合并 OnDataChanged 事件：一帧内多次变更只触发一次 UI 刷新。
/// </summary>
public class Battle_Model
{
    float _maxHP;
    float _hp;
    float _maxSP;
    float _sp;
    float _maxAG;
    float _ag;
    int _maxAtbPoints;
    int _atbPoints;
    int _maxShieldPoints;
    int _shieldPoints;

    bool _dirty;

    public event Action OnDataChanged;
    public event Action OnHPZero;
    public event Action<float, float> OnHPChanged;
    public event Action OnShieldBreak;

    static float SafeValue(float value, float min, float max)
    {
        if (float.IsNaN(value) || float.IsInfinity(value)) return min;
        return Mathf.Clamp(value, min, max);
    }
    static int SafeValue(int value, int min, int max) => Mathf.Clamp(value, min, max);
    // ── HP ──
    public float HP{
        get => _hp;
        set{
            _hp = SafeValue(value, 0, _maxHP);
            MarkDirty();
            OnHPChanged?.Invoke(_hp, _maxHP);
            if (_hp <= 0) OnHPZero?.Invoke();
        }
    }
    public float MaxHP
    {
        get => _maxHP;
        set { _maxHP = SafeValue(value, 1, float.MaxValue); MarkDirty(); }
    }

    // ── SP ──
    public float SP
    {
        get => _sp;
        set { _sp = SafeValue(value, 0, _maxSP); MarkDirty(); }
    }
    public float MaxSP
    {
        get => _maxSP;
        set { _maxSP = SafeValue(value, 1, float.MaxValue); MarkDirty(); }
    }

    // ── AG → ATB 自动溢出转换 ──
    public float AG
    {
        get => _ag;
        set
        {
            float v = value;
            while (v >= _maxAG)
            {
                v -= _maxAG;
                _atbPoints = SafeValue(_atbPoints + 1, 0, _maxAtbPoints);
            }
            _ag = SafeValue(v, 0, _maxAG);
            MarkDirty();
        }
    }
    public float MaxAG
    {
        get => _maxAG;
        set { _maxAG = SafeValue(value, 1, float.MaxValue); MarkDirty(); }
    }

    // ── ATB ──
    public int ATBPoints
    {
        get => _atbPoints;
        set { _atbPoints = SafeValue(value, 0, _maxAtbPoints); MarkDirty(); }
    }
    public int MaxATBPoints
    {
        get => _maxAtbPoints;
        set { _maxAtbPoints = SafeValue(value, 1, int.MaxValue); MarkDirty(); }
    }

    // ── Shield ──
    public int ShieldPoints
    {
        get => _shieldPoints;
        set
        {
            _shieldPoints = SafeValue(value, 0, _maxShieldPoints);
            MarkDirty();
            if (_shieldPoints <= 0) OnShieldBreak?.Invoke();
        }
    }
    public int MaxShieldPoints
    {
        get => _maxShieldPoints;
        set { _maxShieldPoints = SafeValue(value, 1, int.MaxValue); MarkDirty(); }
    }

    void MarkDirty() { _dirty = true; }

    /// <summary>每帧调用一次：有脏数据时才触发 UI 刷新</summary>
    public void FlushUI()
    {
        if (!_dirty) return;
        _dirty = false;
        OnDataChanged?.Invoke();
    }

    public Battle_Model(float maxHp, float maxSp, int maxAtb, float maxAg = 100, int maxShield = 5)
    {
        MaxHP = maxHp;        HP = maxHp;
        MaxSP = maxSp;        SP = maxSp;
        MaxAG = maxAg;        AG = 0;
        MaxATBPoints = maxAtb; ATBPoints = 0;
        MaxShieldPoints = maxShield; ShieldPoints = maxShield;
    }
}
