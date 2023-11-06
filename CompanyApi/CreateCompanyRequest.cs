namespace CompanyApi
{
    public class CreateCompanyRequest
    {
        public string Name { get; set; }

        public CreateCompanyRequest(string name)
        {
            Name = name;
        }
    }
}
