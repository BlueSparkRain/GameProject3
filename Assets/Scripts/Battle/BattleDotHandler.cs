using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责管理Dot的注册和更新
/// </summary>
public class BattleDotHandler : MonoBehaviour{
    IBattlable self;
    Dictionary<E_Dot, DotBase> DotDic = new Dictionary<E_Dot, DotBase>();
    bool _isTriggeringShock;

    public void InitBattleDotHandle(IBattlable _self){
        self = _self;
        EventCenter.AddEventListener<BattleDotHandler, DotBase,int>(E_EventType.Battle_RegisterDot, RegistDot);
        EventCenter.AddEventListener<BattleDamageHandler, float>(E_EventType.Get_Damage, OnSelfGetDamage);
    }

    void OnSelfGetDamage(BattleDamageHandler damageHandler, float damageValue) {
        if (_isTriggeringShock) return;
        if (damageHandler != self.battleDamageHandler) return;
        if (DotDic.TryGetValue(E_Dot.感电, out var shockDot)) {
            _isTriggeringShock = true;
            shockDot.OnDotTrigger();
            _isTriggeringShock = false;
        }
    }
    /// <summary>
    /// 给本单位注册一个新的BUFF（可叠加层数）
    /// 重复注册=层数+1
    /// </summary>
    void RegistDot(BattleDotHandler dotHandle, DotBase dot,int adjustCount){
        //只能对自身的Dot生效
        if (dotHandle == this ){
            if (!DotDic.ContainsKey(dot.Dot_type)){
                DotDic.Add(dot.Dot_type, dot);
                Debug.Log(string.Format("{0} 首次获得 Dot:{1},当前层数{2}", self.Camp, dot.Dot_type,dot.Dot_count));
            }
            else {
                //新增的层数直接加到已存在的Dot上
                Debug.Log(string.Format("{0} Dot:{1} 当前层数{2} 增加层数{3}", self.Camp, dot.Dot_type,dot.Dot_count,adjustCount));
                DotDic[dot.Dot_type].AdjustDotLevel(adjustCount);
                Debug.Log(string.Format("{0} Dot:{1} 当前层数{2} 层数已更新", self.Camp, dot.Dot_type,dot.Dot_count));
            }
        }
    }
    /// <summary>
    /// 从本单位移除一个BUFF
    /// </summary>
    public void UnRegistDot(BattleDotHandler dotHandle, E_Dot dot_type){
        if (dotHandle == this && DotDic.ContainsKey(dot_type)){
            Debug.Log(self.Camp + "移除了Dot：" + dot_type);
            DotDic.Remove(dot_type);
        }
    }

    public int GetDotLayers(E_Dot dotType) {
        if (DotDic.TryGetValue(dotType, out var dot))
            return dot.Dot_count;
        return 0;
    }

    public int ClearDotAndGetLayers(E_Dot dotType) {
        if (DotDic.TryGetValue(dotType, out var dot)) {
            int layers = dot.Dot_count;
            DotDic.Remove(dotType);
            Debug.Log($"{self.Camp}清除了Dot：{dotType}，层数{layers}");
            return layers;
        }
        return 0;
    }
    public void OnDotUpdate(){
        if (DotDic.Count <= 0) return;
        foreach (var dot in DotDic){
            dot.Value.OnDotUpdate();
        }
    }
}
