using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMapIcon : MonoBehaviour
{
    [Header("角色展示图像")]
    public Image characterImage;
    [Header("角色选择按钮")]
    public Button characterSelectButton;
    [Header("角色昵称")]
    public TMP_Text characterNameText;
    E_CharacterType characterType;
    //用于读取精灵或名称信息
    CharacterDataSO characterSOData;
    //剩余行动点
    int remainActionPoints;
    bool canMove;
    Transform charcaterTrans;

    //private CharacterLevelUpHandler levelUpHandler;
    [Header("经验Box")]
    public PlayerLevelBox  levelBox;   
    private void Awake(){
        EventCenter.AddEventListener(E_EventType.Mover_PlayerStartMove, MoverStartMove);
    }
    private void OnDestroy(){
        EventCenter.RemoveEventListener(E_EventType.Mover_PlayerStartMove, MoverStartMove);
    }

    public void InitIcon(E_CharacterType _characterType, Transform _charcaterTrans){
        characterSelectButton.onClick.AddListener(OnClickIconButton);
        characterType = _characterType;
        charcaterTrans = _charcaterTrans;

        // 玩家操控角色需要更新UI数据
        var levelUpHandler = charcaterTrans.GetComponent<CharacterLevelUpHandler>();
        // EXP UI更新已通过EventCenter(E_EventType.AdjustEXP)解耦，无需手动订阅
        //levelUpHandler.InitLevelHandler();
        
        characterSOData = ResourcesLoader.FindCharaterSO(_characterType); 
        characterImage.sprite = characterSOData.characterSprite;
        characterNameText.text = characterSOData.characterName;
    }

    /// <summary>
    /// 设置本回合行动点
    /// </summary>
    /// <param name="value"></param>
    public void SetMoveDot(int value){
        remainActionPoints = value;
        canMove = (remainActionPoints > 0 ? true : false);
    }

    void MoverStartMove() { 
        isActive = false;
        DebugManager.Log(EDebugCategory.MapRoom, $"[PlayerMapIcon] MoverStartMove — {characterSelectButton.gameObject.name}");
        if(characterSelectButton.gameObject)
        characterSelectButton.GetComponent<Image>().color = Color.white;
    }

    public void FlashWarnning(){
        //播放无法交互音效
        //闪红
        Image buttonImage = characterSelectButton.GetComponent<Image>();
        buttonImage.DOColor(Color.red, 0.1f)
        .SetLoops(2, LoopType.Yoyo) 
        .SetEase(Ease.InOutFlash)       
        .OnComplete(() => {          // 动画结束强制还原（双重保险）
            buttonImage.color = Color.white;
        });
    }
    void SetCameraToCharacter(){
        //相机将角色居中
    }
    bool isActive = false;
    void OnClickIconButton(){
        if (!canMove){
            FlashWarnning();
            return;
        }

        var mover = GameRoot.GetManager<MapMoverManager>().GetTargetPlayerMover(this);
        if (mover == null) return;

        bool enteringPathFind = !GameRoot.GetManager<HexPathFindingManager>()?.canPathFind ?? false;
        if (enteringPathFind)
            GameRoot.GetManager<OrthoCameraNavigator>().FocusOnTarget(charcaterTrans.gameObject);
        GameRoot.GetManager<MapMoverManager>().TogglePathFinding();
    }

    public void SetHighlighted(bool active)
    {
        isActive = active;
        if (characterSelectButton != null)
            characterSelectButton.GetComponent<Image>().color = active ? Color.cyan : Color.white;
    }
}
