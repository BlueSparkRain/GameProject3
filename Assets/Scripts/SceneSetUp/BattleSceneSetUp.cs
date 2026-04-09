using UnityEngine;
using Core;
public class BattleSceneSetUp : MonoBehaviour
{
    GameRoot gameRoot;
    private void Awake()
    {
        BattleSkillFactory.RegisterAllSkills();

        gameRoot = GameRoot.Instance;
        //gameRoot.RegisterScene_MonoManager<BattleTargetsSelectManager>();
        //EventCenter.EventTrigger(E_EventType.LoadObjPool, EPoolType.SkillIcon_技能图标);
        //StartCoroutine(  GameRoot.GetManager<ObjectPoolManager>().StartFillPool(EPoolType.SkillIcon));
    }

}
