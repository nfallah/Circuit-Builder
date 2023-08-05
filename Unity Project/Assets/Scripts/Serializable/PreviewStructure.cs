using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PreviewStructure contains all serializable values to restore a preview scene.
/// </summary>
[Serializable]
public class PreviewStructure
{
    /// <summary>
    /// The list of circuit identifiers pertaining to each circuit within the scene.
    /// </summary>
    [SerializeField]
    List<CircuitIdentifier> circuits = new List<CircuitIdentifier>();

    /// <summary>
    /// The unique ID assigned to this preview structure.<br/><br/>
    /// Functionally, this ID is utilized to access the specific folder under which the connection and save information of the preview structure is.
    /// </summary>
    [SerializeField]
    int id;

    /// <summary>
    /// The order in which empty inputs and outputs were selected by the user.<br/><br/>
    /// Functionally, a visualized custom circuit will output these inputs and outputs in their selected order (bottom to top).
    /// </summary>
    [SerializeField]
    List<int> inputOrders,
        outputOrders;

    /// <summary>
    /// Identifying list of connections that exist within the custom circuit.
    /// </summary>
    [SerializeField]
    List<InternalConnection> connections;

    /// <summary>
    /// Name of the custom circuit.
    /// </summary>
    [SerializeField]
    string name;

    /// <summary>
    /// The corresponding user-assigned label for each empty input/output.
    /// </summary>
    [SerializeField]
    List<string> inputLabels,
        outputLabels;

    /// <summary>
    /// Location of the camera within the editor scene.
    /// </summary>
    [SerializeField]
    Vector3 cameraLocation;

    public PreviewStructure(string name) { this.name = name; }

    /// <summary>
    /// Internal class utilized to obtain the custom circuit ID via in-scene raycasting.
    /// </summary>
    public class PreviewStructureReference : MonoBehaviour
    {
        private int id;

        public int ID { get { return id; } set { id = value; } }
    }

    // Getter and setter methods
    public List<CircuitIdentifier> Circuits { get { return circuits; } set { circuits = value; } }

    public int ID { get { return id; } set { id = value; } }

    public List<int> InputOrders { get { return inputOrders; } set { inputOrders = value; } }

    public List<int> OutputOrders { get { return outputOrders; } set { outputOrders = value; } }

    public List<InternalConnection> Connections { get { return connections; } set { connections = value; } }

    public List<string> InputLabels { get { return inputLabels; } set { inputLabels = value; } }

    public List<string> OutputLabels { get { return outputLabels; } set { outputLabels = value; } }

    public Vector3 CameraLocation { get { return cameraLocation; } set { cameraLocation = value; } }

    // Getter method
    public string Name { get { return name; } }
}