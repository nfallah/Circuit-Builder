using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CustomCircuitButtons is utilized to add delegate listeners to its specified custom circuit within the menu for viewing and deleting.
/// </summary>
public class CustomCircuitButtons : MonoBehaviour
{
    /// <summary>
    /// The in-scene buttons pertaining to deletion and viewing a custom circuit.
    /// </summary>
    [SerializeField]
    Button deleteButton, viewButton;

    // Getter methods
    public Button DeleteButton { get { return deleteButton; } }

    public Button ViewButton { get { return viewButton; } }
}