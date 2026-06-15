using Core;
using Core.Interfaces;
using System.Diagnostics;
public class TestCSManager : IGlobalManager
{
    public void MgrDispose()
    {
        DebugManager.Log(EDebugCategory.General, "TestCSManager-Dispose");
    }

    public void MgrInit(GameRoot gameRoot)
    {
        DebugManager.Log(EDebugCategory.General, "TestCSManager-Init");

    }

    public void MgrUpdate(float deltatime)
    {

    }
    public void GGG() {
        DebugManager.Log(EDebugCategory.General, "wox-GGG");
    }
}
