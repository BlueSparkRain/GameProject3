using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 负责根据BattleManager中的战斗数据 产生战斗对象。
/// 敌人位置动态均分在 x 范围（-650~650），新敌人加入时已有敌人平滑调整位置。
/// </summary>
public class BattleLoadManager : MonoSceneManager{
    public Transform PlayerAreaContent;
    public Transform EnemyAreaContent;

    public Transform PlayerAreaStartPivot;
    public Transform EnemyAreaStartPivot;

    [Header("敌人排列")]
    public float enemyMinX = -650f;
    public float enemyMaxX = 650f;
    public float positionAnimDuration = 0.4f;
    public Ease positionEase = Ease.OutCubic;

    List<Transform> _enemyTransforms = new List<Transform>();

    public void LoadAEnemy(CharacterData data){
        var battlerObj = Instantiate(ResourcesLoader.FindBattleCharacterObj(), Vector3.zero, Quaternion.identity, EnemyAreaContent);

        // 出场动画：从上方飞入
        Vector3 bornPos = EnemyAreaStartPivot.position + new Vector3(0, 800, 0);
        battlerObj.transform.position = bornPos;
        battlerObj.GetComponent<BattleHandler>().InitBattler(data);

        _enemyTransforms.Add(battlerObj.transform);
        RepositionAllEnemies(animate: true);
    }

    void RepositionAllEnemies(bool animate)
    {
        int count = _enemyTransforms.Count;
        // 清理已销毁的
        _enemyTransforms.RemoveAll(t => t == null);
        count = _enemyTransforms.Count;

        for (int i = 0; i < count; i++)
        {
            float x = count == 1 ? 0f : Mathf.Lerp(enemyMinX, enemyMaxX, (float)i / (count - 1));
            Vector3 targetPos = EnemyAreaStartPivot.position + Vector3.right * x;

            if (animate)
                _enemyTransforms[i].DOMove(targetPos, positionAnimDuration).SetEase(positionEase);
            else
                _enemyTransforms[i].position = targetPos;
        }
    }

    /// <summary>敌人死亡/移除时调用，重新均分剩余敌人位置</summary>
    public void RemoveEnemy(Transform enemyTransform)
    {
        _enemyTransforms.Remove(enemyTransform);
        RepositionAllEnemies(animate: true);
    }

    public override void MgrUpdate(float deltaTime){
    }
}
