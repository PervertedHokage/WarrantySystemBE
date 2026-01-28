using WarrantySystem.Model.Entities;

namespace WarrantySystem.Model.DTO
{
    public class QuotationDetailDTO : QuotationDetail
    {
        public string SparePartNumber { get; set; } = "";
        public string UnitCode { get; set; } = "";
        public string UnitName { get; set; } = "";
    }
}
