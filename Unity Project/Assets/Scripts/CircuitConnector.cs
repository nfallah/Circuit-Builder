using System;
using System.Collections.Generic;
using UnityEngine;

public class CircuitConnector : MonoBehaviour
{
    private static CircuitConnector instance;

    [SerializeField] GameObject wireReference;

    [SerializeField] Material poweredMaterial, unpoweredMaterial;

    private bool cancelled, currentPowerStatus;

    private Connection currentConnection;

    private GameObject currentWire;

    private Vector3 currentPos;

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

        UpdatePosition(currentWire, currentPos, Coordinates.Instance.GridPos);

        if (Input.GetMouseButtonDown(0) && currentWire.activeSelf)
        {
            currentConnection.EndingWire = currentWire;
            InstantiateWire(currentConnection, Coordinates.Instance.GridPos);
        }
    }

    public static void Connect(Circuit.Input input, Circuit.Output output)
    {
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
    private void UpdatePosition(GameObject wire, Vector3 a, Vector3 b)
    {
        wire.transform.localScale = new Vector3(1, 1, (a - b).magnitude);
        wire.SetActive(wire.transform.localScale.z != 0);
        wire.transform.LookAt(b);
    }

    private void OptimizeMeshes()
    {
        if (currentConnection.StartingWire == currentConnection.EndingWire) return;

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
}