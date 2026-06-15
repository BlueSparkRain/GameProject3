using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责战斗中角色的BUFF的[注册],[移除] 与 [事件驱动注册] [帧驱动]
/// </summary>
public class BattleBuffHandler : MonoBehaviour
{
    Dictionary<BuffBase, BuffTimer> BuffDic = new Dictionary<BuffBase, BuffTimer>();
    IBattlable self;
    public void InitBattleBuffHandle(IBattlable _self)
    {
        self = _self;
        EventCenter.AddEventListener<BattleBuffHandler>(E_EventType.Do_PhyAttack, Check_Phy_AdditiveBuff);
        EventCenter.AddEventListener<BattleBuffHandler, SkillBase, E_SkillLevel, int>(E_EventType.Do_MagAttack, OnMagAttackRecast);
        EventCenter.AddEventListener<BattleBuffHandler, BuffBase>(E_EventType.Battle_RegisteBUFF, RegistBuff);
        EventCenter.AddEventListener<BattleBuffHandler, E_WeaknessType, IBattlable>(E_EventType.Battle_ElementalAttack, OnElementalAttack);
    }
    void Check_Phy_AdditiveBuff(BattleBuffHandler buffHandler)
    {
        if (BuffDic.Count <= 0)
        {
            return;
        }
        if (buffHandler == this)
        {
            foreach (var item in BuffDic)
            {
                if (item.Key.Buff_Type == E_BuffType.炽焰连锁)
                    item.Key.OnBuffTrigger();
            }
        }
    }

    void OnMagAttackRecast(BattleBuffHandler buffHandler, SkillBase skill, E_SkillLevel skillLevel, int henctime)
    {
        if (buffHandler != this) return;
        foreach (var item in BuffDic)
        {
            if (item.Key.Buff_Type == E_BuffType.大魔法化_正面 || item.Key.Buff_Type == E_BuffType.超大魔法化_正面)
            {
                var recast = item.Key as Buff_SkillRecast;
                recast.SetRecastContext(skill, skillLevel, henctime);
                recast.OnBuffTrigger();
            }
        }
    }
    void OnElementalAttack(BattleBuffHandler buffHandler, E_WeaknessType weaknessType, IBattlable target)
    {
        if (buffHandler != this) return;
        foreach (var kv in BuffDic)
        {
            if (kv.Key is Buff_DotOnAttack dotOnAttack)
                dotOnAttack.TryApplyDot(weaknessType, target);
        }
    }

    /// <summary>
    /// 为单位添加一个新的BUFF
    /// </summary>
    /// <param name="buffHandle"></param>
    /// <param name="buff"></param>
    public void RegistBuff(BattleBuffHandler buffHandle, BuffBase buff)
    {
        if (buffHandle == this)
        {
            //如果已经拥有同名的BUFF，直接刷新计时(用Buff_Type比较而不是对象)
            BuffBase existKey = null;
            foreach (var k in BuffDic.Keys)
            {
                if (k.Buff_Type == buff.Buff_Type) { existKey = k; break; }
            }
            if (existKey != null)
            {
                BuffDic[existKey].ResetTimer();
                DebugManager.Log(EDebugCategory.BattleBuff,string.Format("BUFF:{0}重复获取，刷新BUFF持续时间", buff.Buff_Type));
                BattleDebugManager.LogFormat("{0} 的 {1} 已刷新", self.battleDamageHandler.BattleController.CharacterData.Character_Name, buff.Buff_Type);
                return;
            }
            //新BUFF注册
            BuffDic.Add(buff, new BuffTimer(buff.Buff_Dura));
            DebugManager.Log(EDebugCategory.BattleBuff,string.Format("{0} get New BUFF:{1}-{2}", self.Camp, buff.Buff_Attr, buff.Buff_Type));
            BattleDebugManager.LogFormat("{0} 获得了 {1}",
                self.battleDamageHandler.BattleController.CharacterData.Character_Name, buff.Buff_Type);

        }
    }

    public void UnRegistBuff(E_BuffType buffType){
        foreach (var buff in BuffDic)
        {
            if (buff.Key.Buff_Type == buffType)
            {
                DebugManager.Log(EDebugCategory.BattleBuff,$"{self.battleDamageHandler.name}移除了BUFF：{buffType}");
                BattleDebugManager.LogFormat("{0} 的 {1} 已移除",
                    self.battleDamageHandler.BattleController.CharacterData.Character_Name, buffType);
                buff.Key.OnBuffRemove();
                BuffDic.Remove(buff.Key);
                return;
            }
        }
        DebugManager.Log(EDebugCategory.BattleBuff,$"{self.battleDamageHandler.name}未找到BUFF：{buffType}，移除失败");
    }

    public void UnRegistBuffsByAttr(E_BuffPositive attr)
    {
        var toRemove = new List<BuffBase>();
        foreach (var kv in BuffDic)
        {
            if (kv.Key.Buff_Attr == attr)
                toRemove.Add(kv.Key);
        }
        foreach (var key in toRemove)
        {
            DebugManager.Log(EDebugCategory.BattleBuff,$"{self.battleDamageHandler.name}移除了BUFF：{key.Buff_Type}");
            BattleDebugManager.LogFormat("{0} 的 {1} 已移除",
                self.battleDamageHandler.BattleController.CharacterData.Character_Name, key.Buff_Type);
            key.OnBuffRemove();
            BuffDic.Remove(key);
        }
    }

    public void ExtendBuffTimers(E_BuffPositive attr, float extraSeconds)
    {
        foreach (var kv in BuffDic)
        {
            if (kv.Key.Buff_Attr == attr)
                kv.Value.ExtendTimer(extraSeconds);
        }
    }

    public void ExtendBuffByType(E_BuffType buffType, float extraSeconds){
        foreach (var kv in BuffDic)
        {
            if (kv.Key.Buff_Type == buffType)
            {
                kv.Value.ExtendTimer(extraSeconds);
                return;
            }
        }
    }

    public List<BuffBase> GetBuffsByAttr(E_BuffPositive attr)
    {
        var result = new List<BuffBase>();
        foreach (var kv in BuffDic)
        {
            if (kv.Key.Buff_Attr == attr)
                result.Add(kv.Key);
        }
        return result;
    }

    public BuffBase TryGetBuff(E_BuffType buffType)
    {
        foreach (var buff in BuffDic)
        {
            if (buff.Key.Buff_Type == buffType)
                return buff.Key;
        }
        return null;
    }

    public E_SkillTargetType_Auto GetModifiedTargetType(E_SkillTargetType_Auto original, bool isMagic)
    {
        if (!isMagic && TryGetBuff(E_BuffType.无双_正面) != null)
        {
            DebugManager.Log(EDebugCategory.BattleBuff,$"{self.battleDamageHandler.name}无双发动：物理技能目标变为全体");
            return E_SkillTargetType_Auto.对全体;
        }
        if (isMagic && TryGetBuff(E_BuffType.魔力收束_正面) != null)
        {
            DebugManager.Log(EDebugCategory.BattleBuff,$"{self.battleDamageHandler.name}魔力收束发动：魔法技能目标变为单体");
            return E_SkillTargetType_Auto.对单体;
        }
        return original;
    }
    public void OnBuffUpdate()
    {
        if (BuffDic.Count <= 0)
            return;
        //移除应过期的buff
        var expired = new List<BuffBase>();
        foreach (var buffUni in BuffDic)
        {
            if (buffUni.Value.Tick())
                expired.Add(buffUni.Key);
        }
        foreach (var key in expired)
        {
            DebugManager.Log(EDebugCategory.BattleBuff,"移除BUFF" + key.Buff_Type);
            BattleDebugManager.LogFormat("{0} 的 {1} 已过期",
                self.battleDamageHandler.BattleController.CharacterData.Character_Name, key.Buff_Type);
            BuffDic.Remove(key);
        }
        //buff帧驱动
        foreach (var buffUni in BuffDic)
        {
            buffUni.Key.OnBuffUpdate();
        }
    }

    public float GetDamageRate()
    {
        var Buff = TryGetBuff(E_BuffType.战意_正面) as Buff_DamageBoomer;
        if (Buff == null) { return 0.0f; }
        else
        {
            DebugManager.Log(EDebugCategory.BattleBuff,self.battleDamageHandler.name + "获得了伤害倍率BUFF,倍率:" + Buff.BoomerRate);
            return Buff.BoomerRate;
        }

    }

    //GameRoot.GetManager<CoroutineManager>().StartCoroutine(WaitBuffUnRegiste(buff));
    /// <summary>
    /// 协程BUFF计时，定时并自动移除BUFF
    /// </summary>
    /// <param name="buff"></param>
    /// <returns></returns>
    IEnumerator WaitBuffUnRegiste(BuffBase buff)
    {
        yield return new WaitForSeconds(buff.Buff_Dura);
        DebugManager.Log(EDebugCategory.BattleBuff,string.Format("{0} lose BUFF:{1}-{2}", self.Camp, buff.Buff_Attr, buff.Buff_Type));
        BuffDic.Remove(buff);
    }

}

public class BuffTimer
{
    float timer;
    float interval;
    bool start;
    public BuffTimer(float interval)
    {
        this.interval = interval;
        ResetTimer();
    }
    public void ResetTimer()
    {
        timer = interval;
        start = true;
    }
    public void ExtendTimer(float extraSeconds)
    {
        timer += extraSeconds;
    }

    /// <summary>推进计时，过期返回 true</summary>
    public bool Tick()
    {
        if (!start) return false;
        timer -= Time.deltaTime;
        return timer < 0;
    }

}
