using DG.Tweening;
using System.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
public class HoverFadeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float animDuration = 0.3f;

    public CanvasGroup target;
    public void OnPointerEnter(PointerEventData eventData)
    {
        target.DOFade(1,animDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        target.DOFade(0,animDuration);
    }


}
