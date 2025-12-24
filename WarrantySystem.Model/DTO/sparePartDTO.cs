using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantySystem.Model.Entities;

namespace WarrantySystem.Model.DTO
{
    public class sparePartGroupDTO
    {
        public Product Product { get; set; }
        public List<SparePartsGroupWithDetailsDTO> SparePartsGroups { get; set; }
        public List<int> DeletedSparePartGroup { get; set; }
    }
    public class SparePartsGroupWithDetailsDTO
    {
        public SparePartsGroup SparePartsGroup { get; set; }
        public List<SparePart> SparePart { get; set; }
        public List<int> DeletedSparePart { get; set; }
    }


}
