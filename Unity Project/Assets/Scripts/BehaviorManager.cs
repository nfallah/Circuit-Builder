using UnityEngine;
using UnityEngine.EventSystems;

public class BehaviorManager : MonoBehaviour
{
    private enum GameState { GRID_HOVER, CIRCUIT_MOVEMENT, CIRCUIT_PLACEMENT, IO_HOVER, IO_PRESS, USER_INTERFACE, WIRE_HOVER, WIRE_PRESS }

    private enum StateType { UNRESTRICTED, LOCKED, PAUSED }

    private GameState gameState, unpausedGameState;

    private StateType stateType, unpausedStateType;

    private void Start()
    {
        new NAndGate();    
    }

    private void Update()
    {
        gameState = UpdateGameState();
        ExecuteGameState();
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

    private void ExecuteGameState()
    {
        switch (gameState)
        {
            case GameState.IO_PRESS:

                break;
        }
    }
}