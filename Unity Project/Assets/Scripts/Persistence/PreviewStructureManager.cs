using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewStructureManager : MonoBehaviour
{
    private static PreviewStructureManager instance;

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

        // Test #2: the custom circuit must consist of at least 1 circuit
        if (EditorStructureManager.Instance.Circuits.Count == 0)
        {
            TaskbarManager.Instance.CircuitSaveError("The custom circuit must consist of (1) or more circuits.");
            yield break;
        }

        // Test #3: the custom circuit must not include any input gates or display
        foreach (Circuit circuit in EditorStructureManager.Instance.Circuits)
        {
            Type type = circuit.GetType();

            if (type == typeof(InputGate) || type == typeof(Display))
            {
                TaskbarManager.Instance.CircuitSaveError("The custom circuit must not consist of any input gates or displays.");
                yield break;
            }
        }

        // Test #4: the custom circuit must be completely connected (DFS)

        // Test #5: the custom circuit must have at least one empty output

        // Test #6: the circuit name must be unique
        foreach (PreviewStructure previewStructure in MenuSetupManager.Instance.PreviewStructures)
        {
            if (previewStructure.Name == name)
            {
                TaskbarManager.Instance.CircuitSaveError("The custom circuit must have a unique name.");
                yield break;
            }
        }

        // ASK FOR INPUT AND OUTPUT ORDER HERE

        // ASK FOR INPUT AND OUTPUT ORDER HERE

        TaskbarManager.Instance.OnSuccessfulPreviewVerification();

        // FINALIZE HERE



        // FINALIZE HERE

        MenuSetupManager.Instance.PreviewStructures.Add(new PreviewStructure(name));
        TaskbarManager.Instance.OnSuccessfulPreviewStructure();
    }

    public static PreviewStructureManager Instance { get { return instance; } }
}