using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantySystem.Model.Entities;

namespace WarrantySystem.Model.DTO
{
    public class IssuesDTO
    {
        public IssuesGroup IssuesGroup { get; set;}
        public List<Issue> Issues { get; set;}
        public List<int>? DeletedIssues { get; set; }

    }
}
