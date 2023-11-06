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
            var resultList = companies.Skip((int)((pageIndex - 1) * pageSize)).Take((int)pageSize).ToList();
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

        [HttpPost("{companyID}/Employees")]
        public ActionResult<Employee> CreateEmployee(string companyID, CreateEmployeeRequest request)
        {
            if (!companies.Exists(company => company.Id == companyID))
            {
                return NotFound();
            }
            Company? company = companies.FirstOrDefault(x => x.Id == companyID);

            if (company.Employees.Exists(employee => employee.Name.Equals(request.Name)))
            {
                return BadRequest();
            }
            Employee employeeCreated = new Employee(request.Name, request.Salary);
            company.Employees.Add(employeeCreated);
            return StatusCode(StatusCodes.Status201Created, employeeCreated);
        }

        [HttpDelete("{companyID}/employees/{employeeID}")]
        public ActionResult DeleteEmployee(string companyID, string employeeID)
        {
            if (!companies.Exists(company => company.Id == companyID))
            {
                return NotFound();
            }

            Company? company = companies.FirstOrDefault(x => x.Id == companyID);

            if (!company.Employees.Exists(employee => employee.Id == employeeID))
            {
                return NotFound();
            }

            Employee? employee = company.Employees.FirstOrDefault(x => x.Id == employeeID);
            company.Employees.Remove(employee);
            return NoContent();
        }
    }
}
