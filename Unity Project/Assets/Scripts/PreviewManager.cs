using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PreviewManager : MonoBehaviour
{
    private static PreviewManager instance;

    [SerializeField] Material inputMaterial, outputMaterial;

    [SerializeField] TextMeshProUGUI nameText;

    private List<Circuit> circuits = new List<Circuit>();

    private List<Circuit.Input> inputs = new List<Circuit.Input>();

    private List<Circuit.Output> outputs = new List<Circuit.Output>();

    private PreviewStructure previewStructure;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("PreviewManager instance already established; terminating.");
        }

        instance = this;
    }

    private void Start()
    {
        CursorManager.SetMouseTexture(true);
        previewStructure = MenuLogicManager.Instance.CurrentPreviewStructure;
        nameText.text = previewStructure.Name;
        Deserialize();
        UpdateIOMaterials();
    }
    
    private void Deserialize()
    {
        foreach (CircuitIdentifier circuitIdentifier in previewStructure.Circuits) circuits.Add(CircuitIdentifier.RestoreCircuit(circuitIdentifier));

        MenuSetupManager.Instance.RestoreConnections(previewStructure);
        CameraMovementPreview.Instance.PlayerCamera.transform.position = previewStructure.CameraLocation;
    }

    private void UpdateIOMaterials()
    {
        int inputIndex = 0, outputIndex = 0;

        foreach (Circuit circuit in circuits)
        {
            foreach (Circuit.Input input in circuit.Inputs)
            {
                if (previewStructure.InputOrders[inputIndex] != -1) input.Transform.GetComponent<MeshRenderer>().material = inputMaterial;

                inputs.Add(input);
                inputIndex++;
            }

            foreach (Circuit.Output output in circuit.Outputs)
            {
                if (previewStructure.OutputOrders[outputIndex] != -1) output.Transform.GetComponent<MeshRenderer>().material = outputMaterial;

                outputs.Add(output);
                outputIndex++;
            }
        }
    }

    // Single state reference
    public static PreviewManager Instance { get { return instance; } }

    // Getter methods
    public List<Circuit> Circuits { get { return circuits; } }

    public List<Circuit.Input> Inputs { get { return inputs; } }

    public List<Circuit.Output> Outputs { get { return outputs; } }

    public Material InputMaterial { get { return inputMaterial; } }

    public Material OutputMaterial { get { return outputMaterial; } }
}