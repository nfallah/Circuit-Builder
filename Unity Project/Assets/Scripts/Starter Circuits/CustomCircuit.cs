using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCircuit : Circuit
{
    private PreviewStructure previewStructure;

    private List<Circuit> circuitList;

    private Input[] inputs;

    private List<Input> emptyInputs;

    private Output[] outputs;

    private List<Output> emptyOutputs;

    private List<string> emptyInputLabels, emptyOutputLabels;

    public CustomCircuit(PreviewStructure previewStructure) : this(previewStructure, Vector2.zero) {}

    public CustomCircuit(PreviewStructure previewStructure, Vector2 startingPos)
    {
        this.previewStructure = previewStructure;
    }

    public new Input[] Inputs { get { return inputs; } }

    public List<Input> EmptyInputs { get { return emptyInputs; } }

    public new Output[] Outputs { get { return outputs; } }

    public List<Output> EmptyOutputs { get { return emptyOutputs; } }

    protected override List<Output> UpdateOutputs()
    {
        throw new NotImplementedException();
    }

    // Getter method
    public PreviewStructure PreviewStructure { get { return previewStructure; } }
}