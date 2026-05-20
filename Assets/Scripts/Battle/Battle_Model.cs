using System;
using UnityEngine;

/// <summary>
/// 战斗角色数据模型
/// </summary>
public class Battle_Model
{
    // 核心属性
    private float _maxHP;
    private float _hp;
    private float _maxSP;
    private float _sp;
    private float _maxAG;
    private float _ag;
    private int _maxAtbPoints;
    private int _AtbPoints;

    //盾点Info
    private int _maxShieldPoints;
    private int _shieldPoints;

    public event Action OnDataChanged;
    public event Action OnHPZero; 
    private float SafeValue(float value, float min, float max)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            return min;
        return Mathf.Clamp(value, min, max);
    }
    private int SafeValue(int value, int min, int max)
    {
        return Mathf.Clamp(value, min, max);
    }
    // 生命值
    public float HP
    {
        get => _hp;
        set{
            _hp = SafeValue(value, 0, _maxHP);
            OnDataChanged?.Invoke();
            if(_hp<=0)
                OnHPZero?.Invoke();}
    }
    public float MaxHP
    {
        get => _maxHP;
        set{
            _maxHP = SafeValue(value, 1, float.MaxValue);
            OnDataChanged?.Invoke();}
    }

    // 法力值/能量
    public float SP
    {
        get => _sp;
        set{
            _sp = SafeValue(value, 0, _maxSP);
            OnDataChanged?.Invoke();}
    }
    public float MaxSP{
        get => _maxSP;
        set{
            _maxSP = SafeValue(value, 1, float.MaxValue);
            OnDataChanged?.Invoke();}
    }

    // 怒气值
    public float AG{
        get => _ag;
        set{
            _ag = SafeValue(value, 0, _maxAG);
            OnDataChanged?.Invoke();}
    }
    public float MaxAG
    {
        get => _maxAG;
        set{
            _maxAG = SafeValue(value, 1, float.MaxValue);
            OnDataChanged?.Invoke();}
    }

    // ATB行动条
    public int ATBPoints{
        get => _AtbPoints;
        set{
            _AtbPoints = SafeValue(value, 0, _maxAtbPoints);
            OnDataChanged?.Invoke();}
    }
    public int MaxATBPoints
    {
        get => _maxAtbPoints;
        set
        {
            _maxAtbPoints = SafeValue(value, 1, int.MaxValue);
            OnDataChanged?.Invoke();
        }
    }

    public int ShieldPoints
    {
        get => _shieldPoints;
        set
        {
            _shieldPoints = SafeValue(value, 0, _maxShieldPoints);
            OnDataChanged?.Invoke();
        }
    }
    public int MaxShieldPoints
    {
        get => _maxShieldPoints;
        set
        {
            _maxShieldPoints = SafeValue(value, 1, int.MaxValue);
            OnDataChanged?.Invoke();
        }
    }


    // 构造函数
    public Battle_Model(float maxHp, float maxSp, int maxAtb, float maxAg=100,int maxShiled=5){
        MaxHP = maxHp;
        HP = maxHp;

        MaxSP = maxSp;
        SP = maxSp;

        MaxAG = maxAg;
        AG = 0;

        MaxATBPoints = maxAtb;
        ATBPoints = 0;

        MaxShieldPoints = maxShiled;
        ShieldPoints = maxShiled;
    }
}