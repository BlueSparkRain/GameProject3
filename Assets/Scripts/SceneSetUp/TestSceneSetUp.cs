using Core;
using System.Collections;
using UnityEngine;
public class TestSceneSetUp : MonoBehaviour
{
    GameRoot gameRoot;
    void Start()
    {
        gameRoot = GameRoot.Instance;
        gameRoot.RegisterScene_MonoManager<TestManager>();
       
        //gameRoot.RegisterGlobal_MonoManager<TestCSManager>();
        gameRoot.RegisterGlobal_CSManager(new TestCSManager());

        gameRoot.RegisterScene_MonoManager<OrthoCameraNavigator>();
    }
    IEnumerator Wait() {
        yield return new WaitForSeconds(1);
        Debug.Log("异步切换场景");
        GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("UI_Scene");

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(Wait());
            
            //gameRoot.GetManager<TestManager>().Test();
            GameRoot.GetManager<TestCSManager>().GGG();
        }
    }
}
