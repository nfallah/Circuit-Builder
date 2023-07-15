using System.Collections.Generic;
using UnityEngine;

public class Buffer : Circuit
{
    public Buffer() : this(Vector2.zero) { }

    public Buffer(Vector2 startingPos) : base("BUFFER", 1, 1, startingPos, true) { }

    protected override List<Output> UpdateOutputs()
    {
        bool outputStatus = Outputs[0].Powered;
        List<Output> outputs = new List<Output>();

        Outputs[0].Powered = Inputs[0].Powered;

        if (outputStatus != Outputs[0].Powered) outputs.Add(Outputs[0]);

        return outputs;
    }
}