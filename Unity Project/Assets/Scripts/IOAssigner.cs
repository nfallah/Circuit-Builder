using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IOAssigner : MonoBehaviour
{
    private static IOAssigner instance;

    [SerializeField] KeyCode exitKey;

    [SerializeField] Material hoveredMaterial, emptyInputMaterial, emptyOutputMaterial;

    private bool movementOverride;

    private GameObject currentHover;

    private List<Circuit.Input> emptyInputs, orderedInputs;

    private List<Circuit.Output> emptyOutputs, orderedOutputs;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("IOAssigner instance already established; terminating.");
        }

        instance = this;
        enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(exitKey) || Input.GetMouseButtonDown(1)) { Exit(); return; }

        Ray ray = CameraMovement.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo) && (hitInfo.transform.gameObject.layer == 9 || hitInfo.transform.gameObject.layer == 10))
        {
            Circuit.Input input = null; Circuit.Output output = null;
            GameObject hitObject = hitInfo.transform.gameObject;

            if (hitObject.layer == 9)
            {
                input = hitObject.GetComponent<CircuitVisualizer.InputReference>().Input;

                if (!emptyInputs.Contains(input)) return;
            }

            else
            {
                output = hitObject.GetComponent<CircuitVisualizer.OutputReference>().Output;

                if (!emptyOutputs.Contains(output)) return;
            }

            if (currentHover != hitObject)
            {
                if (currentHover != null) currentHover.GetComponent<MeshRenderer>().material = currentHover.layer == 9 ? emptyInputMaterial : emptyOutputMaterial;

                hitObject.GetComponent<MeshRenderer>().material = hoveredMaterial;
                currentHover = hitObject;
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                if (input != null)
                {
                    emptyInputs.Remove(input);
                    orderedInputs.Add(input);
                }
                
                else
                {
                    emptyOutputs.Remove(output);
                    orderedOutputs.Add(output);
                }

                currentHover.GetComponent<MeshRenderer>().material = currentHover.layer == 9 ? CircuitVisualizer.Instance.InputMaterial : CircuitVisualizer.Instance.OutputMaterial;
                currentHover = null;
            }
        }

        else if (currentHover != null)
        {
            currentHover.GetComponent<MeshRenderer>().material = currentHover.layer == 9 ? emptyInputMaterial : emptyOutputMaterial;
            currentHover = null;
        }
    }

    public void Initialize(List<Circuit.Input> emptyInputs, List<Circuit.Output> emptyOutputs)
    {
        movementOverride = true;
        this.emptyInputs = emptyInputs; this.emptyOutputs = emptyOutputs;
        orderedInputs = new List<Circuit.Input>(); orderedOutputs = new List<Circuit.Output>();

        foreach (Circuit.Input input in emptyInputs) input.Transform.GetComponent<MeshRenderer>().material = emptyInputMaterial;

        foreach (Circuit.Output output in emptyOutputs) output.Transform.GetComponent<MeshRenderer>().material = emptyOutputMaterial;

        enabled = true;
    }
    
    private void Exit()
    {
        foreach (Circuit.Input input in emptyInputs) input.Transform.GetComponent<MeshRenderer>().material = CircuitVisualizer.Instance.InputMaterial;

        foreach (Circuit.Output output in emptyOutputs) output.Transform.GetComponent<MeshRenderer>().material = CircuitVisualizer.Instance.OutputMaterial;

        movementOverride = false;
        enabled = false;
        TaskbarManager.Instance.CloseMenu();
    }

    // Singleton state reference
    public static IOAssigner Instance { get { return instance; } }

    // Getter method
    public bool MovementOverride { get { return movementOverride; } }
}