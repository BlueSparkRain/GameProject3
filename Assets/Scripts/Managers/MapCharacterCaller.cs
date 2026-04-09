using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
public static class MapCharacterCaller 
{

    static GameMapManager mapManager;
    /// <summary>
    /// (通过读取存档或场景初始配置)生成一个目标类型的角色
    /// </summary>
    /// <param name="characterType"></param>
    /// <param name="backStr"></param>
    public static CharacterData CallNewCharacter(string backStr) {
        var target = GameObject.Instantiate(ResourcesLoader.FindCharacterObj(backStr));
        return target.GetComponent<CharacterData>();
    }

    public static  void GetRandomPos() {
        mapManager = GameRoot.GetManager<GameMapManager>();

    }


    //同时记录本局游戏场上已经召唤的所有角色（死亡位置也会记录）
    //根据存档来读，如果有记录，按照此前的位置来加载

}

