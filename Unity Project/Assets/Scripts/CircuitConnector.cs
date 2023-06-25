using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitConnector : MonoBehaviour
{
    [SerializeField] GameObject wireReference;

    [SerializeField] Material poweredWire, unpoweredWire;

    private static CircuitConnector instance;

    public class Connection : MonoBehaviour
    {
        private GameObject wire;

        private Circuit.Input input;

        private Circuit.Output output;

        public GameObject Wire { get { return wire; } set { wire = value; } }

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

    // Utilized for instantly creating a new wire and connecting its input and output
    public void Connect(Circuit.Input input, Circuit.Output output)
    {
        Connect(NewConnection, input, output);
    }

    // Utilizes an existing connection (i.e. wire) and updates its input and output
    public void Connect(Connection connection, Circuit.Input input, Circuit.Output output)
    {
        Connect(connection, input.Transform.position, output.Transform.position);
        connection.Input = input;
        connection.Output = output;
    }

    // Utilizes an existing connection (i.e. wire) and updates its start and end positions
    public void Connect(Connection connection, Vector3 a, Vector3 b)
    {
        connection.Wire.transform.position = (a + b) / 2;
        connection.Wire.transform.localScale = new Vector3(1, 1, (a - b).magnitude);
        connection.Wire.transform.LookAt(b);
    }

    public Connection NewConnection
    {
        get
        {
            GameObject wire = Instantiate(wireReference);
            Connection connection = wire.AddComponent<Connection>();

            wire.name = "Wire";
            connection.Wire = wire;
            return connection;
        }
    }

    public static CircuitConnector Instance { get { return instance; } }
}