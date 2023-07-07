using UnityEngine;

public class PreviewStructure
{
    private string name;

    public PreviewStructure(string name)
    {
        this.name = name;
    }

    public string Name { get { return name; } }
}