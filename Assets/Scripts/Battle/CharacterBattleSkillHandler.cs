using UnityEngine;

public class CharacterBattleSkillHandler : MonoBehaviour
{
    public SkillIconSpawner normalSkillIconSpawner;
    public SkillIconSpawner atbSkillIconSpawner;
    CharacterBattleSkiller battleSkiller;
    public CharacterBattleSkiller BattleSkiller => battleSkiller;
    private void Start()
    {
        battleSkiller = new CharacterBattleSkiller(normalSkillIconSpawner,atbSkillIconSpawner);
    }
    private void Update()
    {
        battleSkiller.OnSkillUpdate();
    }

}
