namespace CompanyApi
{
    public class Employee
    {
        public Employee(string name, int salary)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Salary = salary;
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public int Salary { get; set; }

    }
}
