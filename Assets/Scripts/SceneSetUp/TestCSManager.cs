using Core;
using Core.Interfaces;
using System.Diagnostics;
public class TestCSManager : IGlobalManager
{
    public void MgrDispose()
    {
        UnityEngine.Debug.Log("TestCSManager-Dispose");
    }

    public void MgrInit(GameRoot gameRoot)
    {
        UnityEngine.Debug.Log("TestCSManager-Init");

    }

    public void MgrUpdate(float deltatime)
    {

    }
    public void GGG() {
        UnityEngine.Debug.Log("wox-GGG");
    }
}
