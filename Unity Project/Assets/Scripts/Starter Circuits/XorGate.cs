using System.Collections.Generic;
using UnityEngine;

public class XOrGate : Circuit
{
    public XOrGate() : this(Vector2.zero) { }

    public XOrGate(Vector2 startingPos) : base("XOR", 2, 1, startingPos) { }

    protected override List<Output> UpdateOutputs()
    {
        bool outputStatus = Outputs[0].Powered;
        List<Output> outputs = new List<Output>();

        Outputs[0].Powered = Inputs[0].Powered && !Inputs[1].Powered || !Inputs[0].Powered && Inputs[1].Powered;

        if (outputStatus != Outputs[0].Powered) outputs.Add(Outputs[0]);

        return outputs;
    }
}