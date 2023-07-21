using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OutputLayer
{
    [SerializeField] List<int> inputIndeces;

    public OutputLayer(List<int> inputIndeces) { this.inputIndeces = inputIndeces; }

    public List<int> InputIndeces { get { return inputIndeces; } }
}