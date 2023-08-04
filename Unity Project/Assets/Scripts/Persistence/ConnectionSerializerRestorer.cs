using UnityEngine;

/// <summary>
/// ConnectionSerializerRestorer stores deserialized GameObject values from an external <see cref="ConnectionSerializer"/>.<br/><br/>
/// These stored values are then referenced by <see cref="CircuitVisualizer"/> as the assigned connection is restored to the scene.
/// </summary>
public class ConnectionSerializerRestorer
{
    /// <summary>
    /// The <see cref="CircuitConnectorIdentifier"/> pertaining to this connection.
    /// </summary>
    public CircuitConnectorIdentifier circuitConnectorIdentifier { get; private set; }

    /// <summary>
    /// The GameObject that has been created from the assigned <see cref="ConnectionSerializer.startingMesh"/>.
    /// </summary>
    public GameObject startingWire { get; private set; }

    /// <summary>
    /// The GameObject that has been created from the assigned <see cref="ConnectionSerializer.endingMesh"/>.
    /// </summary>
    public GameObject endingWire { get; private set; }

    /// <summary>
    /// The GameObject that has been created from the assigned <see cref="ConnectionSerializer.parentMesh"/>.
    /// </summary>
    public GameObject parentObject { get; private set; }

    /// <summary>
    /// Instantiates and assigns all relevant values extracted from a <see cref="ConnectionSerializer"/>.
    /// </summary>
    public ConnectionSerializerRestorer(CircuitConnectorIdentifier circuitConnectorIdentifier, GameObject startingWire, GameObject endingWire, GameObject parentObject)
    {
        this.circuitConnectorIdentifier= circuitConnectorIdentifier;
        this.startingWire = startingWire;
        this.endingWire = endingWire;
        this.parentObject = parentObject;
    }
}