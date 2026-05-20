using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NoneHexRoom :IHexRoom
{
    public void DoHexRoomInit()
    {

    }

    public void DoHexRoomLogic(UnityAction roomJob = null)
    {
        //Debug.Log("这是个空白房间");
    }

    public void DoHexRoomModel(Vector3 pos)
    {
    }
}
