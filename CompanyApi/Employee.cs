namespace CompanyApi
{
    public class Employee
    {
        public string Id { get; set; } 
        public string Name { get; set; }
        public decimal Salary { get; set; }
    }

    public class CreateEmployeeRequest
    {
        public string Name { get; set; }
        public decimal Salary { get; set; }

    }

}
