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

    private GameObject connections;

    private List<string> emptyInputLabels, emptyOutputLabels;

    public List<Output> finalOutputs;

    private static CustomCircuit currentCustomCircuit;

    public CustomCircuit(PreviewStructure previewStructure) : this(previewStructure, Vector2.zero, true) {}

    public CustomCircuit(PreviewStructure previewStructure, Vector2 startingPos, bool isFirst) : base(previewStructure.Name, Vector2.positiveInfinity)
    {
        if (isFirst) { Visible = true; currentCustomCircuit = this; }
        this.previewStructure = previewStructure;
        CircuitName = previewStructure.Name;
        CreateCircuit(startingPos);
    }

    private void CreateCircuit(Vector2 startingPos)
    {
        connections = new GameObject("Connections [CUSTOM]");

        foreach (CircuitIdentifier circuitIdentifier in previewStructure.Circuits)
        {
            Circuit circuit = CircuitIdentifier.RestoreCircuit(circuitIdentifier, false);
            if (circuit.GetType() != typeof(CustomCircuit)) circuit.customCircuit = this;

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

        int index = 0;
        finalOutputs = new List<Output>(emptyOutputs);

        if (Visible) CircuitVisualizer.Instance.VisualizeCustomCircuit(this, startingPos);

        foreach (InternalConnection internalConnection in previewStructure.Connections)
        {
            CircuitConnector.Connection connection = connections.AddComponent<CircuitConnector.Connection>();
            Input input = inputs[internalConnection.InputIndex];
            Output output = outputs[internalConnection.OutputIndex];

            connection.Input = input;
            connection.Output = output;
            input.Connection = connection;
            input.ParentOutput = output;
            output.Connections.Add(connection);
            output.ChildInputs.Add(input);
            UpdateCircuit(input, output);
            index++;
        }

        UpdateOutputs();

        if (!Visible)
        {
            customCircuit = currentCustomCircuit;
            connections.transform.SetParent(customCircuit.Connections.transform);
        }

        else
        {
            currentCustomCircuit = null;
        }
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

    // Getter methods
    public PreviewStructure PreviewStructure { get { return previewStructure; } }

    
    public GameObject Connections { get { return connections; } }
}