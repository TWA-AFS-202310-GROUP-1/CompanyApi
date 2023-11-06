namespace CompanyApi
{
    public class CreateEmployeeRequest
    {
        public required string Name { get; set; }
        public decimal Salary { get; set; }
        public required string CompanyId { get; set; }
    }
    public class UpdateEmployeeRequest
    {
        public required string Name { get; set; }
        public decimal Salary { get; set; }
        public required string CompanyId { get; set; }
    }
}

