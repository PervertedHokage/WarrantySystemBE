using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantySystem.Model.Entities;

namespace WarrantySystem.Model.DTO
{
    public class WorkOrderDTO
    {
        public WorkOrder WorkOrder { get; set; }
        public List<WorkOrderSparePart>? WorkOrderSpareParts { get; set; }
        public List<int>? DeletedSpareSpart { get; set; }
    }
}
