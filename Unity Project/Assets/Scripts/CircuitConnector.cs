using System;
using System.Collections.Generic;
using UnityEngine;

public class CircuitConnector : MonoBehaviour
{
    private static CircuitConnector instance;

    [SerializeField] GameObject wireReference;

    [SerializeField] Material poweredMaterial, unpoweredMaterial;

    private bool cancelled, currentPowerStatus;

    private int count;

    private Connection currentConnection;

    private GameObject currentWire;

    private Vector3 startingWirePos, currentPos;

    public class Connection : MonoBehaviour
    {
        private Circuit.Input input;

        private Circuit.Output output;

        private GameObject endingWire, startingWire;

        public GameObject EndingWire { get { return endingWire; } set { endingWire = value; } }

        public GameObject StartingWire { get { return startingWire; } set { startingWire = value; } }

        public Circuit.Input Input { get { return input; } set { input = value; } }

        public Circuit.Output Output { get { return output; } set { output = value; } }
    }

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

    private void Update()
    {
        if (cancelled)
        {
            cancelled = enabled = false;
            return;
        }

        bool staringAtIO = Physics.Raycast(CameraMovement.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo) && hitInfo.transform.gameObject.layer == BehaviorManager.Instance.IOLayerCheck;
        Vector3 pos = staringAtIO ? hitInfo.transform.position : Coordinates.Instance.ModePos;

        pos.y = GridMaintenance.Instance.GridHeight;
        UpdatePosition(currentWire, currentPos, pos);

        if (Input.GetMouseButtonDown(0) && currentWire.activeSelf && BehaviorManager.Instance.CurrentStateType != BehaviorManager.StateType.PAUSED)
        {
            count++;

            if (count == 2)
            {
                startingWirePos = currentPos;
            }

            currentConnection.EndingWire = currentWire;
            InstantiateWire(currentConnection, Coordinates.Instance.ModePos);
        }
    }

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
        Instance.currentConnection = null; Instance.currentWire = null;
        Instance.cancelled = true;
        Circuit.UpdateCircuit(input, output);
    }

    public static void Disconnect(Connection connection)
    {
        Circuit.UpdateCircuit(false, connection.Input, null);
        connection.Input.Connection = null;
        connection.Output.Connections.Remove(connection);
        connection.Output.ChildInputs.Remove(connection.Input);
        Destroy(connection.gameObject);
    }

    public static void UpdateConnectionMaterial(Connection connection, bool powered)
    {
        if (connection.GetComponent<MeshRenderer>() != null) connection.GetComponent<MeshRenderer>().material = powered ? Instance.poweredMaterial : Instance.unpoweredMaterial;

        connection.EndingWire.GetComponentInChildren<MeshRenderer>().material = powered ? Instance.poweredMaterial : Instance.unpoweredMaterial;
        connection.StartingWire.GetComponentInChildren<MeshRenderer>().material = powered ? Instance.poweredMaterial : Instance.unpoweredMaterial;
    }

    public void BeginConnectionProcess(bool currentPowerStatus, Vector3 pos)
    {
        count = 0;
        cancelled = false;
        this.currentPowerStatus = currentPowerStatus;
        currentConnection = InstantiateConnection();
        InstantiateWire(currentConnection, pos);
        currentConnection.StartingWire = currentWire;
        currentWire.name = "Starting Wire";
        enabled = true;
    }

    public void CancelConnectionProcess()
    {
        count = -1;
        cancelled = true;
        Destroy(currentConnection.gameObject);
    }

    private void InstantiateWire(Connection connection, Vector3 a)
    {
        currentWire = Instantiate(wireReference, connection.transform);

        if (currentPowerStatus) currentWire.GetComponentInChildren<MeshRenderer>().material = poweredMaterial;

        currentPos = new Vector3(a.x, GridMaintenance.Instance.GridHeight, a.z);
        currentWire.transform.position = currentPos;
        currentWire.SetActive(false);
    }

    // Utilizes an existing wire and updates its start and end positions
    public static void UpdatePosition(GameObject wire, Vector3 a, Vector3 b)
    {
        UpdatePosition(wire, a, b, false);
    }

    public static void UpdatePosition(GameObject wire, Vector3 a, Vector3 b, bool isCentered)
    {
        if (isCentered) wire.transform.position = (a + b) / 2;
        wire.transform.localScale = new Vector3(1, 1, (a - b).magnitude);
        wire.SetActive(wire.transform.localScale.z != 0);
        wire.transform.LookAt(b);

        // Failsafe that ensures the height of the wire does not exceed the global grid height (in case floats are not represented accurately and there is slight rotation)
        Vector3 temp = wire.transform.position;

        temp.y = GridMaintenance.Instance.GridHeight;
        wire.transform.position = temp;
    }

    private void OptimizeMeshes()
    {
        if (currentConnection.StartingWire == currentConnection.EndingWire)
        {
            Destroy(currentWire);
            currentConnection.StartingWire.transform.position = (currentConnection.Input.Transform.position + currentConnection.Output.Transform.position) / 2;
            currentConnection.StartingWire.transform.GetChild(0).transform.localPosition = Vector3.back * 0.5f;

            /* Ensures the height of the wire does not exceed the global grid height, keeping raycasting working as intended.
             * This would otherwise happen as the position of the starting wire is set to the midpoint of the input and output of the connection, which are slightly above the grid height to allow for proper raycasting.
             * Therefore the height must be manually lowered.
             */
            Vector3 temp = currentConnection.StartingWire.transform.position;

            temp.y = GridMaintenance.Instance.GridHeight;
            currentConnection.StartingWire.transform.position = temp;
            return;
        }

        currentConnection.StartingWire.transform.position = startingWirePos;
        currentConnection.StartingWire.transform.eulerAngles += Vector3.up * 180;

        // Checks to see whether there are exactly 3 wires, as 2 of them are the starting and ending wires (cannot be merged into a mesh) and the third wire is the placement wire, which will be deleted regardless.
        if (currentConnection.transform.childCount == 3)
        {
            Destroy(currentWire);
            return;
        }

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

        foreach (Transform child in currentConnection.transform)
        {
            GameObject childObj = child.gameObject;

            if (childObj == currentConnection.StartingWire || childObj == currentConnection.EndingWire) continue;

            Destroy(childObj);
        }

        MeshFilter combinedMeshFilter = currentConnection.gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = currentConnection.gameObject.AddComponent<MeshRenderer>();

        combinedMeshFilter.mesh = combinedMesh;
        meshRenderer.material = currentPowerStatus ? poweredMaterial : unpoweredMaterial;
        currentConnection.gameObject.layer = 11;
        currentConnection.gameObject.AddComponent<MeshCollider>();
    }

    private Connection InstantiateConnection()
    {
        return new GameObject("Connection").AddComponent<Connection>();
    }

    public static CircuitConnector Instance { get { return instance; } }

    public Connection CurrentConnection { get { return currentConnection; } }
}