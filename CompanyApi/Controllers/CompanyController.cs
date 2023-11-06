using Microsoft.AspNetCore.Mvc;

namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>();

        [HttpPost]
        public ActionResult<Company> Create(CreateCompanyRequest request)
        {
            if (companies.Exists(company => company.Name.Equals(request.Name)))
            {
                return BadRequest();
            }
            Company companyCreated = new Company(request.Name);
            companies.Add(companyCreated);
            return StatusCode(StatusCodes.Status201Created, companyCreated);
        }

        [HttpGet("/api/companies")]
        public ActionResult<List<Company>> GetAllCompanies()
        {
            return Ok(companies);
        }

        [HttpGet("/api/companies/{companyName}")]
        public ActionResult<List<Company>> GetCompany(string companyName)
        {
            Company company = companies.FirstOrDefault(c => c.Name == companyName);
            return Ok(company);
        }
        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
        }
    }
}
