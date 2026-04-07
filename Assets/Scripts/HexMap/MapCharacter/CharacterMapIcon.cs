using Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMapIcon : MonoBehaviour
{
    [Header("角色展示图像")]
    public Image characterImage;
    [Header("角色选择按钮")]
    public Button characterSelectButton;
    [Header("角色昵称")]
    public string characterName;

    E_CharacterType characterType;

    //用于读取精灵或名称信息
    CharacterDataSO characterSOData;

    //剩余行动点
    int remainActionPoints;
    bool canMove;

    string characterSO_ParentPath = "SOData/CharacterSOData/";


    private void Awake()
    {
        EventCenter.AddEventListener(E_EventType.Mover_StartMove, MoverStartMove);
    }

    private void OnDestroy()
    {
        EventCenter.RemoveEventListener(E_EventType.Mover_StartMove, MoverStartMove);
        
    }

    public void InitIcon(E_CharacterType _characterType)
    {
        characterSelectButton.onClick.AddListener(OnClickIconButton);
        characterType = _characterType;
        characterSOData = LoadCharacterSOData();
        characterImage.sprite = characterSOData.characterSprite;
        characterName = characterSOData.characterName;
    }
    CharacterDataSO LoadCharacterSOData()
    {
        return Resources.Load<CharacterDataSO>(characterSO_ParentPath + characterType);
    }

    /// <summary>
    /// 设置本回合行动点
    /// </summary>
    /// <param name="value"></param>
    public void SetMoveDot(int value)
    {
        remainActionPoints = value;
        canMove = (remainActionPoints > 0 ? true : false);
    }

    void MoverStartMove() { 
        isActive = false;
        Debug.Log(characterSelectButton);
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
    void OnClickIconButton()
    {
        HexPathFindingManager hexPathFindingManager = GameRoot.GetManager<HexPathFindingManager>();
        if (!GameRoot.GetManager<MapMoverChecker>().GetTargetMover(this))
            return;
        var mover = GameRoot.GetManager<MapMoverChecker>().GetTargetMover(this);
        GameRoot.GetManager<OrthoCameraNavigator>().FocusOnTarget(mover.gameObject);

        if (canMove)
            isActive = !isActive;
        else{
            FlashWarnning();
            return;
        }

        if (isActive){
            hexPathFindingManager.SetPlayerStartRoom(
             mover.CurrentRooom);

            //如果该角色本回合有行动点尚未用尽，可以进行移动
            SetCameraToCharacter();

            if (canMove){
                characterSelectButton.GetComponent<Image>().color = Color.blue;
                //激活寻路管理器寻路状态
                hexPathFindingManager.SetPathFindState(true, remainActionPoints);
            }
            else{
                hexPathFindingManager.SetPathFindState(false, remainActionPoints);

            }
        }
        else{
            hexPathFindingManager.SetPathFindState(false);
            characterSelectButton.GetComponent<Image>().color = Color.white;
        }
    }
}
