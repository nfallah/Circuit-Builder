using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logical representation of an INPUT gate.
/// </summary>
public class InputGate : Circuit
{
    /// <summary>
    /// Powered status unique to an INPUT gate.
    /// </summary>
    private bool powered;

    public InputGate() : this(Vector2.zero) { }

    public InputGate(Vector2 startingPos) : base("INPUT", 0, 1, startingPos) { }

    /// <summary>
    /// Returns an output to update if the output has changed due to alterations in input power statuses.
    /// </summary>
    /// <returns>The list of outputs that should have their connections called.</returns>
    protected override List<Output> UpdateOutputs()
    {
        bool outputStatus = Outputs[0].Powered;
        List<Output> outputs = new List<Output>();
        
        // INPUT gate representation
        Outputs[0].Powered = powered;

        if (outputStatus != Outputs[0].Powered) outputs.Add(Outputs[0]);

        return outputs;
    }

    /// <summary>
    /// Getter and setter method; setting the powered value of the input node will also create an update call.
    /// </summary>
    public bool Powered { get { return powered; } set { powered = value; Update(); UpdateChildren(); } }
}