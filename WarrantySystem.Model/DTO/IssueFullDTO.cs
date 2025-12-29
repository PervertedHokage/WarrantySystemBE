namespace WarrantySystem.Model.DTO
{
    public class IssueFullDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int IssuesGroupId { get; set; }
        public string IssuesGroupName { get; set; } = "";
    }
}
