using DG.Tweening;
using System.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
public class HoverMoveUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Vector3 transVec;
    public float animDuration = 0.5f;

    public RectTransform target;
    public RectTransform pivot;
    public void OnPointerEnter(PointerEventData eventData)
    {
        target. DOMove(pivot.position+transVec, animDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        target.DOMove(pivot.position-transVec, animDuration);
    }


}
