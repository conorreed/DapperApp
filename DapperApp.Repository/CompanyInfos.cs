namespace DapperApp.Models
{
    public class CompanyInfos
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public required string Street { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string PostalCode { get; set; }
        public bool IsActive { get; set; }
        public int ContactCount { get; set; }
        public required string CreatedBy { get; set; }
        public required string ModifiedBy { get; set; }
    }

}
