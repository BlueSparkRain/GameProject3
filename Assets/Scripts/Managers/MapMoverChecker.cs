using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMoverChecker : MonoSceneManager
{
    string mapIconPrefabPath = "Prefab/MapUI/CharacterMapIcon";

    public Transform mapIconParent;

    public CharacterMapMover currentMover;

    public override void MgrUpdate(float deltaTime){}
        
    private Dictionary<CharacterMapIcon,CharacterMapMover>  mapMoverDic = new Dictionary<CharacterMapIcon, CharacterMapMover>();

    int iconNUm=0;
    public CharacterMapIcon CreateNewMapIcon(CharacterMapMover characterRoomMover) { 
        var newIcon=GameObject.Instantiate(Resources.Load<GameObject>(mapIconPrefabPath), mapIconParent).GetComponent<CharacterMapIcon>();
        mapMoverDic.Add(newIcon,characterRoomMover);
        newIcon.InitIcon(characterRoomMover.CharacterType);
        newIcon.GetComponent<RectTransform>().localPosition += new Vector3(200, 0, 0) * iconNUm;
        iconNUm++;
        return newIcon;
    }

    public CharacterMapMover GetTargetMover(CharacterMapIcon mapIcon) {
        if (mapMoverDic.ContainsKey(mapIcon)){
            currentMover=mapMoverDic[mapIcon];
            if (currentMover.IsMoving) {
                Debug.Log("[MapMoverChecker]---请求失败！目标Mover正在移动中");
                mapIcon.FlashWarnning();
                return null;
            }
            else
            return currentMover;
        }
        else{
            Debug.Log("[MapMoverChecker]---请求失败！目标Mover未注册");
            return null;
        }
    }
    public void SetCurrentMover(CharacterMapMover characterRoomMover) {
        currentMover=characterRoomMover;

    }
    public void MoverGo(List<HexRoomData> path) { 
        currentMover.MoveByPath(path);
    }
}
