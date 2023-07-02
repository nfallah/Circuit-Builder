using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BehaviorManager : MonoBehaviour
{
    private static BehaviorManager instance;

    public enum GameState { GRID_HOVER, CIRCUIT_HOVER, CIRCUIT_MOVEMENT, CIRCUIT_PLACEMENT, IO_HOVER, IO_PRESS, USER_INTERFACE, WIRE_HOVER, WIRE_PRESS }

    public enum StateType { UNRESTRICTED, LOCKED, PAUSED }

    [SerializeField] KeyCode cancelKey;

    private Circuit.Input currentInput;

    private Circuit.Output currentOutput;

    private bool ioLMB;

    private int ioLayerCheck;

    private Circuit currentCircuit;

    private GameState gameState, unpausedGameState;

    private StateType stateType, unpausedStateType;

    private Vector3 deltaPos, prevDeltaPos, startingOffset, endingOffset, startingPos;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("BehaviorManager instance already established; terminating.");
        }

        instance = this;
    }

    private void Start()
    {
        new InputGate();
        new AndGate();
        new AndGate(new Vector2(20, 10));
        new NotGate(new Vector2(30, 0));
    }

    private void LateUpdate()
    {
        gameState = UpdateGameState();
        GameStateListener();
    }

    private GameState UpdateGameState()
    {
        // Current state is UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (gameState == GameState.USER_INTERFACE) return gameState; // Last state was UI

            // The UI state pauses the previous state rather than overriding it
            unpausedGameState = gameState;
            unpausedStateType = stateType;
            stateType = StateType.PAUSED;
            Cursor.visible = true;
            CursorManager.SetMouseTexture(true);
            return GameState.USER_INTERFACE;
        }

        // Previous game state was UI, but current state is not (essentially conditions for unpausing)
        if (gameState == GameState.USER_INTERFACE)
        {
            gameState = unpausedGameState;
            stateType = unpausedStateType;
            Cursor.visible = unpausedGameState != GameState.CIRCUIT_MOVEMENT;
        }

        if (stateType == StateType.LOCKED) return gameState; // Locked states must change manually, not automatically

        Ray ray = CameraMovement.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            stateType = StateType.UNRESTRICTED;
            CursorManager.SetMouseTexture(true);
            return GameState.GRID_HOVER;
        }

        GameObject hitObject = hitInfo.transform.gameObject;

        // Mouse is on top of a circuit & LMB has been pressed
        if (gameState == GameState.CIRCUIT_HOVER && Input.GetMouseButton(0))
        {
            currentCircuit = hitObject.GetComponentInParent<CircuitReference>().Circuit;
            CircuitPress();
            stateType = StateType.LOCKED;
            CursorManager.SetMouseTexture(false);
            Cursor.visible = false;
            return GameState.CIRCUIT_MOVEMENT;
        }

        // Mouse is on top of a circuit
        if (hitObject.layer == 8)
        {
            stateType = StateType.UNRESTRICTED;
            CursorManager.SetMouseTexture(false);
            return GameState.CIRCUIT_HOVER;
        }

        // Mouse is on top of any input & LMB has been pressed
        if (gameState == GameState.IO_HOVER && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))
        {
            ioLMB = Input.GetMouseButtonDown(0);
            stateType = StateType.LOCKED;

            if (ioLMB)
            {
                IOLMBPress(hitObject);
                CursorManager.SetMouseTexture(true);
            }
            
            else
            {
                IOAlternatePress(hitObject);
            }
            return GameState.IO_PRESS;
        }

        // Mouse is on top of any input or output
        if (hitObject.layer == 9 || hitObject.layer == 10)
        {
            stateType = StateType.UNRESTRICTED;
            CursorManager.SetMouseTexture(false);
            return GameState.IO_HOVER;
        }

        // Mouse is on top of a wire & RMB has been pressed
        if (gameState == GameState.WIRE_HOVER && Input.GetMouseButtonDown(1))
        {
            stateType = StateType.LOCKED;
            CursorManager.SetMouseTexture(true);
            WirePress(hitObject);
            return GameState.WIRE_PRESS;
        }

        // Mouse is on top of a wire
        if (hitObject.layer == 11)
        {
            stateType = StateType.UNRESTRICTED;
            CursorManager.SetMouseTexture(false);
            return GameState.WIRE_HOVER;
        }

        stateType = StateType.UNRESTRICTED;
        CursorManager.SetMouseTexture(true);
        return GameState.GRID_HOVER;
    }

    private void IOLMBPress(GameObject hitObject)
    {
        bool powerStatus;
        Vector3 startingPos;

        // Input layer was pressed; next press should be on an output layer
        if (hitObject.layer == 9)
        {
            currentInput = hitObject.GetComponent<CircuitVisualizer.InputReference>().Input;
            powerStatus = currentInput.Powered;
            ioLayerCheck = 10;
            startingPos = currentInput.Transform.position;
        }

        // Output layer was pressed; next press should be on an input layer
        else
        {
            currentOutput = hitObject.GetComponent<CircuitVisualizer.OutputReference>().Output;
            powerStatus = currentOutput.Powered;
            ioLayerCheck = 9;
            startingPos = currentOutput.Transform.position;
        }

        CircuitConnector.Instance.BeginConnectionProcess(powerStatus, startingPos);
    }

    private void IOAlternatePress(GameObject hitObject)
    {
        if (Input.GetMouseButtonDown(2))
        {
            if (hitObject.layer == 10 && hitObject.GetComponentInParent<CircuitReference>().Circuit.GetType() == typeof(InputGate))
            {
                InputGate gate = (InputGate)hitObject.GetComponentInParent<CircuitReference>().Circuit;
                gate.Powered = !gate.Powered;
            }

        }

        // Input pressed
        else if (hitObject.layer == 9)
        {
            Circuit.Input input = hitObject.GetComponent<CircuitVisualizer.InputReference>().Input;

            // If there *is* a connection, disconnect it.
            if (input.Connection != null) CircuitConnector.Disconnect(input.Connection);
        }

        // Output pressed
        else
        {
            Circuit.Output output = hitObject.GetComponent<CircuitVisualizer.OutputReference>().Output;
            List<CircuitConnector.Connection> connections = new List<CircuitConnector.Connection>(output.Connections);

            foreach (CircuitConnector.Connection connection in connections) CircuitConnector.Disconnect(connection);
        }

        stateType = StateType.UNRESTRICTED;
    }

    private void WirePress(GameObject hitObject)
    {
        CircuitConnector.Connection connection;

        // Not starting or ending wire (aka the optimized mesh inbetween)
        if (hitObject.transform.parent == null)
        {
            connection = hitObject.GetComponent<CircuitConnector.Connection>();
            Destroy(hitObject.transform.gameObject);
        }

        // Starting and/or ending wire
        else
        {
            connection = hitObject.GetComponentInParent<CircuitConnector.Connection>();
            Destroy(hitObject.transform.parent.parent.gameObject);
        }

        CircuitConnector.Disconnect(connection);
        stateType = StateType.UNRESTRICTED;
    }

    private void CircuitPress()
    {
        Vector3 mousePos = Coordinates.Instance.MousePos;

        startingPos = currentCircuit.PhysicalObject.transform.position;
        endingOffset = startingOffset = mousePos;
    }

    private void GameStateListener()
    {
        switch (gameState)
        {
            case GameState.IO_PRESS:
                if (!ioLMB) return; // Means RMB was pressed, therefore not necessary to run this code
                if (Physics.Raycast(CameraMovement.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo) && hitInfo.transform.gameObject.layer == ioLayerCheck)
                {
                    // Input layer
                    if (ioLayerCheck == 9)
                    {
                        currentInput = hitInfo.transform.GetComponent<CircuitVisualizer.InputReference>().Input;
                    }

                    // Output layer
                    else
                    {
                        currentOutput = hitInfo.transform.GetComponent<CircuitVisualizer.OutputReference>().Output;
                    }

                    CursorManager.SetMouseTexture(false);

                    if (Input.GetMouseButtonDown(0))
                    {
                        // Disconnects the current connection to the input if there is one
                        if (currentInput.ParentOutput != null)
                        {
                            CircuitConnector.Disconnect(currentInput.Connection);
                        }

                        CircuitConnector.Connection connection = CircuitConnector.Instance.CurrentConnection;
                        CircuitConnector.Connect(currentInput, currentOutput);

                        /* Ensures that if the order of selection was not output -> input, the starting and ending wires are swapped with one another.
                         * This occurs as the starting wire is also associated with the input, hence the game objects are swapped to maintan this order.
                         */
                        if (ioLayerCheck == 10)
                        {
                            GameObject temp = connection.StartingWire;

                            connection.StartingWire = connection.EndingWire;
                            connection.EndingWire = temp;
                        }
                        
                        stateType = StateType.UNRESTRICTED;
                        currentInput = null; currentOutput = null;
                        return;
                    }
                }

                else
                {
                    CursorManager.SetMouseTexture(true);
                }

                // Cancels the placement process
                if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1))
                {
                    CircuitConnector.Instance.CancelConnectionProcess();
                    stateType = StateType.UNRESTRICTED;
                    currentInput = null; currentOutput = null;
                }

                break;

            case GameState.CIRCUIT_MOVEMENT:

                // Exit code
                if (!Input.GetMouseButton(0))
                {
                    stateType = StateType.UNRESTRICTED;
                    Cursor.visible = true;
                    currentCircuit = null;
                    return;
                }

                endingOffset = Coordinates.Instance.MousePos;
                prevDeltaPos = deltaPos;
                deltaPos = endingOffset - startingOffset + startingPos;

                if (Coordinates.Instance.CurrentSnappingMode == Coordinates.SnappingMode.GRID) deltaPos = Coordinates.NormalToGridPos(deltaPos);

                currentCircuit.PhysicalObject.transform.position = deltaPos;

                if (prevDeltaPos != deltaPos)
                {
                    foreach (Circuit.Input input in currentCircuit.Inputs)
                    {
                        if (input.Connection != null)
                        {
                            bool isCentered = input.Connection.EndingWire == input.Connection.StartingWire;
                            Vector3 fromPos = isCentered ? input.Connection.Output.Transform.position : input.Connection.EndingWire.transform.position;
                            CircuitConnector.UpdatePosition(input.Connection.EndingWire, fromPos, input.Transform.position, isCentered);
                        }
                    }

                    foreach (Circuit.Output output in currentCircuit.Outputs)
                    {
                        foreach (CircuitConnector.Connection connection in output.Connections)
                        {
                            bool isCentered = connection.EndingWire == connection.StartingWire;
                            Vector3 fromPos = isCentered ? connection.Input.Transform.position : connection.StartingWire.transform.position;
                            CircuitConnector.UpdatePosition(connection.StartingWire, fromPos, output.Transform.position, isCentered);
                        }
                    }
                }
                break;
            case GameState.WIRE_PRESS:
                break;
        }
    }

    // Getter methods
    public static BehaviorManager Instance { get { return instance; } }

    public GameState CurrentGameState { get { return gameState; } }

    public int IOLayerCheck { get { return ioLayerCheck; } }

    public StateType CurrentStateType { get { return stateType; } }
}