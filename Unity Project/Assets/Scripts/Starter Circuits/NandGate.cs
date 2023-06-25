// Universal gate
public class NAndGate : Circuit
{
    public NAndGate() : base("NAND", 2, 1) { }

    protected override void UpdateOutputs()
    {
        Outputs[0].Powered = !(Inputs[0].Powered && Inputs[1].Powered);
    }
}