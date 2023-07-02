using System;
using TMPro;
using UnityEngine;

public class Coordinates : MonoBehaviour
{
    private static Coordinates instance; // Ensures a singleton state pattern is maintained

    public enum SnappingMode { GRID, NONE }

    [SerializeField] KeyCode snapToggleKey;

    [SerializeField] TextMeshProUGUI coordinateText;

    private SnappingMode snappingMode;

    private Plane raycastPlane;

    private Vector3 mousePos;

    private void Update()
    {
        if (Input.GetKeyDown(snapToggleKey) && BehaviorManager.Instance.CurrentStateType != BehaviorManager.StateType.PAUSED)
        {
            snappingMode = snappingMode == SnappingMode.GRID ? SnappingMode.NONE : SnappingMode.GRID;
        }    
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("Coordinates instance already established; terminating.");
        }

        instance = this;
        raycastPlane = new Plane(Vector3.down, GridMaintenance.Instance.GridHeight);
    }

    public static Vector3 NormalToGridPos(Vector3 normalPos)
    {
        return new Vector3((int)(normalPos.x + 0.5f * Mathf.Sign(normalPos.x)), GridMaintenance.Instance.GridHeight, (int)(normalPos.z + 0.5f * Mathf.Sign(normalPos.z)));
    }

    // Getter methods
    public static Coordinates Instance { get { return instance; } }

    private Ray CameraRay { get { return CameraMovement.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition); } }

    public Vector3 GridPos
    {
        get
        {
            return new Vector3((int)(mousePos.x + 0.5f * Mathf.Sign(mousePos.x)), GridMaintenance.Instance.GridHeight, (int)(mousePos.z + 0.5f * Mathf.Sign(mousePos.z)));
        }
    }

    public Vector3 MousePos
    {
        get
        {
            Ray ray = CameraRay;

            if (raycastPlane.Raycast(ray, out float distance))
            {
                mousePos = ray.GetPoint(distance);
                if (BehaviorManager.Instance.CurrentStateType != BehaviorManager.StateType.PAUSED) coordinateText.text = "(" + mousePos.x.ToString("0.0") + ", " + mousePos.z.ToString("0.0") + ")";
                return new Vector3(mousePos.x, GridMaintenance.Instance.GridHeight, mousePos.z);
            }

            throw new Exception("Unable to obtain new mouse position -- raycast failed.");
        }
    }

    public Vector3 ModePos
    {
        get
        {
            return snappingMode == SnappingMode.GRID ? GridPos : MousePos;
        }
    }

    public SnappingMode CurrentSnappingMode { get { return snappingMode; } }
}