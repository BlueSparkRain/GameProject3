using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler{
    [Header("高亮图")]
    public Image selectImage;

    private Color _defaultColor;

    void Start(){
        if (selectImage != null){
            _defaultColor = selectImage.color;
            SetAlpha(0f);
        }
    }

    public void OnPointerClick(PointerEventData eventData){
    }

    public void OnPointerEnter(PointerEventData eventData){
        if (selectImage != null)
            SetAlpha(1f);
    }
    public void OnPointerExit(PointerEventData eventData){
        if (selectImage != null)
            SetAlpha(0f);
    }
    void SetAlpha(float a){
        var c = selectImage.color;
        c.a = a;
        selectImage.color = c;
    }
}
