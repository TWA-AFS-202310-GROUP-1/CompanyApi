using Microsoft.AspNetCore.Mvc;
using System.Linq;
namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>() ;
       

        [HttpGet]
        public ActionResult<List<Company>> GetAll()
        {
            return StatusCode(StatusCodes.Status200OK, companies);
        }

        [HttpGet("{id}")]
        public ActionResult<Company> GetOne(string id) 
        {
            Company? company = companies.FirstOrDefault(company => company.Id == id);
            if (company == null)
            {
                return NotFound();
            }
            return Ok(company);
        }

        [HttpGet("page/{pageSize}/{pageIndex}")]
        public ActionResult<List<Company>> GetCompaniesByPage(int pageSize, int pageIndex)
        {
            int skip = (pageIndex - 1) * pageSize;

            var pageOfCompanies = companies.Skip(skip).Take(pageSize).ToList();

            return Ok(pageOfCompanies);
        }


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

        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
        }


    }
}
