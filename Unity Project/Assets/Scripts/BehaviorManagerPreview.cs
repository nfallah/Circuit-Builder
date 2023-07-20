using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UIElements;
using System.Drawing;

public class BehaviorManagerPreview : MonoBehaviour
{
    private static BehaviorManagerPreview instance;

    [SerializeField] Material selectedMaterial;

    [SerializeField] TextMeshProUGUI coordinatesText, labelText;

    private bool isInput, isUILocked;

    private int labelIndex;

    private GameObject currentHitObject;

    private Plane coordinatesPlane;

    private readonly string defaultHoverText = "hover over and select inputs/outputs to view their order & label";

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("BehaviorManagerPreview instance already established; terminating.");
        }

        instance = this;
        coordinatesPlane = new Plane(Vector3.down, GridMaintenance.Instance.GridHeight);
    }

    private void Start()
    {
        labelText.text = defaultHoverText;
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) { isUILocked = true; State(null); return; }

        isUILocked = false;

        bool raycastHit = Physics.Raycast(CameraMovementPreview.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);

        if (!raycastHit) { State(null); return; }

        if (hitInfo.transform.gameObject.layer != 9 && hitInfo.transform.gameObject.layer != 10) { State(null); return; }

        State(hitInfo.transform.gameObject);
    }

    private void State(GameObject hitObject)
    {
        if (currentHitObject == hitObject) return;

        if (currentHitObject != null) currentHitObject.GetComponent<MeshRenderer>().material = currentHitObject.layer == 9 ? PreviewManager.Instance.InputMaterial : PreviewManager.Instance.OutputMaterial;

        if (hitObject != null && UpdateLabelIndex(hitObject) != -1)
        {
            string newLabelText = isInput ? MenuLogicManager.Instance.CurrentPreviewStructure.InputLabels[labelIndex] : MenuLogicManager.Instance.CurrentPreviewStructure.OutputLabels[labelIndex];

            hitObject.GetComponent<MeshRenderer>().material = selectedMaterial;
            labelText.text = (isInput ? "input" : "output") + " #" + (labelIndex + 1) + (newLabelText != "" ? " - " + newLabelText : "");
            currentHitObject = hitObject;
        }

        else
        {
            labelText.text = defaultHoverText;

            if (hitObject == null) currentHitObject = null;
        }
    }

    // Helper methods
    private int UpdateLabelIndex(GameObject hitObject)
    {
        if (hitObject.layer == 9)
        {
            isInput = true;
            labelIndex = MenuLogicManager.Instance.CurrentPreviewStructure.InputOrders[PreviewManager.Instance.Inputs.IndexOf(hitObject.GetComponent<CircuitVisualizer.InputReference>().Input)];
        }

        else
        {
            isInput = false;
            labelIndex = MenuLogicManager.Instance.CurrentPreviewStructure.OutputOrders[PreviewManager.Instance.Outputs.IndexOf(hitObject.GetComponent<CircuitVisualizer.OutputReference>().Output)];
        }

        return labelIndex;
    }

    public Vector3 UpdateCoordinates()
    {
        Ray raycastRay = CameraMovementPreview.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition);

        if (coordinatesPlane.Raycast(raycastRay, out float distance))
        {
            Vector3 point = raycastRay.GetPoint(distance);

            if (!isUILocked) coordinatesText.text = "(" + point.x.ToString("0.0") + ", " + point.z.ToString("0.0") + ")";

            return new Vector3(point.x, GridMaintenance.Instance.GridHeight, point.z);
        }

        throw new Exception("Unable to obtain new mouse position -- raycast failed.");
    }

    // Singleton state reference
    public static BehaviorManagerPreview Instance { get { return instance; } }

    // Getter method
    public bool IsUILocked { get { return isUILocked; } }
}