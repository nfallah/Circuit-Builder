using UnityEngine;
using UnityEngine.EventSystems;

public class BehaviorManager : MonoBehaviour
{
    private enum GameState { GRID_HOVER, CIRCUIT_MOVEMENT, CIRCUIT_PLACEMENT, IO_HOVER, IO_PRESS, USER_INTERFACE, WIRE_HOVER, WIRE_PRESS }

    private GameState gameState;

    private void Update()
    {
        gameState = UpdateGameState();
        ExecuteGameState();
    }

    private GameState UpdateGameState()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return GameState.USER_INTERFACE;
        }

        Ray ray = CameraMovement.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hitInfo)) return GameState.GRID_HOVER;

        GameObject hitObject = hitInfo.transform.gameObject;

        if (gameState == GameState.IO_HOVER && Input.GetMouseButtonDown(0))
        {
            return GameState.IO_PRESS;
        }

        if (hitObject.layer == 9 || hitObject.layer == 10)
        {
            return GameState.IO_HOVER;
        }

        if (hitObject.layer == 8 && Input.GetMouseButton(0))
        {
            return GameState.CIRCUIT_MOVEMENT;
        }

        if (gameState == GameState.WIRE_HOVER && Input.GetMouseButtonDown(0))
        {
            return GameState.WIRE_PRESS;
        }

        if (hitObject.layer == 11)
        {
            return GameState.WIRE_HOVER;
        }

        return GameState.GRID_HOVER;
    }

    private void ExecuteGameState()
    {

    }
}