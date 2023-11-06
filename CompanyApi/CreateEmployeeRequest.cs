namespace CompanyApi
{
    public class CreateEmployeeRequest
    {
        public CreateEmployeeRequest(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
    }
}
