using System;
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

    private int ioLayerCheck;

    private GameState gameState, unpausedGameState;

    private StateType stateType, unpausedStateType;

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
            CursorManager.SetMouseTexture(true);
            return GameState.USER_INTERFACE;
        }

        // Previous state was not UI, now is
        if (gameState == GameState.USER_INTERFACE)
        {
            gameState = unpausedGameState;
            stateType = unpausedStateType;
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
            stateType = StateType.LOCKED;
            CursorManager.SetMouseTexture(false);
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
        if (gameState == GameState.IO_HOVER && Input.GetMouseButtonDown(0))
        {
            IOPress(hitObject);
            stateType = StateType.LOCKED;
            CursorManager.SetMouseTexture(true);
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

    private void IOPress(GameObject hitObject)
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

    private void GameStateListener()
    {
        switch (gameState)
        {
            case GameState.IO_PRESS:
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

                        CircuitConnector.Connect(currentInput, currentOutput);
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
                    return;
                }

                break;
        }
    }

    // Getter methods
    public static BehaviorManager Instance { get { return instance; } }

    public GameState CurrentGameState { get { return gameState; } }

    public StateType CurrentStateType { get { return stateType; } }
}