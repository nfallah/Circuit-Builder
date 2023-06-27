using System.Collections.Generic;

public class NotGate : Circuit
{
    public NotGate() : base("NOT", 1, 1) {}

    protected override List<Output> UpdateOutputs()
    {
        bool outputStatus = Outputs[0].Powered;
        List<Output> outputs = new List<Output>();

        Outputs[0].Powered = !Inputs[0].Powered;

        if (outputStatus != Outputs[0].Powered) outputs.Add(Outputs[0]);

        return outputs;
    }
}