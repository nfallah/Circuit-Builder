using System.Collections.Generic;

public class InputGate : Circuit
{
    private bool powered;

    public InputGate() : base("INPUT", 0, 1) { }

    protected override List<Output> UpdateOutputs()
    {
        bool outputStatus = Outputs[0].Powered;
        List<Output> outputs = new List<Output>();

        Outputs[0].Powered = powered;

        if (outputStatus != Outputs[0].Powered) outputs.Add(Outputs[0]);

        return outputs;
    }

    // Setter method
    public bool Powered { set { powered = value; Update(); UpdateChildren(); } }
}