using UnityEngine;
using UnityEngine.EventSystems;

public class BehaviorManager : MonoBehaviour
{
    private enum GameState { GRID_HOVER, CIRCUIT_MOVEMENT, CIRCUIT_PLACEMENT, IO_HOVER, IO_PRESS, USER_INTERFACE, WIRE_HOVER, WIRE_PRESS }

    private enum StateType { UNRESTRICTED, LOCKED, PAUSED }

    [SerializeField] KeyCode cancelKey;

    private Circuit.Input currentInput;

    private Circuit.Output currentOutput;

    private int ioLayerCheck;

    private GameState gameState, unpausedGameState;

    private StateType stateType, unpausedStateType;

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
        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (gameState == GameState.USER_INTERFACE) return gameState;

            unpausedGameState = gameState;
            unpausedStateType = stateType;
            stateType = StateType.PAUSED;
            return GameState.USER_INTERFACE;
        }

        if (gameState == GameState.USER_INTERFACE)
        {
            gameState = unpausedGameState;
            stateType = unpausedStateType;
        }

        if (stateType == StateType.LOCKED) return gameState;

        Ray ray = CameraMovement.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            stateType = StateType.UNRESTRICTED;
            return GameState.GRID_HOVER;
        }

        GameObject hitObject = hitInfo.transform.gameObject;

        if (hitObject.layer == 8 && Input.GetMouseButton(0))
        {
            stateType = StateType.LOCKED;
            return GameState.CIRCUIT_MOVEMENT;
        }

        if (gameState == GameState.IO_HOVER && Input.GetMouseButtonDown(0))
        {
            IOPress(hitObject);
            stateType = StateType.LOCKED;
            return GameState.IO_PRESS;
        }

        if (hitObject.layer == 9 || hitObject.layer == 10)
        {
            stateType = StateType.UNRESTRICTED;
            return GameState.IO_HOVER;
        }

        if (gameState == GameState.WIRE_HOVER && Input.GetMouseButtonDown(0))
        {
            stateType = StateType.LOCKED;
            return GameState.WIRE_PRESS;
        }

        if (hitObject.layer == 11)
        {
            stateType = StateType.UNRESTRICTED;
            return GameState.WIRE_HOVER;
        }

        stateType = StateType.UNRESTRICTED;
        return GameState.GRID_HOVER;
    }

    private void IOPress(GameObject hitObject)
    {
        bool powerStatus;
        Vector3 startingPos;

        // Input layer
        if (hitObject.layer == 9)
        {
            currentInput = hitObject.GetComponent<CircuitVisualizer.InputReference>().Input;
            powerStatus = currentInput.Powered;
            ioLayerCheck = 10;
            startingPos = currentInput.Transform.position;
        }

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
                if (Input.GetMouseButtonDown(0) && Physics.Raycast(CameraMovement.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo) && hitInfo.transform.gameObject.layer == ioLayerCheck)
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

                    CircuitConnector.Connect(currentInput, currentOutput);
                    stateType = StateType.UNRESTRICTED;
                    currentInput = null; currentOutput = null;
                }

                else if (Input.GetKeyDown(cancelKey))
                {
                    CircuitConnector.Instance.CancelConnectionProcess();
                    stateType = StateType.UNRESTRICTED;
                    currentInput = null; currentOutput = null;
                }

                break;
        }
    }
}