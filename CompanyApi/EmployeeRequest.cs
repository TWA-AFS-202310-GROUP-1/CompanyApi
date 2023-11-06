namespace CompanyApi
{
    public class EmployeeRequest
    {
        public string CompanyId { get; set; }
        public int Salary { get; set; }
        public string Name { get; set; }

        public class UpdateEmployeeRequest
        {
            public string Name { get; set; }
            public decimal Salary { get; set; }
            public string CompanyId { get; set; }
        }

        public class CreateEmployeeRequest
        {
            public string Name { get; set; }
            public decimal Salary { get; set; }
            public string CompanyId { get; set; }
        }
    }
}
