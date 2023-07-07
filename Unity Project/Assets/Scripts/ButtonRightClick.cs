using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonRightClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] int saveIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            MenuInterfaceManager.Instance.BeginSceneDeletion(saveIndex);
        }
    }
}