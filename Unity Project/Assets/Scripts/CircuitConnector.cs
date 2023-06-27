using System;
using UnityEngine;

public class CircuitConnector : MonoBehaviour
{
    private static CircuitConnector instance;

    [SerializeField] GameObject wireReference;

    [SerializeField] Material poweredMaterial, unpoweredMaterial;

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
    }

    private void Start()
    {
        Circuit circuit = new NAndGate();
        CircuitVisualizer.Instance.VisualizeCircuit(circuit);
        BeginConnection(circuit.Outputs[0].Transform.position);
    }

    private void Update()
    {
        UpdatePosition(currentWire, currentPos, Coordinates.Instance.GridPos);

        if (Input.GetMouseButtonDown(0) && currentWire.activeSelf)
        {
            InstantiateWire(currentConnection, Coordinates.Instance.GridPos);
        }
    }

    public void BeginConnection(Vector3 pos)
    {
        currentConnection = InstantiateConnection();
        InstantiateWire(currentConnection, pos);
        currentConnection.StartingWire = currentWire;
    }

    private void InstantiateWire(Connection connection, Vector3 a)
    {
        currentWire = Instantiate(wireReference, connection.transform);
        currentWire.name = "Wire";
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

    public static void Connect(Connection connection)
    {
        Circuit.UpdateCircuit(connection.Input, connection.Output);
        connection.Input.Connection = connection;
        connection.Output.Connections.Add(connection);
        connection.EndingWire = Instance.currentWire;
    }

    public static void Disconnect(Connection connection)
    {
        Circuit.UpdateCircuit(false, connection.Input, null);
        connection.Input.Connection = null;
        connection.Output.Connections.Remove(connection);
        Destroy(connection.gameObject);

    }

    public Connection InstantiateConnection()
    {
        return new GameObject("Connection").AddComponent<Connection>();
    }

    public static CircuitConnector Instance { get { return instance; } }
}