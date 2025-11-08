namespace ProductionAdapterService.RequestModel
{
    public class ProductionRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = "ReadyForProduction";

    }
}
