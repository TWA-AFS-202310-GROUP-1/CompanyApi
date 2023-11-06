namespace CompanyApi
{
    public class Employee
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Salary { get; set; }
        public string CompanyId { get; set; }
        public Employee(string name, decimal salary, string companyId)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Salary = salary;
            CompanyId = companyId;
        }
    }

}
