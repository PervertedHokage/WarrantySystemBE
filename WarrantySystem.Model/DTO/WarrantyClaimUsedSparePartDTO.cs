using System;
using WarrantySystem.Model.Entities;

namespace WarrantySystem.Model.DTO
{
    public class WarrantyClaimUsedSparePartDTO : WarrantyClaimUsedSparePart
    {
        public string SupplyCode { get; set; }
        public string SupplyName { get; set; }
        public string Unit { get => UnitName; set => UnitName = value; }
        public string UnitName { get; set; }
        public decimal? UnitPrice { get; set; }

        public WarrantyClaimUsedSparePartDTO() { }

        public WarrantyClaimUsedSparePartDTO(WarrantyClaimUsedSparePart entity)
        {
            if (entity == null) return;
            Id = entity.Id;
            WarrantyClaimId = entity.WarrantyClaimId;
            SparePartId = entity.SparePartId;
            UnitId = entity.UnitId;
            Quantity = entity.Quantity;
            Note = entity.Note;
            IsDeleted = entity.IsDeleted;
        }
    }
}
