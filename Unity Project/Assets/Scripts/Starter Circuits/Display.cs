using System.Collections.Generic;
using UnityEngine;

public class Display : Circuit
{
    private GameObject[] previewPins = new GameObject[8];

    private MeshRenderer[] pins = new MeshRenderer[8];

    public Display() : this(Vector2.zero) { }

    public Display(Vector2 startingPos) : base("DISPLAY", 8, 0, startingPos) { }

    protected override List<Output> UpdateOutputs()
    {
        for (int i = 0; i < 8; i++)
        {
            pins[i].material = Inputs[i].Powered ? CircuitVisualizer.Instance.PowerOnMaterial : CircuitVisualizer.Instance.PowerOffMaterial;
        }

        return new List<Output>();
    }

    public GameObject[] PreviewPins { get { return previewPins; } set { previewPins = value; } }

    public MeshRenderer[] Pins { set { pins = value; } }
}