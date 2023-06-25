using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitConnector : MonoBehaviour
{
    [SerializeField] GameObject wireReference;

    [SerializeField] Material poweredWire, unpoweredWire;

    private static CircuitConnector instance;

    private Connection currentConnection;

    private GameObject currentWire;

    private Vector3 currentPos;

    public class Connection : MonoBehaviour
    {
        private Circuit.Input input;

        private Circuit.Output output;

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
        //enabled = false;
    }

    private void Start()
    {
        Circuit circuit = new NAndGate();
        CircuitVisualizer.Instance.VisualizeCircuit(circuit);
        BeginConnection(circuit.Outputs[0]);
    }

    private void Update()
    {
        UpdatePosition(currentWire, currentPos, Coordinates.Instance.GridPos);

        if (Input.GetMouseButtonDown(0))
        {
            InstantiateWire(currentConnection, Coordinates.Instance.GridPos);
        }
    }

    public void BeginConnection(Circuit.Input input)
    {
        currentConnection = InstantiateConnection();
        currentConnection.Input = input;
        InstantiateWire(currentConnection, input.Transform.position);
        input.Wire = currentWire;
        //enabled = true;
    }

    public void BeginConnection(Circuit.Output output)
    {
        currentConnection = InstantiateConnection();
        currentConnection.Output = output;
        InstantiateWire(currentConnection, output.Transform.position);
        output.Wire = currentWire;
        //enabled = true;
    }

    public void InstantiateWire(Connection connection, Vector3 a)
    {
        currentWire = Instantiate(wireReference, connection.transform);
        currentPos = new Vector3(a.x, 0, a.z); // change later
        currentWire.transform.position = currentPos;
    }

    // Utilizes an existing wire and updates its start and end positions
    public void UpdatePosition(GameObject wire, Vector3 a, Vector3 b)
    {
        wire.transform.localScale = new Vector3(1, 1, (a - b).magnitude);
        wire.transform.LookAt(b);
    }

    public Connection InstantiateConnection()
    {
        return new GameObject("Connection").AddComponent<Connection>();
    }

    public static CircuitConnector Instance { get { return instance; } }
}