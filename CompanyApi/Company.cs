namespace CompanyApi
{
    public class Company
    {
        public Company(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            EmployeeList = new List<Employee>();
        }

        public string Id { get; set; }

        public string Name { get; set; }
        public List<Employee> EmployeeList { get; set; }

    }
}
