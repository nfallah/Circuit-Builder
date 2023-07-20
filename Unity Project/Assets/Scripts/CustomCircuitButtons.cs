using UnityEngine;
using UnityEngine.UI;

public class CustomCircuitButtons : MonoBehaviour
{
    [SerializeField] Button deleteButton, viewButton;

    public Button DeleteButton { get { return deleteButton; } }

    public Button ViewButton { get { return viewButton; } }
}