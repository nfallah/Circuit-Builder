using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ButtonRightClick is assigned to the save slot buttons within the menu, enabling for the deletion of a scene.
/// </summary>
public class ButtonRightClick : MonoBehaviour, IPointerClickHandler
{
    /// <summary>
    /// Stores which editor scene this instance is assigned to.
    /// </summary>
    [SerializeField] int saveIndex;

    /// <summary>
    /// Begins the scene deletion process if the captured input was a right click.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) MenuInterfaceManager.Instance.BeginSceneDeletion(saveIndex);
    }
}