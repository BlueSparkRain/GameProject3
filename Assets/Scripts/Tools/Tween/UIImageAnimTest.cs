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
    //public Image image;
    //public float animDuration = 0.5f;
    //public Ease easeType = Ease.OutBack;
    //public Vector3 bornPos =  new Vector3(0,-1000,0); 
    //public Vector3 targetTrans = new Vector3(0, 1000, 0); 

    bool _doFadeIn = false;

    void Start(){
        //// 初始化Image（仅改Y轴，保留X/Z）
        //if (image != null){ 
        //    image.raycastTarget = false;
        //    MagicAnimExtens.ResetRecTransPos(image.rectTransform, bornPos);
        //}
    }

    void Update(){
        //if (Input.GetKeyDown(KeyCode.Alpha1) && image != null){
        //    _doFadeIn = !_doFadeIn;
        //    MagicAnimExtens.DoLocal_UIAnim(
        //        image.rectTransform, 
        //        animDuration,
        //        easeType, 
        //        bornPos, targetTrans,_doFadeIn,true);
        //}

        if (Input.GetKeyDown(KeyCode.Space)) {
            GameRoot.GetManager<UIManager>().OpenPanel<TestPanel>(UIPanelType.TestTPanel);   
        }
    }
}