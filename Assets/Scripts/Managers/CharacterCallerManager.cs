using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
public class MapCharacterCallerManager :MonoSceneManager
{   
    /// <summary>
    /// (通过读取存档或场景初始配置)生成一个目标类型的角色
    /// </summary>
    /// <param name="characterType"></param>
    /// <param name="backStr"></param>
    public CharacterData CallNewCharacter(string backStr) {
        var target = GameObject.Instantiate(ResourcesLoader.FindCharacterObj(backStr));
        return target.GetComponent<CharacterData>();
    }

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        mapManager = GameRoot.GetManager<GameMapManager>();
    }

    GameMapManager  mapManager;

    //同时记录本局游戏场上已经召唤的所有角色（死亡位置也会记录）
    //根据存档来读，如果有记录，按照此前的位置来加载

    

    IEnumerator CreateMap()
    {
        yield return new WaitForSeconds(3);
        //制造玩家
        HexRoomData randonoom = mapManager.GetRnadomRoom();

        //if (PlayerCharacter != null)
        //{
        //    PlayerCharacter.transform.position = randonoom.transform.position + Vector3.up * 0.55f;
        //    //把玩家放到一个特殊的位置,然后原地走一格
        //    GameRoot.GetManager<OrthoCameraNavigator>().FocusOnTarget(PlayerCharacter.gameObject);
        //    PlayerCharacter.ZeroMove();
        //    yield return new WaitForSeconds(1.2f);
        //    PlayerCharacter.transform.DOScale(1.5f, 0.3f).SetEase(Ease.InQuart).From(0);
        //    yield return new WaitForSeconds(0.3f);
        //    PlayerCharacter.transform.DOScale(1, 0.2f).SetEase(Ease.OutQuart);
        //}
    }

    public override void MgrUpdate(float deltaTime)
    { 

    }

}

