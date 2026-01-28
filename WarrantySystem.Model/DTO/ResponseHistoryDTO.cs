using WarrantySystem.Model.Entities;

namespace WarrantySystem.Model.DTO
{
    public class ResponseHistoryDTO : ResponseHistory
    {
        public string CustomerName { get; set; } = "";
        public string HandledBy { get => CreatedBy; set => CreatedBy = value; }
        public string ResponseContent { get => ResponseText; set => ResponseText = value; }

        public ResponseHistoryDTO() { }

        public ResponseHistoryDTO(ResponseHistory entity)
        {
            if (entity == null) return;
            Id = entity.Id;
            ResponseDate = entity.ResponseDate;
            CustomerId = entity.CustomerId;
            ResponseText = entity.ResponseText;
            ReceptionWorkerId = entity.ReceptionWorkerId;
            Status = entity.Status;
            Note = entity.Note;
            CreatedDate = entity.CreatedDate;
            CreatedBy = entity.CreatedBy;
            UpdatedDate = entity.UpdatedDate;
            UpdatedBy = entity.UpdatedBy;
            WarrantyClaimId = entity.WarrantyClaimId;
        }
    }
}
