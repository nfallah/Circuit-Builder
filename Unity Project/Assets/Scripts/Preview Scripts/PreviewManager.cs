using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// PreviewManager deserializes the current preview structure within the preview scene.
/// </summary>
public class PreviewManager : MonoBehaviour
{
    // Singleton state reference
    private static PreviewManager instance;

    /// <summary>
    /// Material used for empty inputs and outputs respectively.
    /// </summary>
    [SerializeField]
    Material inputMaterial,
        outputMaterial;

    /// <summary>
    /// Displays the current input or output label being hovered on, if any.
    /// </summary>
    [SerializeField]
    TextMeshProUGUI nameText;

    /// <summary>
    /// List of instantiated circuits in the scene.
    /// </summary>
    private List<Circuit> circuits = new List<Circuit>();

    /// <summary>
    /// List of all inputs from circuits in the scene.
    /// </summary>
    private List<Circuit.Input> inputs = new List<Circuit.Input>();

    /// <summary>
    /// List of all outputs from circuits in the scene.
    /// </summary>
    private List<Circuit.Output> outputs = new List<Circuit.Output>();

    /// <summary>
    /// The preview structure to deserialize and load into the preview scene.
    /// </summary>
    private PreviewStructure previewStructure;

    // Enforces a singleton state pattern
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("PreviewManager instance already established; terminating.");
        }

        instance = this;
    }

    // Begins the deserialization process
    private void Start()
    {
        CursorManager.SetMouseTexture(true);
        previewStructure = MenuLogicManager.Instance.CurrentPreviewStructure;
        nameText.text = previewStructure.Name;
        Deserialize();
        UpdateIOMaterials();
    }
    
    /// <summary>
    /// Deserializes the current preview structure.<br/><br/>
    /// The restored values include the circuits, connections, and camera position.
    /// </summary>
    private void Deserialize()
    {
        foreach (CircuitIdentifier circuitIdentifier in previewStructure.Circuits) circuits.Add(CircuitIdentifier.RestoreCircuit(circuitIdentifier));

        MenuSetupManager.Instance.RestoreConnections(previewStructure);
        CameraMovementPreview.Instance.PlayerCamera.transform.position = previewStructure.CameraLocation;
    }

    /// <summary>
    /// Iterates through each circuit in the scene to change the material of empty inputs and outputs.
    /// </summary>
    private void UpdateIOMaterials()
    {
        int inputIndex = 0, outputIndex = 0;

        foreach (Circuit circuit in circuits)
        {
            foreach (Circuit.Input input in circuit.Inputs)
            {
                // If this current input exists in InputOrders, it is guaranteed to be an empty input.
                if (previewStructure.InputOrders[inputIndex] != -1) input.Transform.GetComponent<MeshRenderer>().material = inputMaterial;

                inputs.Add(input);
                inputIndex++;
            }

            foreach (Circuit.Output output in circuit.Outputs)
            {
                // If this current output exists in OutputOrders, it is guaranteed to be an empty output.
                if (previewStructure.OutputOrders[outputIndex] != -1) output.Transform.GetComponent<MeshRenderer>().material = outputMaterial;

                outputs.Add(output);
                outputIndex++;
            }
        }
    }

    // Getter methods
    public static PreviewManager Instance { get { return instance; } }

    public List<Circuit> Circuits { get { return circuits; } }

    public List<Circuit.Input> Inputs { get { return inputs; } }

    public List<Circuit.Output> Outputs { get { return outputs; } }

    public Material InputMaterial { get { return inputMaterial; } }

    public Material OutputMaterial { get { return outputMaterial; } }
}