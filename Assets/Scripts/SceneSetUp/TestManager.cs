using Core;
using Core.Interfaces;
using Core.Managers;
using UnityEngine;

public class TestManager :MonoSceneManager
{
    public override void MgrUpdate(float deltaTime)
    {
        //Debug.Log(Time.realtimeSinceStartup);
    }

    public void Test() {
        print("xuzijun-love-wangbeibei");
    }
}
