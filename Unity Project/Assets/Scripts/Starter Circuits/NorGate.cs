﻿// Universal gate
using System.Collections.Generic;

public class NOrGate : Circuit
{
    public NOrGate() : base("NOR", 2, 1) { }

    protected override List<Output> UpdateOutputs()
    {
        bool outputStatus = Outputs[0].Powered;
        List<Output> outputs = new List<Output>();

        Outputs[0].Powered = !(Inputs[0].Powered || Inputs[1].Powered);

        if (outputStatus != Outputs[0].Powered) outputs.Add(Outputs[0]);

        return outputs;
    }
}