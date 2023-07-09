using System;
using UnityEngine;

[Serializable]
public class StartingCircuitIdentifier
{
    public enum CircuitType { INPUT_GATE, DISPLAY, BUFFER, AND_GATE, NAND_GATE, NOR_GATE, NOT_GATE, OR_GATE, XOR_GATE }

    [SerializeField] CircuitType startingCircuitType;

    [SerializeField] Vector2 startingCircuitLocation;

    public StartingCircuitIdentifier(Circuit circuit)
    {
        Vector3 pos = circuit.PhysicalObject.transform.position;

        startingCircuitType = CircuitToCircuitType(circuit);
        startingCircuitLocation = new Vector2(pos.x, pos.z);
    }
    
    public static Circuit RestoreCircuit(StartingCircuitIdentifier startingCircuitIdentifier)
    {
        Vector3 pos = startingCircuitIdentifier.StartingCircuitLocation;

        switch (startingCircuitIdentifier.StartingCircuitType)
        {
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
                throw new Exception("Invalid starting circuit type.");
        }
    }

    private static CircuitType CircuitToCircuitType(Circuit circuit)
    {
        Type type = circuit.GetType();

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

    // Getter methods
    public CircuitType StartingCircuitType { get { return startingCircuitType; } }

    public Vector2 StartingCircuitLocation { get { return startingCircuitLocation; } }
}