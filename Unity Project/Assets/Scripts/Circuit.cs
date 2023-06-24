using System.Collections.Generic;
using UnityEngine;

public abstract class Circuit
{
    // Represents all required members of an input node
    public class Input
    {
        private bool powered; // Represents whether the input is powered by means of the attached circuit

        private Circuit parentCircuit; // Represents the output of the circuit currently attached as input

        private GameObject wire; // Represents the wire attached to this input, if any

        private Transform transform; // Represents the in-scene transform of the input

        // Getter and setter methods
        public bool Powered { get { return powered; } set { powered = value; } }

        public Circuit ParentCircuit { get { return parentCircuit; } set { parentCircuit = value; } }

        public GameObject Wire { get { return wire; } set { wire = value;  } }

        public Transform Transform { get { return transform; } set { transform = value; } }
    }

    // Represents all required members of an output node
    public class Output
    {
        private bool powered; // Represents whether a specific output is powered by this current circuit

        private GameObject wire; // Represents the wire going out of this output, if any

        private int numOutputs; // Serves as a centralized method of checking for the total number of connections from said output

        private List<Circuit> circuits = new List<Circuit>(); // Represents the list of attached circuit(s) from this output

        private List<int> inputIndex = new List<int>(); // Represents the list of which input(s) this output has attached to

        private Transform transform; // Represents the in-scene transform of the output

        // Getter and setter methods
        public bool Powered { get { return powered; } set { powered = value; } }
        public GameObject Wire { get { return wire; } set { wire = value; } }

        public int NumOutputs { get { return numOutputs; } set { numOutputs = value; } }

        public List<Circuit> Circuits { get { return circuits; } set { circuits = value; } }

        public List<int> InputIndex { get { return inputIndex; } set { inputIndex = value; } }

        public Transform Transform { get { return transform; } set { transform = value; } }
    }

    private GameObject physicalObject;

    private readonly Input[] inputs; // The list of input nodes for this circuit

    private readonly Output[] outputs; // The list of output nodes for this circuit

    private readonly string circuitName;

    // Utilized by inherited circuits to determine the specific number of input and output nodes
    public Circuit(string circuitName, int numInputs, int numOutputs)
    {
        this.circuitName = circuitName;
        inputs = new Input[numInputs];
        outputs = new Output[numOutputs];

        for (int i = 0; i < numInputs; i++) { inputs[i] = new Input(); }

        for (int i = 0; i < numOutputs; i++) { outputs[i] = new Output(); }
    }

    /*
     * Updates the output nodes based on a new input specified by a parent circuit (parameters)
     * Then recursively updates its own child circuits with the same method
     */
    public void UpdateCircuit(bool powered, Circuit circuit, int inputIndex)
    {
        inputs[inputIndex].Powered = powered;
        inputs[inputIndex].ParentCircuit = circuit;
        UpdateOutputs();
        UpdateChildren();
    }

    // Recursively calls and updates all children of the circuit
    public void UpdateChildren()
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

    // Getter and setter methods
    public GameObject PhysicalObject { get { return physicalObject; } set { physicalObject = value; } }

    // Getter methods
    public Input[] Inputs { get { return inputs; } }

    public Output[] Outputs {  get { return outputs; } }

    public string CircuitName { get { return circuitName; } }
}