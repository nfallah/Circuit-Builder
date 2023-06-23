using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] Camera playerCamera;

    [SerializeField] float movementSpeed, scrollSpeed, minHeight, maxHeight;

    [SerializeField] KeyCode upKey, downKey;

    private readonly Plane raycastPlane = new Plane(Vector3.up, Vector3.zero);

    private Vector3 mousePos; // Keeps track of the mouse position from the previous frame

    private void Start()
    {
        ClampPos();
        UpdateMousePos();
    }

    private void Update()
    {
        float x, y, z;

        Vector3 mousePosPrev = mousePos;

        UpdateMousePos();

        if (Input.GetMouseButton(0))
        {
            Vector3 mousePosDelta = mousePosPrev - mousePos;

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
        UpdateMousePos();
    }

    // Clamps values to ensure the user cannot traverse out of bounds
    private void ClampPos()
    {
        Vector3 pos = transform.position;

        pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);
        transform.position = pos;
    }

    private void UpdateMousePos()
    {
        Ray ray = CameraRay;

        if (raycastPlane.Raycast(ray, out float distance))
        {
            mousePos = ray.GetPoint(distance);
            Coordinates.Instance.UpdateCoordinates(mousePos);
        }
    }

    private Ray CameraRay { get { return playerCamera.ScreenPointToRay(Input.mousePosition); } }
}