public class InputGate : Circuit
{
    private bool powered;

    public InputGate() : base("INPUT", 0, 1) { }

    protected override void UpdateOutputs()
    {
        Outputs[0].Powered = powered;
    }

    // Setter method
    public bool Powered { set { powered = value; } }
}