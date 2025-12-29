using WarrantySystem.Model.Entities;

namespace WarrantySystem.Model.DTO
{
    public class WarrantyClaimDTO : WarrantyClaim
    {
        public WarrantyClaimDTO() { }
        public WarrantyClaimDTO(WarrantyClaim source)
        {
            foreach (var prop in typeof(WarrantyClaim).GetProperties())
            {
                if (prop.CanRead && prop.CanWrite)
                    prop.SetValue(this, prop.GetValue(source));
            }
        }
        public string ProductName { get; set; } = "";
    }
}
