using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitCaller : MonoBehaviour
{
    private static CircuitCaller instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("CircuitCaller instance already established; terminating.");
        }

        instance = this;
    }

    public static void InitiateUpdateCalls(List<Circuit.UpdateCall> updateCalls)
    {
        instance.StartCoroutine(UpdateCalls(updateCalls));
    }

    private static IEnumerator UpdateCalls(List<Circuit.UpdateCall> updateCalls)
    {
        yield return new WaitForSeconds(Circuit.clockSpeed);

        foreach (Circuit.UpdateCall updateCall in updateCalls)
        {
            // This means that sometime from the call initiation and now, the connection was destroyed and should no longer be pursued.
            if (updateCall.Input.ParentOutput == null) continue;

            Circuit.UpdateCircuit(updateCall.Powered, updateCall.Input, updateCall.Output);
        }
    }

    public static void Destroy(Circuit circuit)
    {
        foreach (Circuit.Input input in circuit.Inputs)
        {
            if (input.Connection != null)
            {
                CircuitConnector.Disconnect(input.Connection);
            }
        }

        foreach (Circuit.Output output in circuit.Outputs)
        {
            foreach (CircuitConnector.Connection connection in new List<CircuitConnector.Connection>(output.Connections))
            {
                CircuitConnector.Disconnect(connection);
            }
        }

        EditorStructureManager.Instance.DisplaySavePrompt = true;
        EditorStructureManager.Instance.Circuits.Remove(circuit); // Removes circuit for potential serialization
        Destroy(circuit.PhysicalObject);
    }
}