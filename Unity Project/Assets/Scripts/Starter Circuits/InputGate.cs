public class InputGate : Circuit
{
    private bool powered;

    public InputGate() : base("INPUT", 0, 1) { }

     private new void UpdateCircuit(bool powered, Circuit circuit, int inputIndex) { }

    protected override void UpdateOutputs()
    {
        Outputs[0].Powered = powered;
    }

    // Setter method
    public bool Powered { set { powered = value; Update(); UpdateChildren(); } }
}