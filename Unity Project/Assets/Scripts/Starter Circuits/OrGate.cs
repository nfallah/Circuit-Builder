public class OrGate : Circuit
{
    public OrGate() : base("OR", 2, 1) { }

    protected override void UpdateOutputs()
    {
        Outputs[0].Powered = Inputs[0].Powered || Inputs[1].Powered;
    }
}