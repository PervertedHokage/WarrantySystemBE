namespace WarrantySystem.Model.DTO
{
    public class WorkOrderByClaimNoDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? ProgressComplete { get; set; }
        public int? StatusId { get; set; }
        public string Description { get; set; }

        public int? QuotationId { get; set; }
        public string QuotationNumber { get; set; }

        public int? ProductId { get; set; }
        public string Name { get; set; }
        public string ProductCode { get; set; }

        public string FullName { get; set; }
        public int? UserId { get; set; }

        public int? WarrantyClaimId { get; set; }
        public string CustomerName { get; set; }
        public string ClaimNo { get; set; }

        public string Status { get; set; }
    }
}
