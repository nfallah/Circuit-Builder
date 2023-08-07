using System;
using UnityEngine;

/// <summary>
/// InternalConnection serializes all connections occurring in a custom circuit.
/// </summary>
[Serializable]
public class InternalConnection
{
    /// <summary>
    /// The input and output indeces of the connection.<br/><br/>
    /// These values are referenced once the <see cref="Circuit.Input"/> and <see cref="Circuit.Output"/> lists of the <see cref="CustomCircuit"/> are established.
    /// </summary>
    [SerializeField] int inputIndex, outputIndex;

    public InternalConnection(int inputIndex, int outputIndex)
    {
        this.inputIndex = inputIndex;
        this.outputIndex = outputIndex;
    }

    // Getter methods
    public int InputIndex { get { return inputIndex; } }

    public int OutputIndex { get { return outputIndex; } }
}