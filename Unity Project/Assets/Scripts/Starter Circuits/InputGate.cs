using System.Collections.Generic;
using UnityEngine;

public class InputGate : Circuit
{
    private bool powered;

    public InputGate() : this(Vector2.zero) { }

    public InputGate(Vector2 startingPos) : base("INPUT", 0, 1, startingPos) { }

    protected override List<Output> UpdateOutputs()
    {
        bool outputStatus = Outputs[0].Powered;
        List<Output> outputs = new List<Output>();

        Outputs[0].Powered = powered;

        if (outputStatus != Outputs[0].Powered) outputs.Add(Outputs[0]);

        return outputs;
    }

    // Setter method
    public bool Powered { get { return powered; } set { powered = value; Update(); UpdateChildren(); } }
}