namespace DemoExamSolution.DTO
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public int OrderNumber { get; set; }
        public string Articul { get; set; }
        public string StatusName { get; set; }
        public string OrderAddress { get; set; }
        public string OrderDate { get; set; }
        public string DeliveryDate { get; set; }
        public string ClientName { get; set; }
        public int Code { get; set; }
    }
}
