using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] float movementSpeed, minHeight, maxHeight;

    [SerializeField] KeyCode upKey, downKey;

    private Vector3 cameraPos; // Utilized for height clamping

    private void Start()
    {
        cameraPos = transform.position;
        UpdatePos();
    }

    private void Update()
    {
        // Obtains x and z axis values based on input; initializes y-axis value
        float x = Input.GetAxisRaw("Horizontal") * movementSpeed * Time.deltaTime;
        float y = 0;
        float z = Input.GetAxisRaw("Vertical") * movementSpeed * Time.deltaTime;

        // Determines y axis values (holding both "upKey" and "downKey" will negate one another)
        if (Input.GetKey(upKey))
        {
            y += movementSpeed * Time.deltaTime;
        }

        if (Input.GetKey(downKey))
        {
            y -= movementSpeed * Time.deltaTime;
        }

        // Adds obtained values and updates position
        cameraPos += x * Vector3.right + y * Vector3.up + z * Vector3.forward;
        UpdatePos();
    }

    // Clamps values and mirrors "cameraPos" to the actual in-scene transform
    private void UpdatePos()
    {
        cameraPos.y = Mathf.Clamp(cameraPos.y, minHeight, maxHeight);
        transform.position = cameraPos;
    }
}