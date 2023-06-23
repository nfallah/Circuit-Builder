using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitVisualizer : MonoBehaviour
{
    private static CircuitVisualizer instance;

    [SerializeField] float inputSize, outputSize, heightMargins, width;

    [SerializeField] Material baseMaterial, inputMaterial, outputMaterial;

    private readonly int[] triangles = new int[] { 0, 1, 3, 3, 1, 2 };

    private readonly Vector2[] uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };

    private readonly Vector3[] normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("CircuitVisualizer instance already established; terminating.");
        }

        instance = this;
    }

    private void Start()
    {
        VisualizeCircuit(new AndGate());    
    }

    public void VisualizeCircuit(Circuit circuit)
    {
        // Setting dimensions
        int inputMargins = circuit.Inputs.Length + 1, outputMargins = circuit.Outputs.Length + 1;
        float inputHeight = inputMargins * heightMargins + circuit.Inputs.Length * inputSize;
        float outputHeight = outputMargins * heightMargins + circuit.Outputs.Length * outputSize;
        Vector2 dimensions = new Vector2(width, Mathf.Max(inputHeight, outputHeight));

        // Creating base quad
        GameObject physicalObject = new GameObject(circuit.CircuitName);
        GameObject baseQuad = new GameObject("Circuit Base");

        baseQuad.transform.parent = physicalObject.transform;
        baseQuad.transform.localPosition = Vector3.zero;

        Mesh mesh = new Mesh();
        MeshFilter meshFilter = baseQuad.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = baseQuad.AddComponent<MeshRenderer>();

        mesh.vertices = new Vector3[]
        {
            new Vector3(-dimensions.x / 2, 0, -dimensions.y / 2),
            new Vector3(-dimensions.x / 2, 0, dimensions.y / 2),
            new Vector3(dimensions.x / 2, 0, dimensions.y / 2),
            new Vector3(dimensions.x / 2, 0, -dimensions.y / 2)
        };
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.normals = normals;
        meshFilter.mesh = mesh;
        meshRenderer.material = baseMaterial;
        baseQuad.AddComponent<MeshCollider>();

        // Creating input nodes
        float inputStepSize = (dimensions.y - circuit.Inputs.Length * inputSize) / inputMargins;
        int index = 0;

        for (float currentHeight = inputStepSize + inputSize / 2; index < circuit.Inputs.Length; currentHeight += inputStepSize + inputSize)
        {
            GameObject inputQuad = new GameObject("Circuit Input");

            inputQuad.transform.parent = physicalObject.transform;
            inputQuad.transform.localPosition = new Vector3(-dimensions.x / 2, 0.01f, currentHeight - dimensions.y / 2);
            mesh = new Mesh();
            meshFilter = inputQuad.AddComponent<MeshFilter>();
            meshRenderer = inputQuad.AddComponent<MeshRenderer>();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-inputSize / 2, 0, -inputSize / 2),
                new Vector3(-inputSize / 2, 0, inputSize / 2),
                new Vector3(inputSize / 2, 0, inputSize / 2),
                new Vector3(inputSize / 2, 0, -inputSize / 2)
            };
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.normals = normals;
            meshFilter.mesh = mesh;
            meshRenderer.material = inputMaterial;
            inputQuad.AddComponent<MeshCollider>();
            index++;
        }

        // Creating output nodes
        float outputStepSize = (dimensions.y - circuit.Outputs.Length * outputSize) / outputMargins;
        index = 0;

        for (float currentHeight = outputStepSize + outputSize / 2; index < circuit.Outputs.Length; currentHeight += outputStepSize + outputSize)
        {
            GameObject outputQuad = new GameObject("Circuit Output");

            outputQuad.transform.parent = physicalObject.transform;
            outputQuad.transform.localPosition = new Vector3(dimensions.x / 2, 0.01f, currentHeight - dimensions.y / 2);
            mesh = new Mesh();
            meshFilter = outputQuad.AddComponent<MeshFilter>();
            meshRenderer = outputQuad.AddComponent<MeshRenderer>();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-outputSize / 2, 0, -outputSize / 2),
                new Vector3(-outputSize / 2, 0, outputSize / 2),
                new Vector3(outputSize / 2, 0, outputSize / 2),
                new Vector3(outputSize / 2, 0, -outputSize / 2)
            };
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.normals = normals;
            meshFilter.mesh = mesh;
            meshRenderer.material = outputMaterial;
            outputQuad.AddComponent<MeshCollider>();
            index++;
        }
    }

    // Getter method
    public static CircuitVisualizer Instance { get { return instance; } }
}