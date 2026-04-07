using System.Collections;
using System.Threading.Tasks;
using Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Image动画测试脚本（无报错终极版）
/// 核心：移除OnKill回调，修复空引用错误，支持打断无突变
/// </summary>
public class UIImageAnimTest : MonoBehaviour
{
    void Update(){

        if (Input.GetKeyDown(KeyCode.P))
        {
            GameRoot.GetManager<UIManager>().OpenPanel<TestPanel>(E_UIPanelType.TestTPanel);
        }
      
    }
}