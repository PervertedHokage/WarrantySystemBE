using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantySystem.Model.Entities;

namespace WarrantySystem.Model.DTO
{
    public class SaleOrderDTO
    {
        public Order Order { get; set; }
        public List<SaleOrderDetailDTO> SaleOrderDetailDTO { get; set; }
        //public List<OrderDetailInfo>  OrderDetailInfo { get; set; }
        public List<int> DeletedOrder { get; set; }


    }

    public class SaleOrderDetailDTO
    {
        public OrderDetail OrderDetails { get; set; }
        public List<OrderDetailInfo> OrderDetailInfo { get; set; }

    }
}
