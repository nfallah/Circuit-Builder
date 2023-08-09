using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CircuitConnector facilitates the connection process between circuits in the scene editor.
/// </summary>
public class CircuitConnector : MonoBehaviour
{
    // Singleton state reference
    private static CircuitConnector instance;
    
    /// <summary>
    /// Reference to the wire prefab.
    /// </summary>
    [SerializeField]
    GameObject wireReference;

    /// <summary>
    /// The material for powered and unpowered statuses respectively.
    /// </summary>
    [SerializeField]
    Material poweredMaterial, unpoweredMaterial;

    private bool cancelled;

    private Connection currentConnection;

    private GameObject currentWire;

    private int count;

    private Vector3 startingWirePos, currentPos;

    /// <summary>
    /// Represents a connection from the output circuit to the input circuit.
    /// </summary>
    public class Connection : MonoBehaviour
    {
        /// <summary>
        /// The input associated with the connection.
        /// </summary>
        private Circuit.Input input;

        /// <summary>
        /// The output associated with the connection.
        /// </summary>
        private Circuit.Output output;

        /// <summary>
        /// The starting and ending wires associated with the connection.
        /// </summary>
        private GameObject endingWire, startingWire;

        // Getter and setter methods
        public Circuit.Input Input { get { return input; } set { input = value; } }

        public Circuit.Output Output { get { return output; } set { output = value; } }

        public GameObject EndingWire { get { return endingWire; } set { endingWire = value; } }

        public GameObject StartingWire { get { return startingWire; } set { startingWire = value; } }
    }

    // Enforces a singleton state pattern and disables update calls.
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("CircuitConnector instance already established; terminating.");
        }

        instance = this;
        enabled = false;
    }

    // While the connection has not been cancelled or completed, this method allows for creating pivots to organize the wire.
    private void Update()
    {
        // If the connection process has been cancelled, disable update calls and return.
        if (cancelled)
        {
            cancelled = enabled = false;
            return;
        }

        // If the game is currently paused, skip frame.
        if (BehaviorManager.Instance.CurrentStateType == BehaviorManager.StateType.PAUSED) return;

        // Whether the user is staring at a valid input or output for completing the connection.
        bool staringAtIO = Physics.Raycast(CameraMovement.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo) && hitInfo.transform.gameObject.layer == BehaviorManager.Instance.IOLayerCheck;
        
        // The position to move the end of the wire to.
        // If hovered onto a valid input or output for completing the connection, it will snap to its position.
        Vector3 pos = staringAtIO ? hitInfo.transform.position : Coordinates.Instance.ModePos;

        pos.y = GridMaintenance.Instance.GridHeight;
        UpdatePosition(currentWire, currentPos, pos); // Updates the position of the wire.

        // Creates a new pivot as long as the wire is active (has a length >= 0).
        if (Input.GetMouseButtonDown(0) && currentWire.activeSelf)
        {
            count++;

            if (count == 2) startingWirePos = currentPos;

            currentConnection.EndingWire = currentWire;

            // Places a new wire at the current pivot
            InstantiateWire(currentConnection, Coordinates.Instance.ModePos);
        }
    }

    /// <summary>
    /// Final step in restoring the logic of a serialized connection by initializing a <seealso cref="Connection"/> instance and assigning all of its values.
    /// </summary>
    /// <param name="prefab">The base GameObject of the connection.</param>
    /// <param name="input">The input of the connection.</param>
    /// <param name="output">The output of the connection.</param>
    /// <param name="endingWire">The ending wire of the connection.</param>
    /// <param name="startingWire">The starting wire of the connection.</param>
    /// <param name="isEditor">Whether the connection is being restored in the editor.</param>
    public static void ConnectRestoration(GameObject prefab, Circuit.Input input, Circuit.Output output, GameObject endingWire, GameObject startingWire, bool isEditor)
    {
        Connection connection = prefab.AddComponent<Connection>();

        connection.Input = input;
        connection.Output = output;
        input.Connection = connection;
        input.ParentOutput = output;
        output.Connections.Add(connection);
        output.ChildInputs.Add(input);
        connection.EndingWire = endingWire;
        connection.StartingWire = startingWire;

        if (isEditor) EditorStructureManager.Instance.Connections.Add(connection); // Re-adds connection for potential serialization

        // If the circuit is an input gate, do not pursue an update call.
        if (output.ParentCircuit.GetType() == typeof(InputGate)) return;

        Circuit.UpdateCircuit(input, output);
    }

    // Finalizes the current connection in progress.
    public static void Connect(Circuit.Input input, Circuit.Output output)
    {
        Instance.count = -1;
        Instance.currentConnection.Input = input;
        Instance.currentConnection.Output = output;
        Instance.currentConnection.Input.Connection = Instance.currentConnection;
        Instance.currentConnection.Output.Connections.Add(Instance.currentConnection);
        Instance.currentConnection.Output.ChildInputs.Add(input);
        Instance.currentConnection.EndingWire.name = "Ending Wire";
        Instance.OptimizeMeshes();
        EditorStructureManager.Instance.Connections.Add(Instance.currentConnection); // Adds connection for potential serialization
        Instance.currentConnection = null; Instance.currentWire = null;
        Instance.cancelled = true;
        Circuit.UpdateCircuit(input, output);
    }

    /// <summary>
    /// Removes a connection from the editor scene.
    /// </summary>
    /// <param name="connection"></param>
    public static void Disconnect(Connection connection)
    {
        EditorStructureManager.Instance.DisplaySavePrompt = true;
        EditorStructureManager.Instance.Connections.Remove(connection); // Removes connection for potential serialization
        Circuit.UpdateCircuit(false, connection.Input, null);
        connection.Input.Connection = null;
        connection.Output.Connections.Remove(connection);
        connection.Output.ChildInputs.Remove(connection.Input);
        Destroy(connection.gameObject);
    }

    /// <summary>
    /// Updates all wire materials pertaining to a connection, if applicable.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="powered"></param>
    public static void UpdateConnectionMaterial(Connection connection, bool powered)
    {
        // If there is an optimized mesh in the wire, update it.
        if (connection.GetComponent<MeshRenderer>() != null) connection.GetComponent<MeshRenderer>().material = powered ? Instance.poweredMaterial : Instance.unpoweredMaterial;

        // If there is a starting mesh in the wire, update it.
        if (connection.StartingWire != null) connection.StartingWire.GetComponentInChildren<MeshRenderer>().material = powered ? Instance.poweredMaterial : Instance.unpoweredMaterial;

        // If there is an ending mesh in the wire, update it.
        if (connection.EndingWire != null) connection.EndingWire.GetComponentInChildren<MeshRenderer>().material = powered ? Instance.poweredMaterial : Instance.unpoweredMaterial;
    }

    /// <summary>
    /// Begins the connection process at the specified position.
    /// </summary>
    /// <param name="pos"></param>
    public void BeginConnectionProcess(Vector3 pos)
    {
        count = 0;
        cancelled = false;
        currentConnection = InstantiateConnection();
        InstantiateWire(currentConnection, pos);
        currentConnection.StartingWire = currentWire;
        currentWire.name = "Starting Wire";
        enabled = true; // Enables frame-by-frame update calls from Unity
    }

    /// <summary>
    /// Cancels the current connection process.
    /// </summary>
    public void CancelConnectionProcess()
    {
        count = -1;
        cancelled = true;
        Destroy(currentConnection.gameObject);
    }

    /// <summary>
    /// Creates a new wire at the specified position for the given connection.
    /// </summary>
    /// <param name="connection">The connection this wire is a part of.</param>
    /// <param name="a">The startring position to instantiate this wire at.</param>
    private void InstantiateWire(Connection connection, Vector3 a)
    {
        currentWire = Instantiate(wireReference, connection.transform);
        currentPos = new Vector3(a.x, GridMaintenance.Instance.GridHeight, a.z);
        currentWire.transform.position = currentPos;
        currentWire.SetActive(false);
    }

    /// <summary>
    /// Specific signature of <seealso cref="UpdatePosition(GameObject, Vector3, Vector3, bool)"/> under which isCentered is always false.
    /// </summary>
    /// <param name="wire">The wire to move.</param>
    /// <param name="a">The starting position.</param>
    /// <param name="b">The ending position.</param>
    public static void UpdatePosition(GameObject wire, Vector3 a, Vector3 b) { UpdatePosition(wire, a, b, false); }

    /// <summary>
    /// Updates the start and end positions of the specified wire.
    /// </summary>
    /// <param name="wire">The wire to move.</param>
    /// <param name="a">The starting position.</param>
    /// <param name="b">The ending position.</param>
    /// <param name="isCentered">Whether the wire should be centered.</param>
    public static void UpdatePosition(GameObject wire, Vector3 a, Vector3 b, bool isCentered)
    {
        // If the wire is centered, then startingWire == endingWire and it must be positioned differently.
        if (isCentered) wire.transform.position = (a + b) / 2;

        wire.transform.localScale = new Vector3(1, 1, (a - b).magnitude);
        wire.SetActive(wire.transform.localScale.z != 0);
        wire.transform.LookAt(b);

        // Ensures the height of the wire does not exceed the global grid height
        Vector3 temp = wire.transform.position;

        temp.y = GridMaintenance.Instance.GridHeight;
        wire.transform.position = temp;
    }

    /// <summary>
    /// Optimizes all non-starting and non-ending wire meshes by merging them together into a single mesh.
    /// </summary>
    private void OptimizeMeshes()
    {
        // There is a single wire in the connection
        if (currentConnection.StartingWire == currentConnection.EndingWire)
        {
            Destroy(currentWire);

            // If there is a single wire in the connection, it must be centered so UpdatePosition() can work properly.
            currentConnection.StartingWire.transform.position = (currentConnection.Input.Transform.position + currentConnection.Output.Transform.position) / 2;

            // Furthermore, the pivot must be altered.
            currentConnection.StartingWire.transform.GetChild(0).transform.localPosition = Vector3.back * 0.5f;

            // Ensures the height of the wire does not exceed the global grid height
            Vector3 temp = currentConnection.StartingWire.transform.position;

            temp.y = GridMaintenance.Instance.GridHeight;
            currentConnection.StartingWire.transform.position = temp;
            return;
        }

        // Ensures the starting wire behaves properly with the UpdatePosition() method
        currentConnection.StartingWire.transform.position = startingWirePos;
        currentConnection.StartingWire.transform.eulerAngles += Vector3.up * 180;

        // Checks to see whether there are exactly 3 wires.
        // 2 of them are the starting and ending wires (cannot be merged into a mesh) and the third wire is the placement wire, which will be deleted regardless.
        if (currentConnection.transform.childCount == 3)
        {
            Destroy(currentWire);
            return;
        }
        
        // Begins the mesh combination process
        List<CombineInstance> combineInstances = new List<CombineInstance>();

        foreach (Transform child in currentConnection.transform)
        {
            GameObject childObj = child.gameObject;

            if (childObj == currentConnection.StartingWire || childObj == currentConnection.EndingWire || childObj == currentWire) continue;

            MeshFilter meshFilter = childObj.GetComponentInChildren<MeshFilter>();
            CombineInstance combineInstance = new CombineInstance();

            combineInstance.mesh = meshFilter.mesh;
            combineInstance.transform = meshFilter.transform.localToWorldMatrix;

            combineInstances.Add(combineInstance);
        }

        Mesh combinedMesh = new Mesh();

        combinedMesh.CombineMeshes(combineInstances.ToArray());

        // Deletes the original instances of the unmerged meshes.
        foreach (Transform child in currentConnection.transform)
        {
            GameObject childObj = child.gameObject;

            if (childObj == currentConnection.StartingWire || childObj == currentConnection.EndingWire) continue;

            Destroy(childObj);
        }

        MeshFilter combinedMeshFilter = currentConnection.gameObject.AddComponent<MeshFilter>();

        currentConnection.gameObject.AddComponent<MeshRenderer>();
        combinedMeshFilter.mesh = combinedMesh;
        currentConnection.gameObject.layer = 11;
        currentConnection.gameObject.AddComponent<MeshCollider>();
    }

    /// <summary>
    /// Creates a new connection GameObject.
    /// </summary>
    /// <returns>The connection component of a newly instantiated GameObject.</returns>
    private Connection InstantiateConnection() { return new GameObject("Connection").AddComponent<Connection>(); }

    // Getter methods
    public static CircuitConnector Instance { get { return instance; } }

    public Connection CurrentConnection { get { return currentConnection; } }
}