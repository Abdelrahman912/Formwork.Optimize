namespace FormworkOptimize.Core.Entities.CostParameters
{
    public class IncludedFormworkTransportation : Transportation
    {
        public override double GetCost()
        {
            return 0;
        }
    }
}
