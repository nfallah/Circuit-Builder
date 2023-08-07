using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CircuitCaller handles every circuit call after a short delay defined in <see cref="Circuit"/>.
/// </summary>
public class CircuitCaller : MonoBehaviour
{
    private static CircuitCaller instance; // Singleton state reference

    // Enforces a singleton state pattern
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("CircuitCaller instance already established; terminating.");
        }

        instance = this;
    }

    /// <summary>
    /// Starts a coroutine that shortly accesses the list of provided update calls.
    /// </summary>
    /// <param name="updateCalls">The list of update calls to pursue.</param>
    public static void InitiateUpdateCalls(List<Circuit.UpdateCall> updateCalls) { instance.StartCoroutine(UpdateCalls(updateCalls)); }

    /// <summary>
    /// Attempts to access the list of provided update calls.
    /// </summary>
    /// <param name="updateCalls">The list of update calls to call.</param>
    private static IEnumerator UpdateCalls(List<Circuit.UpdateCall> updateCalls)
    {
        yield return new WaitForSeconds(Circuit.clockSpeed);

        foreach (Circuit.UpdateCall updateCall in updateCalls)
        {
            // Sometime between the call initiation and now, the referenced output was destroyed and should no longer be pursued.
            if (updateCall.Input.ParentOutput == null) continue;

            if (!CustomCircuitTest(updateCall)) continue;

            // Otherwise, the update call is accessed to update the relevant circuits.
            Circuit.UpdateCircuit(updateCall.Powered, updateCall.Input, updateCall.Output);
        }
    }

    /// <summary>
    /// Ensures that an update call pertaining to a custom circuit only runs if its custom circuit is not deleted.
    /// </summary>
    /// <param name="updateCall">The update call to test.</param>
    /// <returns>Whether this update call should be utilized.</returns>
    private static bool CustomCircuitTest(Circuit.UpdateCall updateCall)
    {
        // In preview scene, therefore not necessary to run the test
        if (EditorStructureManager.Instance == null) return true;

        // If the input of an update call is under a parent circuit, it is guaranteed that its output is as well.
        bool isInternalConnection = updateCall.Input.ParentCircuit.customCircuit != null;

        // Connection does not pertain to the inside of a custom circuit.
        if (!isInternalConnection) return true;

        // Otherwise, obtain the top-most custom circuit and check to see if it is still within the scene.
        CustomCircuit customCircuitParent = updateCall.Input.ParentCircuit.customCircuit;

        while (customCircuitParent.customCircuit != null) customCircuitParent = customCircuitParent.customCircuit;

        return !customCircuitParent.ShouldDereference;
    }

    /// <summary>
    /// Deletes the specified circuit from the scene.
    /// </summary>
    /// <param name="circuit">The circuit to destroy.</param>
    public static void Destroy(Circuit circuit)
    {
        // First disconnects any potential input connections
        foreach (Circuit.Input input in circuit.Inputs)
        {
            if (input.Connection != null)
            {
                CircuitConnector.Disconnect(input.Connection);
            }
        }

        // Then disconnects any potential output connections
        foreach (Circuit.Output output in circuit.Outputs)
        {
            foreach (CircuitConnector.Connection connection in new List<CircuitConnector.Connection>(output.Connections))
            {
                CircuitConnector.Disconnect(connection);
            }
        }

        // Ensures all remaining calls within the custom circuit are skipped
        if (circuit.GetType() == typeof(CustomCircuit)) ((CustomCircuit)circuit).ShouldDereference = true;

        EditorStructureManager.Instance.DisplaySavePrompt = true; // Destroying a circuit triggers the save prompt
        EditorStructureManager.Instance.Circuits.Remove(circuit); // Removes circuit for potential serialization
        Destroy(circuit.PhysicalObject);
    }
}