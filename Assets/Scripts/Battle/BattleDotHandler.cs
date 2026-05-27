using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责各种Dot的结算和更新
/// </summary>
public class BattleDotHandler : MonoBehaviour{
    IBattlable self;
    Dictionary<E_Dot, DotBase> DotDic = new Dictionary<E_Dot, DotBase>();
    public void InitBattleDotHandle(IBattlable _self){
        self = _self;
        EventCenter.AddEventListener<BattleDotHandler, DotBase,int>(E_EventType.Battle_RegisterDot, RegistDot);
    }
    /// <summary>
    /// 本单位获得一种新的BUFF（不会随时间消失）
    /// 重复获得=层数加1
    /// </summary>
    /// <param name="dotHandle"></param>
    /// <param name="dot"></param>
    void RegistDot(BattleDotHandler dotHandle, DotBase dot,int adjustCount){
        //只能获得没有的Dot
        if (dotHandle == this ){
            if (!DotDic.ContainsKey(dot.Dot_type)){
                DotDic.Add(dot.Dot_type, dot);
                Debug.Log(string.Format("{0} 首次获得 Dot:{1},当前层数{2}", self.Camp, dot.Dot_type,dot.Dot_count));
            }
            else {
                //将新增的层数直接加到已有的Dot上
                Debug.Log(string.Format("{0} Dot:{1} 当前层数{2} 新增层数{3}", self.Camp, dot.Dot_type,dot.Dot_count,adjustCount));
                DotDic[dot.Dot_type].AdjustDotLevel(adjustCount);
                Debug.Log(string.Format("{0} Dot:{1} 当前层数{2} 层数已更新", self.Camp, dot.Dot_type,dot.Dot_count));
            }
        }
    }
    /// <summary>
    /// 本单位移除一种BUFF
    /// </summary>
    /// <param name="dotHandle"></param>
    /// <param name="dot_type"></param>
    public void UnRegistDot(BattleDotHandler dotHandle, E_Dot dot_type){
        if (dotHandle == this && DotDic.ContainsKey(dot_type)){
            Debug.Log(self.Camp + "已移除Dot：" + dot_type);
            DotDic.Remove(dot_type);
        }
    }
    public void OnDotUpdate(){
        if (DotDic.Count <= 0) return;
        foreach (var dot in DotDic){
            dot.Value.OnDotUpdate();
        }
    }
}
