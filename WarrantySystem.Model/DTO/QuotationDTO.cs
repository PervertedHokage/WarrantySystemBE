using WarrantySystem.Model.Entities;

namespace WarrantySystem.Model.DTO
{
    public class QuotationDTO : Quotation
    {
        public string ClaimNo { get; set; } = "";

        public int? ProductSerialId { get; set; }

        public string ProductName { get; set; } = "";

        public string StatusQuotationText { get; set; } = "";

        public string StatusReplyText { get; set; } = "";
    }
}
