using UnityEngine;

public class ConnectionIdentifier : MonoBehaviour
{
    [SerializeField] GameObject endingWire, startingWire;

    [SerializeField] CircuitConnectorIdentifier circuitConnectorIdentifier;

    // Getter and setter methods
    public GameObject EndingWire { get { return endingWire; } set { endingWire = value; } }

    public GameObject StartingWire { get { return startingWire; } set { startingWire = value; } }

    public CircuitConnectorIdentifier CircuitConnectorIdentifier { get { return circuitConnectorIdentifier; } set { circuitConnectorIdentifier = value; } }
}