using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class Circuit
{
    public static float clockSpeed = 0.075f; // Time it takes for one update call to occur; measured in seconds.

    public CustomCircuit customCircuit;

    // Represents all required members of an input node
    public class Input
    {
        public Input(Circuit parentCircuit) { this.parentCircuit = parentCircuit; }

        private bool powered; // Represents whether the input is powered by means of the attached circuit

        private Circuit parentCircuit; // Represents the circuit that this input composes

        private CircuitConnector.Connection connection; // Represents the connection related to this input, if any

        private MeshRenderer statusRenderer; // Mesh renderer of the material that displays whether the input is powered or not

        private Output parentOutput; // Represents the output connecting to this input, if any

        private Transform transform; // Represents the in-scene transform of the input

        // Getter and setter methods
        public bool Powered { get { return powered; } set { powered = value; } }

        public Circuit ParentCircuit { get { return parentCircuit; } set { parentCircuit = value; } }

        public CircuitConnector.Connection Connection { get { return connection; } set { connection = value;  } }

        public MeshRenderer StatusRenderer { get { return statusRenderer; } set { statusRenderer = value; } }

        public Output ParentOutput { get { return parentOutput; } set { parentOutput = value; } }

        public Transform Transform { get { return transform; } set { transform = value; } }
    }

    // Represents all required members of an output node
    public class Output
    {
        public Output(Circuit parentCircuit) { this.parentCircuit = parentCircuit; }

        private bool powered; // Represents whether a specific output is powered by this current circuit

        private Circuit parentCircuit; // Represents the circuit that this input composes

        private List<CircuitConnector.Connection> connections = new List<CircuitConnector.Connection>(); // Represents the connections related to this output, if any

        private List<Input> childInputs = new List<Input>(); // Represents the inputs this output connects to, if any

        private MeshRenderer statusRenderer; // Mesh renderer of the material that displays whether the output is powered or not

        private Transform transform; // Represents the in-scene transform of the output

        // Getter and setter methods
        public bool Powered { get { return powered; } set { powered = value; } }

        public Circuit ParentCircuit { get { return parentCircuit; } set { parentCircuit = value; } }

        public List<CircuitConnector.Connection> Connections { get { return connections; } set { connections = value; } }

        public List<Input> ChildInputs { get { return childInputs; } set { childInputs = value; } }

        public MeshRenderer StatusRenderer { get { return statusRenderer; } set { statusRenderer = value; } }

        public Transform Transform { get { return transform; } set { transform = value; } }
    }

    public class UpdateCall
    {
        private bool powered;

        private Input input;

        private Output output;

        public UpdateCall(bool powered, Input input, Output output)
        {
            this.powered = powered;
            this.input = input;
            this.output = output;
        }

        public bool Powered { get { return powered; } }

        public Input Input { get { return input; } }

        public Output Output { get { return output; } }
    }

    private GameObject physicalObject;

    private Input[] inputs; // The list of input nodes for this circuit

    private List<Output> outputsToUpdate; // The list of outputs whose powered statuses have changed due to an input change

    private Output[] outputs; // The list of output nodes for this circuit

    private bool visible;

    private string circuitName;

    // Utilized by inherited circuits to determine the specific number of input and output nodes
    public Circuit(string circuitName, int numInputs, int numOutputs, Vector2 startingPosition) : this(circuitName, numInputs, numOutputs, startingPosition, true) {}

    public Circuit(string circuitName, int numInputs, int numOutputs, Vector2 startingPosition, bool createIO)
    {
        this.circuitName = circuitName;

        if (createIO)
        {
            inputs = new Input[numInputs];
            outputs = new Output[numOutputs];

            for (int i = 0; i < numInputs; i++) { inputs[i] = new Input(this); }

            for (int i = 0; i < numOutputs; i++) { outputs[i] = new Output(this); }
        }

        visible = startingPosition.x != float.PositiveInfinity && startingPosition.y != float.PositiveInfinity;

        if (visible) CircuitVisualizer.Instance.VisualizeCircuit(this, startingPosition);
    }

    public Circuit(string circuitName, Vector2 startingPosition) : this(circuitName, 0, 0, startingPosition, false) { }

    public Circuit() { }

    /*
     * Updates a circuit and one of its inputs based on an output from another circuit
     * Then recursively calls all parent circuits of the input
     */
    public static void UpdateCircuit(bool powered, Input input, Output output)
    {
        Debug.Log("-=+=-");
        Debug.Log("POWERED: " + powered);
        Debug.Log("NAME: " + input.ParentCircuit.circuitName);
        Debug.Log("OUTPUT == NULL: " + (output == null));
        Debug.Log("-=+=-");
        input.Powered = powered;
        if (input.StatusRenderer != null) input.StatusRenderer.material = powered ? CircuitVisualizer.Instance.PowerOnMaterial : CircuitVisualizer.Instance.PowerOffMaterial;
        if (input.Connection != null) CircuitConnector.UpdateConnectionMaterial(input.Connection, powered);
        input.ParentOutput = output;
        input.ParentCircuit.Update();
        input.ParentCircuit.UpdateChildren();
    }

    public static void UpdateCircuit(Input input, Output output)
    {
        UpdateCircuit(output.Powered, input, output);
    }

    // Recursively calls and updates all connectio1ns instantiated by this circuit
    public void UpdateChildren()
    {
        List<UpdateCall> updateList = new List<UpdateCall>();

        foreach (Output output in outputsToUpdate)
        {
            if (customCircuit != null && customCircuit.finalOutputs.Contains(output)) customCircuit.finalOutputs.Remove(output);

            foreach (Input input in output.ChildInputs)
            {
                updateList.Add(new UpdateCall(output.Powered, input, output));
            }
        }

        CircuitCaller.InitiateUpdateCalls(updateList);
    }

    public void Update()
    {
        bool shouldCheckAllOutputs = customCircuit != null && customCircuit.finalOutputs.Count > 0;

        outputsToUpdate = UpdateOutputs();

        if (shouldCheckAllOutputs) { outputsToUpdate = Outputs.ToList(); }

        UpdateStatuses();
    }

    private void UpdateStatuses()
    {
        foreach (Output output in outputsToUpdate)
        {
            if (output.StatusRenderer == null) continue;

            output.StatusRenderer.material = output.Powered ? CircuitVisualizer.Instance.PowerOnMaterial : CircuitVisualizer.Instance.PowerOffMaterial;
        }
    }

    /*
     * Abstract implementation representing the input/output logic of the circuit
     * Utilizes all inputs to calculate the state of all outputs
     * Then returns the list of outputs that have changed as a result of an input alteration
     */
    protected abstract List<Output> UpdateOutputs();

    public bool Visible { get { return visible; } set { visible = value; } }

    // Getter and setter methods
    public GameObject PhysicalObject { get { return physicalObject; } set { physicalObject = value; } }

    // Getter methods
    public Input[] Inputs { get { return inputs; } set { inputs = value; } }

    public Output[] Outputs { get { return outputs; } set { outputs = value; } }

    public string CircuitName { get { return circuitName; } set { circuitName = value; } }
}