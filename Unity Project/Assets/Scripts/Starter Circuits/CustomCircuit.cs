using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCircuit : Circuit
{
    private PreviewStructure previewStructure;

    private List<Circuit> circuitList = new List<Circuit>();

    private List<Input> inputs = new List<Input>(), emptyInputs = new List<Input>();

    private List<Output> outputs = new List<Output>(), emptyOutputs = new List<Output>();

    private List<string> emptyInputLabels, emptyOutputLabels;

    public CustomCircuit(PreviewStructure previewStructure) : this(previewStructure, Vector2.zero, true) {}

    public CustomCircuit(PreviewStructure previewStructure, Vector2 startingPos, bool isFirst) : base(previewStructure.Name, Vector2.positiveInfinity)
    {
        if (isFirst) Visible = true;

        this.previewStructure = previewStructure;
        CreateCircuit(startingPos);
    }

    private void CreateCircuit(Vector2 startingPos)
    {
        foreach (CircuitIdentifier circuitIdentifier in previewStructure.Circuits)
        {
            Circuit circuit = CircuitIdentifier.RestoreCircuit(circuitIdentifier, false);
            circuitList.Add(circuit);

            foreach (Input input in circuit.Inputs) inputs.Add(input);

            foreach (Output output in circuit.Outputs) outputs.Add(output);
        }

        int inputAmount = previewStructure.InputLabels.Count;

        for (int i = 0; i < inputAmount; i++)
        {
            emptyInputs.Add(inputs[previewStructure.InputOrders.IndexOf(i)]);
        }

        int outputAmount = previewStructure.OutputLabels.Count;

        for (int i = 0; i < outputAmount; i++)
        {
            emptyOutputs.Add(outputs[previewStructure.OutputOrders.IndexOf(i)]);
        }

        Inputs = emptyInputs.ToArray(); Outputs = emptyOutputs.ToArray();

        // Add connections here

        if (!Visible) return;

        CircuitVisualizer.Instance.VisualizeCustomCircuit(this, startingPos);
        UpdateOutputs();
    }

    public List<Input> EmptyInputs { get { return emptyInputs; } }

    public List<Output> EmptyOutputs { get { return emptyOutputs; } }

    protected override List<Output> UpdateOutputs()
    {
        foreach (Input input in emptyInputs)
        {
            UpdateCircuit(false, input, null);
        }

        return null;
    }

    // Getter method
    public PreviewStructure PreviewStructure { get { return previewStructure; } }
}