using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class BattleBuffHandler : MonoBehaviour
{
    public List<BuffBase> BuffList = new List<BuffBase>();
    IBattlable self;
    public void InitBattleBufferHandle(IBattlable _self) {
     self= _self;
        EventCenter.AddEventListener<BattleBuffHandler>(E_EventType.Do_PhyAttack, Check_Phy_AdditiveBuff);
        EventCenter.AddEventListener<BattleBuffHandler, BuffBase>(E_EventType.Battle_RegisteBUFF, RegisteBuff);
    }

    void Check_Phy_AdditiveBuff(BattleBuffHandler buffHandler){
        if (BuffList.Count <= 0){
            Debug.Log(self.Camp + "no buff");
            return;
        }
        if (buffHandler == this) { 
            foreach (var item in BuffList){
                if (item.Buff_Name == E_BuffName.³ãÑæÁ¬Ëø)
                    item.BuffTrigger();
            }
        }
        Debug.Log(self.Camp + "no xxxx buff");
    }
    public void RegisteBuff(BattleBuffHandler buffHandle, BuffBase buff){
        Debug.Log(buffHandle + "check buffHandle");
        if (buffHandle == this){
            BuffList.Add(buff);
            Debug.Log(string.Format("{0} get BUFF:{1}-{2}", self.Camp, buff.Buff_Attr, buff.Buff_Name));
            GameRoot.GetManager<CoroutineManager>().StartCoroutine(WaitBuffUnRegiste(buff));
        }
    }
    IEnumerator WaitBuffUnRegiste(BuffBase buff){
        yield return new WaitForSeconds(buff.Buff_Dura);
        Debug.Log(string.Format("{0} lose BUFF:{1}-{2}", self.Camp, buff.Buff_Attr, buff.Buff_Name));
        BuffList.Remove(buff);
    }

    public void OnBuffUpdate() {
        if (BuffList.Count <= 0)
            return;
        foreach (var buff in BuffList){
            buff.BuffUpdate();
        }
    }
}
