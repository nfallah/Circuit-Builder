using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PreviewStructure
{
    [SerializeField] List<CircuitIdentifier> circuits = new List<CircuitIdentifier>();

    [SerializeField] int id;

    [SerializeField] List<int> inputOrders, outputOrders;

    [SerializeField] string name;

    [SerializeField] List<string> inputLabels, outputLabels;

    [SerializeField] Vector3 cameraLocation;

    public PreviewStructure(string name)
    {
        this.name = name;
    }

    // Getter and setter methods
    public Vector3 CameraLocation { get { return cameraLocation; } set { cameraLocation = value; } }

    public List<CircuitIdentifier> Circuits { get { return circuits; } set { circuits = value; } }

    public int ID { get { return id; } set { id = value; } }

    public List<int> InputOrders { get { return inputOrders; } set { inputOrders = value; } }

    public List<int> OutputOrders { get { return outputOrders; } set { outputOrders = value; } }

    public List<string> InputLabels { get { return inputLabels; } set { inputLabels = value; } }

    public List<string> OutputLabels { get { return outputLabels; } set { outputLabels = value; } }

    // Getter method
    public string Name { get { return name; } }
}