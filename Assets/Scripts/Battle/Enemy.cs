using System.Collections.Generic;

public class Enemy : IBattleUnit
{
    public E_Camp Camp => E_Camp.敌方;
    public bool IsAlive => true;
    // 技能列表
    //public List<ISkill> Skills { get; set; }

    //public void TakeDamage(int damage) { }
}