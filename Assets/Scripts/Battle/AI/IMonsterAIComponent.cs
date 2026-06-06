/// <summary>
/// 怪物个性AI组件接口。
/// 每种怪物类型可实现此接口来定义独特的战斗行为触发条件与响应。
/// </summary>
public interface IMonsterAIComponent
{
    void OnBattleStart(Battle_Controller controller, BattleSkiller skiller);
    void OnBattleUpdate(Battle_Controller controller, BattleSkiller skiller);
    void OnHPChanged(float currentHP, float maxHP, Battle_Controller controller, BattleSkiller skiller);
}
