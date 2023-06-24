using UnityEngine;

public class WireReference : MonoBehaviour
{
    private Circuit.Input input;

    private Circuit.Output output;

    public Circuit.Output Output { get { return output; } set { output = value; } }
}