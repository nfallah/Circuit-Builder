using System;
using UnityEngine;

[Serializable]
public class CircuitIdentifier
{
    public enum CircuitType { CUSTOM_CIRCUIT, INPUT_GATE, DISPLAY, BUFFER, AND_GATE, NAND_GATE, NOR_GATE, NOT_GATE, OR_GATE, XOR_GATE }

    [SerializeField] CircuitType circuitType;

    [SerializeField] int previewStructureID = -1;

    [SerializeField] Vector2 circuitLocation;

    public CircuitIdentifier(Circuit circuit)
    {
        Vector3 pos = circuit.PhysicalObject.transform.position;

        circuitType = CircuitToCircuitType(circuit);
        circuitLocation = new Vector2(pos.x, pos.z);

        if (circuitType == CircuitType.CUSTOM_CIRCUIT) previewStructureID = ((CustomCircuit)circuit).PreviewStructure.ID;
    }

    public static Circuit RestoreCircuit(CircuitIdentifier circuitIdentifier)
    {
        Vector3 pos = circuitIdentifier.circuitLocation;

        switch (circuitIdentifier.circuitType)
        {
            case CircuitType.CUSTOM_CIRCUIT:
                return new CustomCircuit(MenuSetupManager.Instance.PreviewStructures[MenuSetupManager.Instance.PreviewStructureIDs.IndexOf(circuitIdentifier.previewStructureID)], pos);
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