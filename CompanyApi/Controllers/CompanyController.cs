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

        [HttpGet("{pageSize}/{pageIndex}")]
        public ActionResult<List<Company>> GetByPageInfo(int pageSize, int pageIndex)
        {
            int totalCompanies = companies.Count;
            int startIndex = pageSize * (pageIndex - 1);
            int endIndex = Math.Min(startIndex + pageSize, totalCompanies);

            if (startIndex >= totalCompanies)
            {
                return StatusCode(StatusCodes.Status200OK, new List<Company>());
            }

            List<Company> resCompanies = companies.GetRange(startIndex, endIndex - startIndex);
            return StatusCode(StatusCodes.Status200OK, resCompanies);
        }

        [HttpPut("{Id}")]
        public ActionResult<Company> Put(string Id, CreateCompanyRequest request)
        {
            Company companyToUpdate = companies.FirstOrDefault(c => c.Id == Id);

            if (companyToUpdate != null)
            {
                companyToUpdate.Name = request.Name;
                return StatusCode(StatusCodes.Status200OK, companyToUpdate);
            }

            return StatusCode(StatusCodes.Status404NotFound);
        }

        [HttpPost("{companyId}/employees")]
        public ActionResult<Employee> AddEmployee(string companyId, [FromBody] CreateEmployeeRequest request)
        {
            var company = companies.FirstOrDefault(c => c.Id == companyId);
            if (company == null) return NotFound();

            Employee employee = new Employee(request.Name, request.Salary, request.CompanyId);
            company.EmployeeList.Add(employee);
            return StatusCode(StatusCodes.Status201Created, employee);
        }

        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
        }
    }
}
