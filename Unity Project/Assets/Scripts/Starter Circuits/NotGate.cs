using System.Collections.Generic;
using UnityEngine;

public class NotGate : Circuit
{
    public NotGate() : this(Vector2.zero) { }

    public NotGate(Vector2 startingPos) : base("NOT", 1, 1, startingPos) { }

    protected override List<Output> UpdateOutputs()
    {
        bool outputStatus = Outputs[0].Powered;
        List<Output> outputs = new List<Output>();

        Outputs[0].Powered = !Inputs[0].Powered;

        if (outputStatus != Outputs[0].Powered || MaterialNotMatching()) outputs.Add(Outputs[0]);

        return outputs;
    }

    private bool MaterialNotMatching()
    {
        if (Outputs[0].StatusRenderer == null) return false;

        return (Outputs[0].Powered && Outputs[0].StatusRenderer.material != CircuitVisualizer.Instance.PowerOnMaterial) ||
               (!Outputs[0].Powered && Outputs[0].StatusRenderer.material != CircuitVisualizer.Instance.PowerOffMaterial);
    }
}