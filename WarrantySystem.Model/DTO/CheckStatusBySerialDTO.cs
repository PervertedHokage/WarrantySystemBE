namespace WarrantySystem.Model.DTO
{
    public class CheckStatusBySerialDTO
    {
        public int STT { get; set; }

        public string CustomerName { get; set; }

        public string CustomerPhoneNumber { get; set; }

        public string CustomerAddress { get; set; }

        public string CustomerEmail { get; set; }

        public string ProductName { get; set; }

        public string ProductSerial { get; set; }
        public string IMEI1 { get; set; }
        public string IMEI2 { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public DateTime? DateStart { get; set; }

        public DateTime? DateEnd { get; set; }
    }

}
