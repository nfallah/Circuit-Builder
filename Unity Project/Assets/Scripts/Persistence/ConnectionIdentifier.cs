using UnityEngine;

public class ConnectionIdentifier : MonoBehaviour
{
    [SerializeField] bool isOptimized = false;

    [SerializeField] GameObject endingWire, startingWire;

    [SerializeField] Vector2 inputCoordinate, outputCoordinate;

    // Getter and setter methods
    public bool IsOptimized { get { return isOptimized; } set { isOptimized = value; } }

    public GameObject EndingWire { get { return endingWire; } set { endingWire = value; } }

    public GameObject StartingWire { get { return startingWire; } set { startingWire = value; } }

    public Vector2 InputCoordinate { get { return inputCoordinate; } set { inputCoordinate = value; } }

    public Vector2 OutputCoordinate { get { return outputCoordinate; } set { outputCoordinate = value; } }
}