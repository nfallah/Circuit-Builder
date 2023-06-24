﻿public class NotGate : Circuit
{
    public NotGate() : base("NOT", 1, 1) {}

    public override void UpdateOutputs()
    {
        Outputs[0].Powered = !Inputs[0].Powered;
    }
}