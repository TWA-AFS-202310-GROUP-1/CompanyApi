namespace CompanyApi
{
    public class CreateCompanyRequest
    {
        public required string Name { get; set; }
        public CreateCompanyRequest(string name)
        {
            this.Name = name;
        }
    }
}
