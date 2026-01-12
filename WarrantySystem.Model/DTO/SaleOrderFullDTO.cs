namespace WarrantySystem.Model.DTO
{
    public class SaleOrderFullDTO
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        public string Code { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public decimal Price { get; set; }
        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public int? SerialId { get; set; }
        public int? OrderDetailInfoId { get; set; }
        public string ProductSerial { get; set; }
        public string Imei1 { get; set; }
        public string Imei2 { get; set; }
    }
}
