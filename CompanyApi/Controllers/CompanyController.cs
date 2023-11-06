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

        [HttpPut("{id}")]
        public ActionResult UpdateCompany(string id, [FromBody] Company updatedCompany)
        {
            var company = companies.FirstOrDefault(c => c.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            company.Name = updatedCompany.Name;

            return NoContent(); 
        }


        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
        }


        [HttpPost("{companyId}/employees")]
        public ActionResult<Employee> AddEmployee(string companyId, [FromBody] CreateEmployeeRequest request)
        {
            var company = companies.FirstOrDefault(c => c.Id == companyId);
            if (company == null)
            {
                return NotFound("Company not found.");
            }

            var employee = new Employee
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Salary = request.Salary
            };

            company.Employees.Add(employee);
            return StatusCode(StatusCodes.Status201Created, employee);
        }


        [HttpDelete("{companyId}/employees/{employeeId}")]
        public IActionResult DeleteEmployee(string companyId, string employeeId)
        {
            var company = companies.FirstOrDefault(c => c.Id == companyId);
            if (company == null)
            {
                return NotFound("Company not found.");
            }

            var employee = company.Employees.FirstOrDefault(e => e.Id == employeeId);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            company.Employees.Remove(employee);
            return NoContent();
        }



    }
}
