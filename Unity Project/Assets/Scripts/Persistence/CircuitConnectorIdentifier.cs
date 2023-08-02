using System;
using UnityEngine;

/// <summary>
/// CircuitConnectorIdentifier serializes a connection by storing relevant index values.
/// </summary>
[Serializable]
public class CircuitConnectorIdentifier
{
    // Serialized values that are provided within the primary constructor.
    [SerializeField]
    int inputCircuitIndex,
        outputCircuitIndex,
        inputIndex,
        outputIndex;

    /// <param name="inputCircuitIndex">The ordered index of the circuit that this input is a part of.</param>
    /// <param name="outputCircuitIndex">The ordered index of the circuit that this output is a part of.</param>
    /// <param name="inputIndex">The ordered index of the circuit's inputs that this input is a part of.</param>
    /// <param name="outputIndex">The ordered index of the circuit's outputs that this output is a part of.</param>
    public CircuitConnectorIdentifier(int inputCircuitIndex, int outputCircuitIndex, int inputIndex, int outputIndex)
    {
        this.inputCircuitIndex = inputCircuitIndex;
        this.outputCircuitIndex = outputCircuitIndex;
        this.inputIndex = inputIndex;
        this.outputIndex = outputIndex;
    }

    // Getter methods
    public int InputCircuitIndex { get { return inputCircuitIndex; } }
    
    public int OutputCircuitIndex { get { return outputCircuitIndex; } }

    public int InputIndex { get { return inputIndex; } }

    public int OutputIndex { get { return outputIndex; } }
}