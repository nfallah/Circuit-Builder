using System;
using UnityEngine;

/// <summary>
/// DisplayReference is assigned to and stores values unique to the Display prefab.
/// </summary>
public class DisplayReference : MonoBehaviour
{
    /// <summary>
    /// The inputs of the display.
    /// </summary>
    [SerializeField]
    GameObject[] inputs = new GameObject[8];

    /// <summary>
    /// The preview pins (on hover) corresponding to each value within <seealso cref="inputs"/>.
    /// </summary>
    [SerializeField]
    GameObject[] previewPins = new GameObject[8];

    /// <summary>
    /// The input statuses of the display.
    /// </summary>
    [SerializeField]
    MeshRenderer[] inputStatuses = new MeshRenderer[8];

    /// <summary>
    /// The pins (on power) corresponding to each value within <seealso cref="inputs"/>.
    /// </summary>
    [SerializeField]
    MeshRenderer[] pins = new MeshRenderer[8];

    // Ensures the sizes of each array cannot be modified within the inspector
    private void OnValidate()
    {
        if (inputs.Length != 8) Array.Resize(ref inputs, 8);

        if (inputStatuses.Length != 8) Array.Resize(ref inputStatuses, 8);

        if (pins.Length != 8) Array.Resize(ref pins, 8);

        if (previewPins.Length != 8) Array.Resize(ref previewPins, 8);
    }

    // Getter methods
    public GameObject[] Inputs { get { return inputs; } }

    public GameObject[] PreviewPins { get { return previewPins; } }

    public MeshRenderer[] InputStatuses { get { return inputStatuses; } }

    public MeshRenderer[] Pins { get { return pins; } }
}