using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Xml.Linq;

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

        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
        }

        [HttpGet]
        public ActionResult<List<Company>> GetAll([FromQuery] int? pageSize, [FromQuery] int? pageIndex)
        {
            if (pageSize == null && pageIndex == null)
            {
                return Ok(companies);
            }
            var resultList = companies.Skip((int)((pageIndex - 1) * pageSize)).Take((int)pageSize).ToList(); // PageIndex start with 1
            return Ok(resultList);
        }

        [HttpGet("{id}")]
        public ActionResult<Company> Get(string id)
        {
            Company? res = companies.FirstOrDefault(x => x.Id == id);
            if(res == null)
            {
                return NotFound();
            }
            return StatusCode(StatusCodes.Status200OK, res);
        }

        [HttpPut("{id}")]
        public ActionResult<Company> Put(string id, [FromBody]CreateCompanyRequest request)
        {
            if (!companies.Exists(company => company.Id.Equals(id)))
            {
                return NotFound();
            }
            Company? res = companies.FirstOrDefault(x => x.Id == id);
            res.Name = request.Name;

            return Ok(res);
        }
    }
}
