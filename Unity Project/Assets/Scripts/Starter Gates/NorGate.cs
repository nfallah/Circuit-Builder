// Universal gate
public class NorGate : Circuit
{
    public NorGate() : base("NOR", 2, 1) { }

    public override void UpdateOutputs()
    {
        Outputs[0].Powered = !(Inputs[0].Powered || Inputs[1].Powered);
    }
}