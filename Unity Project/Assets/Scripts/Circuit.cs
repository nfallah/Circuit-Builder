using System.Collections.Generic;

public abstract class Circuit
{
    // Represents all required members of an input node
    private class Input
    {
        private bool powered; // Represents whether the input is powered by means of the attached circuit

        private Circuit circuit; // Represents the output of the circuit currently attached as input

        // Getter and setter methods
        public bool Powered { get { return powered; } set { powered = value; } }

        public Circuit Circuit { get { return circuit; } set { circuit = value; } }
    }

    // Represents all rquired members of an output node
    private class Output
    {
        private bool powered; // Represents whether a specific output is powered by this current circuit

        private int numOutputs; // Serves as a centralized method of checking for the total number of connections from said output

        private List<Circuit> circuits = new List<Circuit>(); // Represents the list of attached circuit(s) from this output

        private List<int> inputIndex = new List<int>(); // Represents the list of which input(s) this output has attached to

        // Getter and setter methods
        public bool Powered { get { return powered; } set { powered = value; } }

        public int NumOutputs { get { return numOutputs; } set { numOutputs = value; } }

        public List<Circuit> Circuits { get { return circuits; } set { circuits = value; } }

        public List<int> InputIndex { get { return inputIndex; } set { inputIndex = value; } }
    }

    private readonly Input[] inputs; // The list of input nodes for this circuit

    private readonly Output[] outputs; // The list of output nodes for this circuit

    // Utilized by inherited circuits to determine the specific number of input and output nodes
    public Circuit(int numInputs, int numOutputs)
    {
        inputs = new Input[numInputs];
        outputs = new Output[numOutputs];
    }

    /*
     * Updates the output nodes based on a new input specified by a parent circuit (parameters)
     * Then recursively updates its own child circuits with the same method
     */
    public void UpdateCircuit(bool powered, Circuit circuit, int inputIndex)
    {
        inputs[inputIndex].Powered = powered;
        inputs[inputIndex].Circuit = circuit;
        UpdateOutputs();
        UpdateChildren();
    }

    // Recursively calls and updates all children of the circuit
    private void UpdateChildren()
    {
        foreach (Output output in outputs)
        {
            for (int i = 0; i < output.NumOutputs; i++)
            {
                output.Circuits[i].UpdateCircuit(output.Powered, this, output.InputIndex[i]);
            }
        }
    }

    /*
     * Abstract implementation representing the input/output logic of the circuit
     * Utilizes all inputs to calculate the state of all outputs
     */
    public abstract void UpdateOutputs();
}