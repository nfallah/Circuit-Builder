using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionSerializerRestorer
{
    public CircuitConnectorIdentifier circuitConnectorIdentifier { get; private set; }

    public GameObject startingWire { get; private set; }

    public GameObject endingWire { get; private set; }

    public GameObject parentObject { get; private set; }

    public ConnectionSerializerRestorer(CircuitConnectorIdentifier circuitConnectorIdentifier, GameObject startingWire, GameObject endingWire, GameObject parentObject)
    {
        this.circuitConnectorIdentifier= circuitConnectorIdentifier;
        this.startingWire = startingWire;
        this.endingWire = endingWire;
        this.parentObject = parentObject;
    }
}