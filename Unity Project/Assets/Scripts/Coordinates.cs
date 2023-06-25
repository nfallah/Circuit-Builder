using System;
using TMPro;
using UnityEngine;

public class Coordinates : MonoBehaviour
{
    private static Coordinates instance; // Ensures a singleton state pattern is maintained

    [SerializeField] TextMeshProUGUI coordinateText;

    private Vector3 gridPos;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("Coordinates instance already established; terminating.");
        }

        instance = this;
    }

    public void UpdateCoordinates(Vector3 worldPos)
    {
        coordinateText.text = "(" + worldPos.x.ToString("0.0") + ", " + worldPos.z.ToString("0.0") + ")";
        gridPos = new Vector3((int)(worldPos.x + 0.5f * Mathf.Sign(worldPos.x)), 0, (int)(worldPos.z + 0.5f * Mathf.Sign(worldPos.z)));
    }

    // Getter methods
    public static Coordinates Instance { get { return instance; } }

    public Vector3 GridPos { get { return gridPos; } }
}