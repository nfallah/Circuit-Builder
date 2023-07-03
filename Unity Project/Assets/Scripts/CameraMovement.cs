using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private static CameraMovement instance;

    [SerializeField] Camera playerCamera;

    [SerializeField] float movementSpeed, scrollSpeed, minHeight, maxHeight;

    [SerializeField] KeyCode upKey, downKey;

    private Vector3 mousePosCurrent; // Keeps track of the mouse position from the current frame

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("CameraMovement instance already established; terminating.");
        }

        instance = this;
    }

    private void Start()
    {
        ClampPos();
        mousePosCurrent = Coordinates.Instance.MousePos;
    }

    private void Update()
    {
        float x, y, z;

        Vector3 mousePosPrev = mousePosCurrent;

        mousePosCurrent = Coordinates.Instance.MousePos;

        // If the scene is paused, no movement can occur
        if (BehaviorManager.Instance.CurrentStateType == BehaviorManager.StateType.PAUSED) return;

        // Otherwise, movement features are allowed depending on whether the game is unrestricted or locked.
        if (Input.GetMouseButton(0) && BehaviorManager.Instance.CurrentStateType == BehaviorManager.StateType.UNRESTRICTED)
        {
            Vector3 mousePosDelta = mousePosPrev - mousePosCurrent;

            x = mousePosDelta.x;
            z = mousePosDelta.z;
        }

        else
        {
            // Obtains x and z axis values based on input
            x = Input.GetAxisRaw("Horizontal") * movementSpeed * Time.deltaTime;
            z = Input.GetAxisRaw("Vertical") * movementSpeed * Time.deltaTime;
        }

        if (Mathf.Abs(Input.mouseScrollDelta.y) > 0)
        {
            y = -Input.mouseScrollDelta.y * scrollSpeed * Time.deltaTime;
        }

        else
        {
            y = 0;

            // Determines y axis values (holding both "upKey" and "downKey" will negate one another)
            if (Input.GetKey(upKey))
            {
                y += movementSpeed * Time.deltaTime;
            }

            if (Input.GetKey(downKey))
            {
                y -= movementSpeed * Time.deltaTime;
            }
        }

        // Adds obtained values and updates position
        transform.position += x * Vector3.right + y * -CameraRay.direction + z * Vector3.forward;
        ClampPos();
        mousePosCurrent = Coordinates.Instance.MousePos;
    }

    // Clamps values to ensure the user cannot traverse out of bounds
    private void ClampPos()
    {
        Vector3 pos = transform.position;

        pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);
        transform.position = pos;
    }

    public static CameraMovement Instance { get { return instance; } }

    public Camera PlayerCamera { get { return playerCamera; } }

    private Ray CameraRay { get { return playerCamera.ScreenPointToRay(Input.mousePosition); } }
}