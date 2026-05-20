using UnityEngine;
using Core;
using System.Collections;
public class BattleSceneSetUp : MonoBehaviour
{
    GameRoot gameRoot;
    private void Awake(){
        gameRoot=GameRoot.Instance;
        gameRoot.RegisterScene_MonoManager<BattleTargetsSelectManager>();
        gameRoot.RegisterScene_MonoManager<BattleLoadManager>();
    }
    private void Start(){
        StartCoroutine(LoadGame());
    }
    
    IEnumerator LoadGame() { 
        //使用Battlemanager进行战斗场景的加载
        yield return  new WaitForSeconds(2);
        GameRoot.GetManager<GameBattleManager>().SpawnBattleCharacter();
    }
}
