using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewStructureManager : MonoBehaviour
{
    private static PreviewStructureManager instance;

    private bool[] reachedCircuits;

    private int circuitCount;

    private List<Circuit.Input> emptyInputs;

    private List<Circuit.Output> emptyOutputs;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("PreviewStructureManager instance already established; terminating.");
        }

        instance = this;
    }

    public void VerifyPreviewStructure(string name)
    {
        StartCoroutine(VerifyPreviewStructureCoroutine(name));
    }

    public IEnumerator VerifyPreviewStructureCoroutine(string name)
    {
        yield return null;

        // Test #1: the custom circuit cannot have an empty name.
        if (name == "")
        {
            TaskbarManager.Instance.CircuitSaveError("The custom circuit must not have an empty name.");
            yield break;
        }

        // Test #2: the circuit name must be unique
        foreach (PreviewStructure previewStructure in MenuSetupManager.Instance.PreviewStructures)
        {
            if (previewStructure.Name == name)
            {
                TaskbarManager.Instance.CircuitSaveError("The custom circuit must have a unique name.");
                yield break;
            }
        }

        // Test #3: the custom circuit must consist of at least 1 circuit
        if (EditorStructureManager.Instance.Circuits.Count == 0)
        {
            TaskbarManager.Instance.CircuitSaveError("The custom circuit must consist of (1) or more circuits.");
            yield break;
        }

        // Test #4: the custom circuit must not include any input gates or display
        foreach (Circuit circuit in EditorStructureManager.Instance.Circuits)
        {
            Type type = circuit.GetType();

            if (type == typeof(InputGate) || type == typeof(Display))
            {
                TaskbarManager.Instance.CircuitSaveError("The custom circuit must not consist of any input gates or displays.");
                yield break;
            }
        }

        // Test #5: the custom circuit must be completely connected
        reachedCircuits = new bool[EditorStructureManager.Instance.Circuits.Count];
        circuitCount = 0;
        emptyInputs = new List<Circuit.Input>(); emptyOutputs = new List<Circuit.Output>();
        CircuitConnectionTest(EditorStructureManager.Instance.Circuits[0]);

        if (circuitCount != reachedCircuits.Length)
        {
            TaskbarManager.Instance.CircuitSaveError("The custom circuit must be entirely connected.");
            yield break;
        }

        // Test #6: the custom circuit must have at least one empty output
        if (emptyOutputs.Count == 0)
        {
            TaskbarManager.Instance.CircuitSaveError("The custom circuit must have (1) or more empty outputs.");
            yield break;
        }

        TaskbarManager.Instance.CloseMenu();
        TaskbarManager.Instance.NullState();
        IOAssigner.Instance.Initialize(emptyInputs, emptyOutputs);

        // ASK FOR INPUT AND OUTPUT ORDER HERE


        // ASK FOR INPUT AND OUTPUT ORDER HERE

        //**********TaskbarManager.Instance.OnSuccessfulPreviewVerification();

        // FINALIZE HERE



        // FINALIZE HERE

        //**********MenuSetupManager.Instance.PreviewStructures.Add(new PreviewStructure(name));
        //**********TaskbarManager.Instance.OnSuccessfulPreviewStructure();
    }

    /// <summary>
    /// Performs a depth-first search starting at the first placed circuit to determine whether the scene represents a complete graph. <br/>
    /// At the same time, any circuit input or output without a connection is stored for the next test.
    /// </summary>
    private void CircuitConnectionTest(Circuit currentCircuit)
    {
        int index = EditorStructureManager.Instance.Circuits.IndexOf(currentCircuit);

        if (reachedCircuits[index]) return;

        reachedCircuits[index] = true;
        circuitCount++;

        foreach (Circuit.Input input in currentCircuit.Inputs)
        {
            if (input.ParentOutput == null) { emptyInputs.Add(input); continue; }

            CircuitConnectionTest(input.ParentOutput.ParentCircuit);
        }

        foreach (Circuit.Output output in currentCircuit.Outputs)
        {
            if (output.ChildInputs.Count == 0) { emptyOutputs.Add(output); continue; }

            foreach (Circuit.Input input in output.ChildInputs)
            {
                CircuitConnectionTest(input.ParentCircuit);
            }
        }
    }

    public static PreviewStructureManager Instance { get { return instance; } }
}