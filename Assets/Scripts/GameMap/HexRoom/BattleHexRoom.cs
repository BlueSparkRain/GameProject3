using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleHexRoom : HexRoom
{
    public override void DoRoomJob(UnityAction roomJob)
    {
        base.DoRoomJob(roomJob);
        Debug.Log("点击到-战斗房间");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
