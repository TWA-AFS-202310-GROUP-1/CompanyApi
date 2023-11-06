using Microsoft.AspNetCore.Mvc;
using static CompanyApi.EmployeeRequest;

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

/*        [HttpGet("")]
        public ActionResult<List<Company>> GetAll()
        {
            return Ok(companies.ToList());
        }*/

        [HttpGet("{id}")]
        public ActionResult<Company> Get(string id)
        {
            Company res = companies.FirstOrDefault(company => company.Id.Equals(id));
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }

        [HttpGet]
        public ActionResult<List<Company>> GetPaged([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
        {
            if(pageIndex == null || pageSize == null)
            {
                return Ok(companies.ToList());
            }
            List<Company> page = companies.Skip(((int)pageIndex - 1) * (int)pageSize).Take((int)pageSize).ToList();
            return Ok(page);         
        }

        [HttpPut("{companyID}")]
        public ActionResult UpdateCompany(string companyID, [FromBody] CreateCompanyRequest updateData)
        {
            var existingCompany = companies.FirstOrDefault(c => c.Id == companyID);
            if (existingCompany == null)
            {
                return NotFound("Company Not Found");
            }
            existingCompany.Name = updateData.Name;
            return NoContent();
        }

        [HttpPost("{companyId}/employees")]
        public ActionResult<Employee> AddEmployee(string companyId, [FromBody] CreateEmployeeRequest request)
        {
            var company = companies.FirstOrDefault(c => c.Id == companyId);
            if (company == null) return NotFound();

            Employee employee = new(request.Name, request.Salary, request.CompanyId);
            company.AddEmployee(employee);
            return StatusCode(StatusCodes.Status201Created, employee);
        }

        [HttpDelete("{companyID}/employees/{employeeID}")]
        public IActionResult DeleteEmployee(string companyID, string employeeID)
        {
            var company = companies.FirstOrDefault(c => c.Id == companyID);
            if (company == null)
            {
                return NotFound("Company Not Found");
            }
            var employee = company.Employees.FirstOrDefault(e => e.Id == employeeID);

            if (employee == null)
            {
                return NotFound("Employee Not Found");
            }
            company.Employees.Remove(employee);
            return NoContent();
        }

        [HttpGet("{companyID}/employees")]
        public IActionResult GetEmployeesByCompany(string companyID)
        {
            var company = companies.FirstOrDefault(c => c.Id == companyID);
            if (company == null)
            {
                return NotFound("Company Not Found");
            }
            var employees = company.Employees.ToList();
            return Ok(employees); 
        }

    }
}
