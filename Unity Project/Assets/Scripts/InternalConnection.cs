using System;
using UnityEngine;

[Serializable]
public class InternalConnection
{
    [SerializeField] int inputIndex, outputIndex;

    public InternalConnection(int inputIndex, int outputIndex)
    {
        this.inputIndex = inputIndex;
        this.outputIndex = outputIndex;
    }

    public int InputIndex { get { return inputIndex; } }

    public int OutputIndex { get { return outputIndex; } }
}