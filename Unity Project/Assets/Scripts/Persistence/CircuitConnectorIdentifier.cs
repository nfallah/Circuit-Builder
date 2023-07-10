using System;

[Serializable]
public class CircuitConnectorIdentifier
{
    public int inputCircuitIndex, outputCircuitIndex, inputIndex, outputIndex;

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