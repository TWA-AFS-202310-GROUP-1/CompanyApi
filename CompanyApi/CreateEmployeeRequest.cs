namespace CompanyApi
{
    public class CreateEmployeeRequest
    {
        public string Name { get; set; }
        public CreateEmployeeRequest(string name)
        {
            Name = name;
        }
    }
}
