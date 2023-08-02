using System;
using System.IO;
using UnityEngine;

[Serializable]
public class ConnectionSerializer
{
    [SerializeField]
    bool singleWired;

    [SerializeField]
    CircuitConnectorIdentifier circuitConnectorIdentifier;

    [SerializeField]
    MeshSerializer startingMesh, endingMesh, parentMesh;

    private ConnectionSerializer() { }

    public static void SerializeConnection(CircuitConnector.Connection connection, CircuitConnectorIdentifier circuitConnectorIdentifier, string path)
    {
        ConnectionSerializer connectionSerializer = new ConnectionSerializer();

        connectionSerializer.circuitConnectorIdentifier = circuitConnectorIdentifier;
        connectionSerializer.startingMesh = new MeshSerializer(connection.StartingWire.transform.GetChild(0).GetChild(0).GetComponent<MeshFilter>().mesh, connection.StartingWire.transform);
        connectionSerializer.endingMesh = new MeshSerializer(connection.EndingWire.transform.GetChild(0).GetChild(0).GetComponent<MeshFilter>().mesh, connection.EndingWire.transform);
        connectionSerializer.singleWired = connection.StartingWire == connection.EndingWire;
        if (connection.GetComponent<MeshFilter>() != null) connectionSerializer.parentMesh = new MeshSerializer(connection.GetComponent<MeshFilter>().mesh, connection.transform);

        File.WriteAllText(path, JsonUtility.ToJson(connectionSerializer));
    }

    // Getter methods
    public bool SingleWired { get { return singleWired; } }

    public CircuitConnectorIdentifier CircuitConnectorIdentifier { get { return circuitConnectorIdentifier; } }

    public MeshSerializer StartingMesh { get { return startingMesh; } }

    public MeshSerializer EndingMesh { get { return endingMesh; } }

    public MeshSerializer ParentMesh { get { return parentMesh; } }
}