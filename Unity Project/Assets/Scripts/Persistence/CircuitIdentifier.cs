using System;
using UnityEngine;

/// <summary>
/// CircuitIdentifier provides circuit serialization and deserialization by storing relevant enum values.
/// </summary>
[Serializable]
public class CircuitIdentifier
{
    /// <summary>
    /// CircuitType is the serialized representation of any circuit located within a scene.
    /// </summary>
    public enum CircuitType { CUSTOM_CIRCUIT, INPUT_GATE, DISPLAY, BUFFER, AND_GATE, NAND_GATE, NOR_GATE, NOT_GATE, OR_GATE, XOR_GATE }

    /// <summary>
    /// The circuit type to be serialized by this CircuitIdentifier instance.
    /// </summary>
    [SerializeField]
    public CircuitType circuitType;

    /// <summary>
    /// Contains a valid custom circuit ID if the referenced CircuitType value is <seealso cref="CircuitType.CUSTOM_CIRCUIT"/>.
    /// </summary>
    [SerializeField]
    public int previewStructureID = -1;

    /// <summary>
    /// The location of the circuit within the scene.
    /// </summary>
    [SerializeField]
    Vector2 circuitLocation;

    /// <param name="circuit">The circuit to serialize.</param>
    public CircuitIdentifier(Circuit circuit)
    {
        Vector3 pos = circuit.PhysicalObject.transform.position;

        circuitType = CircuitToCircuitType(circuit);
        circuitLocation = new Vector2(pos.x, pos.z);

        // If the circuit is a custom one, store its ID
        if (circuitType == CircuitType.CUSTOM_CIRCUIT) previewStructureID = ((CustomCircuit)circuit).PreviewStructure.ID;
    }

    /// <summary>
    /// Instantiates and returns a <see cref="Circuit"/> based on the provided CircuitIdentifier (using default visibility settings).
    /// </summary>
    /// <param name="circuitIdentifier">The CircuitIdentifier to access and reference.</param>
    /// <returns>The instantiated <seealso cref="Circuit"/>.</returns>
    public static Circuit RestoreCircuit(CircuitIdentifier circuitIdentifier)
    {
        return RestoreCircuit(circuitIdentifier, true);
    }

    /// <summary>
    /// Instantiates and returns a <see cref="Circuit"/> based on the provided CircuitIdentifier.
    /// </summary>
    /// <param name="circuitIdentifier">The CircuitIdentifier to access and reference.</param>
    /// <param name="visible">False if the circuit is located inside of a custom circuit, otherwise true.</param>
    /// <returns>The instantiated <seealso cref="Circuit"/>.</returns>
    public static Circuit RestoreCircuit(CircuitIdentifier circuitIdentifier, bool visible)
    {
        Vector2 pos = visible ? circuitIdentifier.circuitLocation : Vector2.positiveInfinity;

        switch (circuitIdentifier.circuitType)
        {
            case CircuitType.CUSTOM_CIRCUIT:
                return new CustomCircuit(MenuSetupManager.Instance.PreviewStructures[MenuSetupManager.Instance.PreviewStructureIDs.IndexOf(circuitIdentifier.previewStructureID)], pos, visible);
            case CircuitType.INPUT_GATE:
                return new InputGate(pos);
            case CircuitType.DISPLAY:
                return new Display(pos);
            case CircuitType.BUFFER:
                return new Buffer(pos);
            case CircuitType.AND_GATE:
                return new AndGate(pos);
            case CircuitType.NAND_GATE:
                return new NAndGate(pos);
            case CircuitType.NOR_GATE:
                return new NOrGate(pos);
            case CircuitType.NOT_GATE:
                return new NotGate(pos);
            case CircuitType.OR_GATE:
                return new OrGate(pos);
            case CircuitType.XOR_GATE:
                return new XOrGate(pos);
            default:
                throw new Exception("Invalid circuit type.");
        }
    }

    /// <summary>
    /// Converts the provided <see cref="Circuit"/> to a valid <seealso cref="CircuitType"/> for serialization.
    /// </summary>
    /// <param name="circuit">The circuit to convert.</param>
    /// <returns>The converted <seealso cref="CircuitType"/>.</returns>
    private static CircuitType CircuitToCircuitType(Circuit circuit)
    {
        Type type = circuit.GetType();

        if (type == typeof(CustomCircuit)) return CircuitType.CUSTOM_CIRCUIT;

        if (type == typeof(InputGate)) return CircuitType.INPUT_GATE;

        if (type == typeof(Display)) return CircuitType.DISPLAY;

        if (type == typeof(Buffer)) return CircuitType.BUFFER;

        if (type == typeof(AndGate)) return CircuitType.AND_GATE;

        if (type == typeof(NAndGate)) return CircuitType.NAND_GATE;

        if (type == typeof(NOrGate)) return CircuitType.NOR_GATE;

        if (type == typeof(NotGate)) return CircuitType.NOT_GATE;

        if (type == typeof(OrGate)) return CircuitType.OR_GATE;

        if (type == typeof(XOrGate)) return CircuitType.XOR_GATE;

        throw new Exception("Invalid circuit type.");
    }
}