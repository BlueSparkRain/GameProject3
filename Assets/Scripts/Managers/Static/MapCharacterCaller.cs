using Core;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
public static class MapCharacterCaller 
{

    static GameMapManager mapManager;
    /// <summary>
    /// (通过读取存档或场景初始配置)生成一个目标类型的角色
    /// </summary>
    /// <param name="characterType"></param>
    /// <param name="backStr"></param>
    public static CharacterDataTag CallNewCharacter(string backStr) {
        var target = GameObject.Instantiate(ResourcesLoader.FindCharacterObj(backStr));
        return target.GetComponent<CharacterDataTag>();
    }

    public static void SetMapPos(Transform characterTrans) {
        mapManager = GameRoot.GetManager<GameMapManager>();
        //地图还没有加载，还没来得及注册
        HexRoomData randonoom = mapManager.GetRnadomRoom();

        characterTrans.position = randonoom.transform.position + Vector3.up * 1.2f;
        characterTrans.localScale = Vector3.zero;

        characterTrans.DOScale(1.5f, 0.3f).SetEase(Ease.InQuart).From(0);
        characterTrans.DOScale(1, 0.2f).SetEase(Ease.OutQuart);
    }

    public static  void GetRandomPos() {
        mapManager = GameRoot.GetManager<GameMapManager>();

    }


    //同时记录本局游戏场上已经召唤的所有角色（死亡位置也会记录）
    //根据存档来读，如果有记录，按照此前的位置来加载

}

