namespace CompanyApi
{
    public class Employee
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public decimal Salary { get; set; }
        public string CompanyId { get; private set; }
        public Employee(string name, decimal salary, string companyId)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Salary = salary;
            CompanyId = companyId;
        }

        public Employee Update(Employee employee)
        {
            Name = employee.Name;
            Salary = employee.Salary;
            CompanyId = employee.CompanyId;
            return this;
        }
    }

}
