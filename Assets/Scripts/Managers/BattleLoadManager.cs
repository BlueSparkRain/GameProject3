using DG.Tweening;
using UnityEngine;


/// <summary>
/// 负责根据BattleManager中的战斗数据 产生战斗对象
/// </summary>
public class BattleLoadManager : MonoSceneManager
{
    public Transform PlayerAreaContent;
    public Transform EnemyAreaContent;

    public Transform PlayerAreaStartPivot;
    public Transform EnemyAreaStartPivot;
    int playerIndex = 0;
    int enemyIndex = 0;
    float offset = 700;

    /// <summary>
    /// 战斗过程中可能有新角色加入战斗
    /// 外部接口：由BattleManager调用
    /// </summary>
    public void LoadAPlayer(CharacterData data)
    {
        var battlerObj = Instantiate(ResourcesLoader.FindBattleCharacterObj(),Vector3.zero,Quaternion.identity, PlayerAreaContent);
        //battlerObj.transform.SetParent(PlayerAreaContent);
        //一段动画，表示卡牌弹出
        Vector3 targetPos = PlayerAreaStartPivot.position + Vector3.right * (playerIndex++) * offset;
        battlerObj.transform.DOMove(targetPos, 0.5f).SetEase(Ease.OutBounce).From(targetPos+new Vector3(0, -800, 0));
        //卡牌出场完毕后，统一开始战斗
        battlerObj.GetComponent<BattleHandler>().InitBattler(data);
    }
    public void LoadAEnemy(CharacterData data)
    {
        var battlerObj = Instantiate(ResourcesLoader.FindBattleCharacterObj(), Vector3.zero, Quaternion.identity, EnemyAreaContent);

        Vector3 targetPos = EnemyAreaStartPivot.position + Vector3.right * (enemyIndex++) * offset;
        battlerObj.transform.DOMove(targetPos, 0.5f).SetEase(Ease.OutBounce).From(targetPos + new Vector3(0, 800, 0));
        //battlerObj.transform.position = EnemyAreaStartPivot.position + Vector3.right * (enemyIndex++) * offset;
        battlerObj.GetComponent<BattleHandler>().InitBattler(data);

    }

   

    public override void MgrUpdate(float deltaTime)
    {
    }
}
