using System.Diagnostics.CodeAnalysis;

namespace CompanyApi
{
    public class CreateCompanyRequest
    {
        public required string Name { get; set; }

        [SetsRequiredMembers]
        public CreateCompanyRequest(string name)
        {
            this.Name = name;
        }
    }
}
