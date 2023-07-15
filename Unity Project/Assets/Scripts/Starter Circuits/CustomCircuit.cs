using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCircuit : Circuit
{
    private List<Circuit> circuitList;

    private Input[] inputs;

    private List<Input> emptyInputs;

    private Output[] outputs;

    private List<Output> emptyOutputs;

    private List<string> emptyInputLabels, emptyOutputLabels;

    public CustomCircuit(string circuitName, List<Input> inputs, List<Output> outputs, List<Input> emptyInputs, List<Output> emptyOutputs, List<string> emptyInputLabels, List<string> emptyOutputLabels, Vector2 startingPosition)
    {
        CircuitName = circuitName;
        this.inputs = inputs.ToArray();
        this.outputs = outputs.ToArray();
        this.emptyInputs = emptyInputs;
        this.emptyOutputs = emptyOutputs;
        this.emptyInputLabels = emptyInputLabels; this.emptyOutputLabels = emptyOutputLabels;
        CircuitVisualizer.Instance.VisualizeCustomCircuit(this, startingPosition);
    }

    public new Input[] Inputs { get { return inputs; } }

    public List<Input> EmptyInputs { get { return emptyInputs; } }

    public new Output[] Outputs { get { return outputs; } }

    public List<Output> EmptyOutputs { get { return emptyOutputs; } }

    protected override List<Output> UpdateOutputs()
    {
        throw new System.NotImplementedException();
    }
}
