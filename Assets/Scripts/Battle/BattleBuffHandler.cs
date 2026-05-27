using Core;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 管理战斗中角色的BUFF的【注册],[移除] 与 [事件触发注册]，[帧更新]
/// </summary>
public class BattleBuffHandler : MonoBehaviour{
    Dictionary<BuffBase,BuffTimer>  BuffDic=new Dictionary<BuffBase, BuffTimer> ();
    IBattlable self;
    public void InitBattleBuffHandle(IBattlable _self) {
        self= _self;
        EventCenter.AddEventListener<BattleBuffHandler>(E_EventType.Do_PhyAttack, Check_Phy_AdditiveBuff);
        EventCenter.AddEventListener<BattleBuffHandler, BuffBase>(E_EventType.Battle_RegisteBUFF, RegistBuff);
    }
    void Check_Phy_AdditiveBuff(BattleBuffHandler buffHandler){
        if (BuffDic.Count <= 0){
            return;
        }
        if (buffHandler == this) { 
            foreach (var item in BuffDic){
                if (item.Key.Buff_Type == E_BuffType.炽焰连锁)
                    item.Key.OnBuffTrigger();
            }
        }
    }

    /// <summary>
    /// 本单位获得一个新的BUFF
    /// </summary>
    /// <param name="buffHandle"></param>
    /// <param name="buff"></param>
    public void RegistBuff(BattleBuffHandler buffHandle, BuffBase buff){
        if (buffHandle == this ){
            //如果已经拥有同类型BUFF，直接刷新计时(按Buff_Type比较而非对象引用)
            BuffBase existKey = null;
            foreach (var k in BuffDic.Keys) {
                if (k.Buff_Type == buff.Buff_Type) { existKey = k; break; }
            }
            if (existKey != null) {
                BuffDic[existKey].ResetTimer();
                Debug.Log(string.Format("BUFF:{0}重复获取，刷新BUFF持续时间", buff.Buff_Type));
                return;
            }
            //新BUFF，注册
            BuffDic.Add(buff,new BuffTimer(buff.Buff_Dura));
            Debug.Log(string.Format("{0} get New BUFF:{1}-{2}", self.Camp, buff.Buff_Attr, buff.Buff_Type));

        }
    }

    public void UnRegistBuff(E_BuffType buffType) {
        foreach (var buff in BuffDic){
            if (buff.Key.Buff_Type == buffType) {
                Debug.Log($"{self.battleDamageHandler.name}移除了BUFF：{buffType}");
                BuffDic.Remove(buff.Key);
                return;
            }
        }
        Debug.Log($"{self.battleDamageHandler.name}未持有BUFF：{buffType}，移除失败");
    }
    public void OnBuffUpdate() {
        if (BuffDic.Count <= 0)
            return;
        //检测对应的buff是否结束
        var expired = new List<BuffBase>();
        foreach (var buffUni in BuffDic) {
            if (buffUni.Value.CheckBuffDone())
                expired.Add(buffUni.Key);
        }
        foreach (var key in expired){
            Debug.Log("移除BUFF"+key.Buff_Type);
            BuffDic.Remove(key);
        }
        //buff帧更新
        foreach (var buffUni in BuffDic){
            buffUni.Key.OnBuffUpdate();
        }
    }

    BuffBase TryGetBuff(E_BuffType buffType) {
        foreach (var buff in BuffDic){
            if (buff.Key.Buff_Type == buffType){
                return buff.Key;
            }
        }
        return null;
    }

    public float GetDamageRate() {
        var Buff = TryGetBuff(E_BuffType.战意_正面) as Buff_DamageBoomer;
        if (Buff == null) { return 0.0f; }
        else {Debug.Log(self.battleDamageHandler.name+"存在造成伤害增幅相关BUFF,倍率:"+Buff.BoomerRate);
        return Buff.BoomerRate; }
            
    }

    //GameRoot.GetManager<CoroutineManager>().StartCoroutine(WaitBuffUnRegiste(buff));
    /// <summary>
    /// 开始计时BUFF，倒计时结束，移除BUFF
    /// </summary>
    /// <param name="buff"></param>
    /// <returns></returns>
    IEnumerator WaitBuffUnRegiste(BuffBase buff){
        yield return new WaitForSeconds(buff.Buff_Dura);
        Debug.Log(string.Format("{0} lose BUFF:{1}-{2}", self.Camp, buff.Buff_Attr, buff.Buff_Type));
        BuffDic.Remove(buff);
    }

}

public class BuffTimer {
    float timer;
    float interval;
    bool start;
    public BuffTimer(float interval) {
        this.interval = interval;
        ResetTimer();
    }
    public void ResetTimer() { 
        timer=interval;
        start=true;
    }

    /// <summary>
    /// 返回BUFF是否结束
    /// </summary>
    /// <returns></returns>
    public bool CheckBuffDone() {
        if (!start) return false;   
        if (timer >= 0) 
            timer -= Time.deltaTime;
        else return true;
        return false;
    }

}

