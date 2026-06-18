using Core;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BattleMVCHandler),
    typeof(BattleDamageHandler),
    typeof(BattleSkillHandler))]
[RequireComponent(typeof(BattleArtEffectHandller),
    typeof(BattleDotHandler),
    typeof(BattleBuffHandler))]
[RequireComponent(typeof(BattleWeaknessHandler))]
public class BattleHandler : MonoBehaviour{
    IBattlable self;
    public IBattlable Self => self;
    public BattleMVCHandler MVCHandler;
    BattleSkillHandler skillHandler;
    public BattleBuffHandler buffHandler;
    BattleDotHandler  dotHandler;
    BattleDamageHandler damageHandler;
    public BattlerStateTag battlerStateTag;
    BattleArtEffectHandller  artEffectHandller;
    BattleWeaknessHandler weaknessHandler;

    IMonsterAIComponent monsterAI;
    CharacterData characterDataRef;

    public TMP_Text NameText;
    bool start=false;
    public void InitBattler(CharacterData characterData){
        bool isplayer = (characterData.characterType == E_CharacterType.P_海螺骑士);

        var damageHandler = GetComponentInChildren<BattleDamageHandler>();
        self = isplayer ? new Player(damageHandler) : new Enemy(damageHandler);

        NameText.text = characterData.Character_Name;

        //注册战斗单位
        BattleTargetSelector.RegisteNewBattler(self);
        battlerStateTag = new BattlerStateTag { CharacterType = characterData.characterType };
        // 预先加载弱点配置(含护盾初始值，供MVC初始化使用)
        var weaknessConfig = ResourcesLoader.FindWeaknessConfig(characterData.characterType);

        //初始化MVCHandler
        MVCHandler = GetComponentInChildren<BattleMVCHandler>();
        MVCHandler.InitMVCHandler(isplayer, characterData, battlerStateTag, self,
        weaknessConfig?.initialShieldPoints ?? 5);
        //初始化BuffHandler
        buffHandler = GetComponentInChildren<BattleBuffHandler>();
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
        DebugManager.Log(EDebugCategory.BattleSkiller,characterData.Character_Name + "---进入战斗");
    }

    void Update(){
        if (!start) return;

        skillHandler.OnSkillerUpdate();
        buffHandler.OnBuffUpdate();
        dotHandler.OnDotUpdate();
        MVCHandler.OnMVCHandlerUpdate();

        var phaseMgr = GameRoot.GetManager<BattlePhaseManager>();
        if (phaseMgr == null || phaseMgr.CurrentPhase == BattlePhase.InProgress)
            monsterAI?.OnBattleUpdate(MVCHandler.BattleController, skillHandler.GetSkiller());
    }

    void InitMonsterAI(CharacterData characterData)
    {
        var skiller = skillHandler.GetSkiller();
        bool usedMapConfig = false;
        bool isPlayer = characterData.characterType == E_CharacterType.P_海螺骑士;

        // 优先使用地图配置的技能ID（可移动角色 → SkillAssignPanel），否则：
        // - 玩家：不加载技能（空列表 = 玩家选择不配置）
        // - 非玩家角色：回退到SO配置
        if (characterData.mapNormalSkillIDs != null && characterData.mapNormalSkillIDs.Count > 0)
        {
            skiller.LoadAutoSkillsFromIDs(characterData.mapNormalSkillIDs, characterData.AutoSkillSlotCount);
            usedMapConfig = true;
        }
        else if (!isPlayer)
        {
            var autoSkillConfig = ResourcesLoader.FindAutoSkillConfig(characterData.characterType);
            if (autoSkillConfig != null)
                skiller.LoadAutoSkillsFromConfig(autoSkillConfig, characterData.AutoSkillSlotCount);
        }

        if (characterData.mapATBSkillIDs != null && characterData.mapATBSkillIDs.Count > 0)
        {
            skiller.LoadActiveSkillsFromIDs(characterData.mapATBSkillIDs, characterData.AtbSkillSlotCount);
            if (!isPlayer)
                skiller.SetATBExecutor(new ATBIntentionExecutor(
                    self, MVCHandler.BattleController, characterData.mapATBSkillIDs));
            usedMapConfig = true;
        }
        else if (!isPlayer)
        {
            var atbConfig = ResourcesLoader.FindATBIntentionConfig(characterData.characterType);
            if (atbConfig != null)
            {
                skiller.LoadActiveSkillsFromConfig(atbConfig, characterData.AtbSkillSlotCount);
                if (!isPlayer)
                    skiller.InitATBIntention(MVCHandler.BattleController, atbConfig);
            }
        }

        if (usedMapConfig)
            DebugManager.Log(EDebugCategory.BattleSkiller,$"[BattleHandler] {characterData.Character_Name} 使用地图配置技能 (Auto:{characterData.mapNormalSkillIDs?.Count ?? 0}, ATB:{characterData.mapATBSkillIDs?.Count ?? 0})");

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
