using System;

namespace WarrantySystem.Model.Entities;

public class WarrantyClaimAttachment
{
    public int Id { get; set; }

    public int WarrantyClaimId { get; set; }

    public string FileName { get; set; }

    public string FilePath { get; set; }

    public long FileSize { get; set; }

    public string FileType { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; }
}
