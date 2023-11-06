namespace CompanyApi
{
    public class Employee
    {
        public Employee(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
        }

        public string Id { get; set; }

        public string Name { get; set; }
    }
}
