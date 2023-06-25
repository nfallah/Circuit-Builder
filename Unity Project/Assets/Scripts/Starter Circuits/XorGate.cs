public class XOrGate : Circuit
{
    public XOrGate() : base("XOR", 2, 1) { }

    protected override void UpdateOutputs()
    {
        Outputs[0].Powered = Inputs[0].Powered && !Inputs[1].Powered || !Inputs[0].Powered && Inputs[1].Powered;
    }
}