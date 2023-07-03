using System;
using UnityEngine;

public class DisplayReference : MonoBehaviour
{
    [SerializeField] GameObject[] inputs = new GameObject[8];

    [SerializeField] MeshRenderer[] inputStatuses = new MeshRenderer[8], pins = new MeshRenderer[8];

    // Ensures the sizes of each array cannot be modified via the inspector
    private void OnValidate()
    {
        if (inputs.Length != 8) Array.Resize(ref inputs, 8);
        if (inputStatuses.Length != 8) Array.Resize(ref inputStatuses, 8);
        if (pins.Length != 8) Array.Resize(ref pins, 8);
    }

    public GameObject[] Inputs { get { return inputs; } }

    public MeshRenderer[] InputStatuses { get { return inputStatuses; } }

    public MeshRenderer[] Pins { get { return pins; } }
}