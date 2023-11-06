namespace CompanyApi
{
    public class CreateCompanyRequest
    {
        public CreateCompanyRequest(string name) 
        {
            this.Name = name;
        }

        public string Name { get; set; }
    }
}
