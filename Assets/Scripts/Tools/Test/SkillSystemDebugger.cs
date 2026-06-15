using UnityEngine;

/// <summary>
/// 技能系统调试脚本——挂载到场景任意GameObject
/// Space: 随机新增技能 | I: 解锁ATB槽
/// </summary>
public class SkillSystemDebugger : MonoBehaviour
{
    CharacterMapSkiller PlayerSkiller
    {
        get
        {
            var checker = Core.GameRoot.GetManager<MapSkillerCheker>();
            return checker?.PlayerSkiller;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var sk = PlayerSkiller;
            if (sk != null)
                sk.GetNewSkill(Random.Range(0, 59));
            else
                DebugManager.LogWarning(EDebugCategory.General, "[SkillSystemDebugger] 玩家CharacterMapSkiller未就绪");
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            var sk = PlayerSkiller;
            if (sk == null) return;
            var data = sk.GetComponent<CharacterHandler>()?.CharacterData;
            if (data != null)
            {
                data.UnlockAtbSlot(1);
                DebugManager.Log(EDebugCategory.General, $"[SkillSystemDebugger] ATB槽数已解锁至 {data.AtbSkillSlotCount}/{CharacterData.maxAtbSkillSlotCount}");
            }
        }
    }
}
