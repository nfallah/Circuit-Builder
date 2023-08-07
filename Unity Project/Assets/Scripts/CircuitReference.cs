using UnityEngine;

/// <summary>
/// CircuitReference is a <see cref="Circuit"/> wrapper class attached to all instantiated circuit GameObjects.<br/><br/>
/// This script is utilized to obtain the target circuit for any given GameObject through raycasts.
/// </summary>
public class CircuitReference : MonoBehaviour
{
    /// <summary>
    /// The circuit that this 
    /// </summary>
    private Circuit circuit;

    // Getter and setter method
    public Circuit Circuit { get { return circuit; } set { circuit = value; } }
}