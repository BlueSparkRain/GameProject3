using Core;
using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : UIPanelBase
{
    [Header("菜单按钮")]
    [SerializeField] private MenuButtonItem[] menuButtons;

    [Header("起止标记")]
    [SerializeField] private Transform startTrans;
    [SerializeField] private Transform endTrans;
    [Header("动画参数")]
    [SerializeField] private float entryDuration = 0.6f;
    [SerializeField] private Ease entryEase = Ease.OutBack;
    [SerializeField] private float staggerDelay = 0.4f;
    public Button newGameButton;
    public Button continueButton;
    public Button settingButton;

    void OnClickGameButton(){
        GameRoot.GetManager<UIManager>().HidePanel(E_UIPanelType.MenuPanel);
        JsonSaver.StartNewGame();
        DOVirtual.DelayedCall(2f, () => {
            GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("MapScene");
        }, true).SetUpdate(true);
    }
    void OnClickContinueButton(){
        GameRoot.GetManager<UIManager>().HidePanel(E_UIPanelType.MenuPanel);
        DOVirtual.DelayedCall(2f, () => {
            GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("MapScene");
        }, true).SetUpdate(true);
    }
    void OnClickSettingButton(){
        GameRoot.GetManager<UIManager>().OpenPanel<SettingsPanel>(E_UIPanelType.MenuPanel,null);
    }
    public override void Init(E_UIPanelType type, string uniqueID){
        base.Init(type, uniqueID);
        if (newGameButton != null) newGameButton.onClick.AddListener(OnClickGameButton);
        if (continueButton != null) continueButton.onClick.AddListener(OnClickContinueButton);
        if (settingButton != null) settingButton.onClick.AddListener(OnClickSettingButton);
    }
    protected override void PlayEnterAnim(System.Action onComplete){
        StartCoroutine(EnterSequence(onComplete));
    }
    protected override void PlayExitAnim(System.Action onComplete){
        StartCoroutine(ExitSequence(onComplete));
    }
    IEnumerator EnterSequence(System.Action onComplete)
    {
        panelRoot.anchoredPosition = Vector2.zero;
        yield return null; // 等一帧，让布局系统重建后再读位置

        float startX = startTrans.position.x;
        float endX = endTrans.position.x;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == null) continue;

            Transform t = menuButtons[i].transform;
            // 手动设置起始位置，避免 DOTween From() + SetUpdate(true) 偶发跳过首帧
            Vector3 pos = t.position;
            pos.x = startX;
            t.position = pos;

            t.DOMoveX(endX, entryDuration)
                .SetEase(entryEase)
                .SetUpdate(true);

            yield return new WaitForSecondsRealtime(staggerDelay);
        }

        yield return new WaitForSecondsRealtime(entryDuration);
        onComplete?.Invoke();
    }

    IEnumerator ExitSequence(System.Action onComplete)
    {
        float startX = startTrans.position.x;

        for (int i = 0; i < menuButtons.Length; i++)
        {
        //for (int i = menuButtons.Length - 1; i >= 0; i--)
        //{
            if (menuButtons[i] == null) continue;

            menuButtons[i].transform
                .DOMoveX(startX, entryDuration)
                .SetEase(entryEase)
                .SetUpdate(true);

            yield return new WaitForSecondsRealtime(staggerDelay);
        }

        yield return new WaitForSecondsRealtime(entryDuration);
        panelRoot.anchoredPosition = Anim_BornPos;
        onComplete?.Invoke();
    }
}
