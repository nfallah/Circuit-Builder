using System.Collections.Generic;
using UnityEngine;

public abstract class Circuit
{
    // Represents all required members of an input node
    public class Input
    {
        private bool powered; // Represents whether the input is powered by means of the attached circuit

        private Circuit parentCircuit; // Represents the circuit that this input composes

        private GameObject wire; // Represents the wire attached to this input, if any

        private MeshRenderer statusRenderer; // Mesh renderer of the material that displays whether the input is powered or not

        private Output parentOutput; // Represents the output connecting to this input, if any

        private Transform transform; // Represents the in-scene transform of the input

        // Getter and setter methods
        public bool Powered { get { return powered; } set { powered = value; } }

        public Circuit ParentCircuit { get { return parentCircuit; } set { parentCircuit = value; } }

        public GameObject Wire { get { return wire; } set { wire = value;  } }

        public MeshRenderer StatusRenderer { get { return statusRenderer; } set { statusRenderer = value; } }

        public Output ParentOutput { get { return parentOutput; } set { parentOutput = value; } }

        public Transform Transform { get { return transform; } set { transform = value; } }
    }

    // Represents all required members of an output node
    public class Output
    {
        private bool powered; // Represents whether a specific output is powered by this current circuit

        private Circuit parentCircuit; // Represents the circuit that this input composes

        private GameObject wire; // Represents the wire going out of this output, if any

        private List<Input> childInputs = new List<Input>(); // Represents the inputs this output connects to, if any

        private MeshRenderer statusRenderer; // Mesh renderer of the material that displays whether the output is powered or not

        private Transform transform; // Represents the in-scene transform of the output

        // Getter and setter methods
        public bool Powered { get { return powered; } set { powered = value; } }

        public Circuit ParentCircuit { get { return ParentCircuit; } set { ParentCircuit = value; } }

        public GameObject Wire { get { return wire; } set { wire = value; } }

        public List<Input> ChildInputs { get { return childInputs; } set { childInputs = value; } }

        public MeshRenderer StatusRenderer { get { return statusRenderer; } set { statusRenderer = value; } }

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
    public static void UpdateCircuit(bool powered, Input input, Output output)
    {
        input.Powered = powered;
        input.StatusRenderer.material = powered ? CircuitVisualizer.Instance.PowerOnMaterial : CircuitVisualizer.Instance.PowerOffMaterial;
        input.ParentOutput = output;
        input.ParentCircuit.Update();
        input.ParentCircuit.UpdateChildren();
    }

    // Recursively calls and updates all children of the circuit
    public void UpdateChildren()
    {
        foreach (Output output in outputs)
        {
            foreach (Input input in output.ChildInputs) UpdateCircuit(output.Powered, input, output);
        }
    }

    public void Update()
    {
        UpdateOutputs();
        UpdateStatuses();
    }

    private void UpdateStatuses()
    {
        foreach (Output output in outputs)
        {
            output.StatusRenderer.material = output.Powered ? CircuitVisualizer.Instance.PowerOnMaterial : CircuitVisualizer.Instance.PowerOffMaterial;
        }
    }

    /*
     * Abstract implementation representing the input/output logic of the circuit
     * Utilizes all inputs to calculate the state of all outputs
     */
    protected abstract void UpdateOutputs();

    // Getter and setter methods
    public GameObject PhysicalObject { get { return physicalObject; } set { physicalObject = value; } }

    // Getter methods
    public Input[] Inputs { get { return inputs; } }

    public Output[] Outputs {  get { return outputs; } }

    public string CircuitName { get { return circuitName; } }
}