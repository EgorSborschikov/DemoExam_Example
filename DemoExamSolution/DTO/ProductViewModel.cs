namespace DemoExamSolution.DTO
{
    /// <summary>
    /// Модель представления данных о товарах
    /// </summary>
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Articul { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string Supplier { get; set; }
        public decimal Price { get; set; }
        public string UnitOfMeasurement { get; set; }
        public int QuantityInStock { get; set; }
        public int Discount { get; set; }
        public string PhotoPath { get; set; }
    }
}
