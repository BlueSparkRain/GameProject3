using UnityEngine;

public class Dot_Freeze : DotBase{

    float timer;
    //每0.5s检测一次
    float interval = 0.5f;
    public Dot_Freeze(E_Dot _dotType, IBattlable _self,int _dotCount) : base(_dotType, _self,_dotCount){
        timer = interval;
    }

    /// <summary>
    /// <summary>
    /// 直接斩杀血量低于 层数*目标最大生命值%2 的单位
    /// </summary>
    public override void OnDotTrigger()
    {
        base.OnDotTrigger();
        Debug.Log($"{self.Camp}的{dot_type} Dot触发,结算斩杀线[最大生命值%2*Dot层数]：{(int)(self.battleDamageHandler.GetMaxHealth() * 0.02f * dot_count)}");
        self.battleDamageHandler.GetDamage(E_Skill_DamageType.魔法, self.battleDamageHandler.GetMaxHealth());
    }
    /// <summary>
    /// 检测角色是否达到斩杀线
    /// </summary>
    /// <returns></returns>
    bool CheckKill(){return (int)(self.battleDamageHandler.GetCurrentHealth()) <= (int)(self.battleDamageHandler.GetMaxHealth() * 0.02f * dot_count);}
    public override void OnDotUpdate(){
        base.OnDotUpdate();
        if (timer > 0) {
            timer -= Time.deltaTime;
        }
        else { 
            timer=interval;
            if(CheckKill()) {
                OnDotTrigger();
            }
        }
    }

}
