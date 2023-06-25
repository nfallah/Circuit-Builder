// Universal gate
public class NOrGate : Circuit
{
    public NOrGate() : base("NOR", 2, 1) { }

    protected override void UpdateOutputs()
    {
        Outputs[0].Powered = !(Inputs[0].Powered || Inputs[1].Powered);
    }
}