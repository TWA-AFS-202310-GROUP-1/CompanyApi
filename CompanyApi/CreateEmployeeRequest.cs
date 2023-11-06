namespace CompanyApi
{
    public class CreateEmployeeRequest
    {
        public string Name { get; set; }
        public int Salary { get; set; }

        public CreateEmployeeRequest(string name, int salary)
        {
            Name = name;
            Salary = salary;
        }
    }
}
