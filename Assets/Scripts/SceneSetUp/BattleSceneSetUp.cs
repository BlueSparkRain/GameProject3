using UnityEngine;
using Core;
public class BattleSceneSetUp : MonoBehaviour
{
    private void Awake()
    {
        GameRoot.Instance.RegisterScene_MonoManager<BattleTargetsSelectManager>();
        EventCenter.EventTrigger(E_EventType.LoadObjPool, EPoolType.SkillIcon_技能图标);
        //StartCoroutine(  GameRoot.GetManager<ObjectPoolManager>().StartFillPool(EPoolType.SkillIcon));
    }

}
