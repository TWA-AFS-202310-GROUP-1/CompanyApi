namespace CompanyApi
{
    public class Company
    {
        public Company(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Employees = new List<Employee>();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public List<Employee> Employees { get; set; }

        public void AddEmployee(Employee employee)
        {
            Employees.Add(employee);
        }
    }
}
