using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BattleMVCHandler),
    typeof(BattleDamageHandler),
    typeof(BattleSkillHandler))]
[RequireComponent(typeof(BattleArtEffectHandller),
    typeof(BattleDotHandler),
    typeof(BattleBuffHandler))]
[RequireComponent(typeof(BattleWeaknessHandler))]
/// </summary>
public class BattleHandler : MonoBehaviour
{
    IBattlable self;
    public BattleMVCHandler MVCHandler;
    BattleSkillHandler skillHandler;
    BattleBuffHandler buffHandler;
    BattleDotHandler  dotHandler;
    BattleDamageHandler damageHandler;
    BattlerStateTag battlerStateTag;
    BattleArtEffectHandller  artEffectHandller;
    BattleWeaknessHandler weaknessHandler;

    IMonsterAIComponent monsterAI;
    CharacterData characterDataRef;

    bool start=false;
    public void InitBattler(CharacterData characterData){
        bool isplayer = (characterData.characterType == E_CharacterType.P_海螺骑士);

        self = isplayer ? new Player(GetComponent<BattleDamageHandler>()) : new Enemy(GetComponent<BattleDamageHandler>());
        //注册战斗单位
        BattleTargetSelector.RegisteNewBattler(self);
        battlerStateTag = new BattlerStateTag { CharacterType = characterData.characterType };

        // 预先加载弱点配置(含护盾初始值，供MVC初始化使用)
        var weaknessConfig = ResourcesLoader.FindWeaknessConfig(characterData.characterType);

        //初始化MVCHandler
        MVCHandler = GetComponentInChildren<BattleMVCHandler>();
        MVCHandler.InitMVCHandler(isplayer, characterData, battlerStateTag,
            weaknessConfig?.initialShieldPoints ?? 5);

        //初始化BuffHandler
        buffHandler=GetComponent<BattleBuffHandler>();
        buffHandler.InitBattleBuffHandle(self);

        //初始化DotHandler
        dotHandler=GetComponentInChildren<BattleDotHandler>();
        dotHandler.InitBattleDotHandle(self);

        //初始化WeaknessHandler
        weaknessHandler = GetComponentInChildren<BattleWeaknessHandler>();
        weaknessHandler.InitWeaknessHandle(self, weaknessConfig);

        //初始化BattleDamageHandler
        damageHandler= GetComponentInChildren<BattleDamageHandler>();
        damageHandler.InitDataHandler(MVCHandler,buffHandler,dotHandler,weaknessHandler);

        //初始化ArtEffectHandler
        artEffectHandller = GetComponentInChildren<BattleArtEffectHandller>();
        artEffectHandller.InitArtEffectHandler(damageHandler);

        //初始化BattleSkiller
        skillHandler = GetComponentInChildren<BattleSkillHandler>();
        skillHandler.InitBattleSkillHandler(self, MVCHandler,battlerStateTag);

        characterDataRef = characterData;
        InitMonsterAI(characterData);

        start=true;
        Debug.Log(characterData.Character_Name + "---进入战斗");
    }

    void Update(){
        if (start) {
            skillHandler.OnSkillerUpdate();
            buffHandler.OnBuffUpdate();
            dotHandler.OnDotUpdate();
            MVCHandler.OnMVCHandlerUpdate();
            monsterAI?.OnBattleUpdate(MVCHandler.BattleController, skillHandler.GetSkiller());
        }
    }

    void InitMonsterAI(CharacterData characterData)
    {
        var skiller = skillHandler.GetSkiller();

        // 加载自动技能配置(由 normalSkillIconSpawner 对所有角色统一直接调用)
        var autoSkillConfig = ResourcesLoader.FindAutoSkillConfig(characterData.characterType);
        if (autoSkillConfig != null)
            skiller.LoadAutoSkillsFromConfig(autoSkillConfig, characterData.AutoSkillSlotCount);

        // 加载ATB意图技能配置(由 atbSkillIconSpawner)
        var atbConfig = ResourcesLoader.FindATBIntentionConfig(characterData.characterType);
        if (atbConfig != null)
        {
            skiller.LoadActiveSkillsFromConfig(atbConfig, characterData.AtbSkillSlotCount);
            skiller.InitATBIntention(MVCHandler.BattleController, atbConfig);
        }

        monsterAI = MonsterAIFactory.Create(characterData.characterType);
        if (monsterAI != null)
        {
            monsterAI.OnBattleStart(MVCHandler.BattleController, skillHandler.GetSkiller());

            var model = MVCHandler.BattleController.Model;
            model.OnHPChanged += (currentHP, maxHP) =>
            {
                monsterAI.OnHPChanged(currentHP, maxHP,
                    MVCHandler.BattleController, skillHandler.GetSkiller());
            };
        }
    }
}
