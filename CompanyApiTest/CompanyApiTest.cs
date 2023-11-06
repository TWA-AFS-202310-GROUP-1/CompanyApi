using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace CompanyApiTest
{
    public class CompanyApiTest
    {
        private HttpClient httpClient;

        public CompanyApiTest()
        {
            WebApplicationFactory<Program> webApplicationFactory = new WebApplicationFactory<Program>();
            httpClient = webApplicationFactory.CreateClient();
        }

        [Fact]
        public async Task Should_return_created_company_with_status_201_when_create_cpmoany_given_a_company_name()
        {
            // Given
            await ClearDataAsync();
            Company companyGiven = new Company("BlueSky Digital Media");
            
            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "/api/companies", 
                SerializeObjectToContent(companyGiven)
            );
           
            // Then
            Assert.Equal(HttpStatusCode.Created, httpResponseMessage.StatusCode);
            Company? companyCreated = await DeserializeTo<Company>(httpResponseMessage);
            Assert.NotNull(companyCreated);
            Assert.NotNull(companyCreated.Id);
            Assert.Equal(companyGiven.Name, companyCreated.Name);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_create_company_given_a_existed_company_name()
        {
            // Given
            await ClearDataAsync();
            Company companyGiven = new Company("BlueSky Digital Media");

            // When
            await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "/api/companies", 
                SerializeObjectToContent(companyGiven)
            );
            // Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_create_company_given_a_company_with_unknown_field()
        {
            // Given
            await ClearDataAsync();
            StringContent content = new StringContent("{\"unknownField\": \"BlueSky Digital Media\"}", Encoding.UTF8, "application/json");
          
            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("/api/companies", content);
           
            // Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_list_of_companies_with_status_200_when_get_all_companies_given_nothing()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky");
            await httpClient.PostAsJsonAsync("api/companies", companyGiven);

            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies");

            var result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>();

            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(companyGiven.Name, result[0].Name);
        }

        [Fact]
        public async Task Should_return_company_name_with_status_200_when_find_company_given_companyID()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky");
            var company = await httpClient.PostAsJsonAsync("api/companies", companyGiven);
            var companyRes = await company.Content.ReadFromJsonAsync<Company>();

            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies/" + companyRes.Id);
            var result = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(companyRes.Name, result.Name);
        }

        [Fact]
        public async Task Should_return_not_found_with_status_404_when_not_find_company_given_companyID()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky");
            var company = await httpClient.PostAsJsonAsync("api/companies", companyGiven);
            var result = await company.Content.ReadFromJsonAsync<Company>();

            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies/" + "sss");
            Assert.Equal(HttpStatusCode.NotFound, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_companies_with_status_200_when_find_company_list_given_page_and_page_size()
        {
            await ClearDataAsync();
            List<Company> companies = new List<Company>();
            for (int i = 1; i <= 10; i++)
            {
                CreateCompanyRequest companyGiven = new CreateCompanyRequest($"Comapny {i}");
                var response = await httpClient.PostAsJsonAsync("api/companies", companyGiven);
                var result = await response.Content.ReadFromJsonAsync<Company>();
                companies.Add(result);
            }

            int pageIndex = 2;
            int pageSize = 2;

            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("api/companies?pageSize=2&pageIndex=2");
            var page = await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>();
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(companies[2].Name, page[0].Name);
        }

        [Fact]
        public async Task Should_update_company_with_status_204_when_valid_company_id_given()
        {
            await ClearDataAsync();

            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky");
            var company = await httpClient.PostAsJsonAsync("api/companies", companyGiven);
            var companyRes = await company.Content.ReadFromJsonAsync<Company>();

            var updateData = new
            {
                name = "Google"
            };
            var response = await httpClient.PutAsJsonAsync($"/api/companies/{companyRes.Id}", updateData);

            HttpResponseMessage afterUpdate = await httpClient.GetAsync($"/api/companies/{companyRes.Id}");

            var updatedCompany = await afterUpdate.Content.ReadFromJsonAsync<Company>();
            Assert.Equal("Google", updatedCompany.Name);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Should_not_update_company_with_status_404_when_invalid_company_id_given()
        {
            await ClearDataAsync();

            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky");
            var company = await httpClient.PostAsJsonAsync("api/companies", companyGiven);
            var companyRes = await company.Content.ReadFromJsonAsync<Company>();

            var updateData = new
            {
                name = "Google"
            };
            var response = await httpClient.PutAsJsonAsync($"/api/companies/xxx", updateData);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Should_add_employee_to_specific_company_with_status_201()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Apple");
            var companyResponse = await httpClient.PostAsJsonAsync("api/companies", companyGiven);
            var company = await companyResponse.Content.ReadFromJsonAsync<Company>();

            var employeeRequest = new EmployeeRequest
            {
                Name = "wx",
                Salary = 1000,
                CompanyId = company.Id
            };


            var response = await httpClient.PostAsJsonAsync($"/api/companies/{company.Id}/employees", employeeRequest);
            var addedEmployee = await response.Content.ReadFromJsonAsync<Employee>();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("wx", addedEmployee.Name);
            Assert.Equal(1000, addedEmployee.Salary);
        }

        [Fact]
        public async Task Should_delete_specific_employee_with_status_204()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Apple");
            var companyResponse = await httpClient.PostAsJsonAsync("api/companies", companyGiven);
            var company = await companyResponse.Content.ReadFromJsonAsync<Company>();

            var employeeRequest = new EmployeeRequest
            {
                Name = "wx",
                Salary = 1000,
                CompanyId = company.Id
            };

            var response = await httpClient.PostAsJsonAsync($"/api/companies/{company.Id}/employees", employeeRequest);
            var addedEmployee = await response.Content.ReadFromJsonAsync<Employee>();
            var response2 = await httpClient.DeleteAsync($"/api/companies/{company.Id}/employees/{addedEmployee.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
            var deletedEmployee = company.Employees.FirstOrDefault(e => e.Id == addedEmployee.Id);
            Assert.Null(deletedEmployee);
        }

        [Fact]
        public async Task Should_return_list_of_employees_by_company_with_status_200()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Apple");
            var companyResponse = await httpClient.PostAsJsonAsync("api/companies", companyGiven);
            var company = await companyResponse.Content.ReadFromJsonAsync<Company>();

            var employeeRequest = new EmployeeRequest
            {
                Name = "wx",
                Salary = 1000,
                CompanyId = company.Id
            };

            await httpClient.PostAsJsonAsync($"/api/companies/{company.Id}/employees", employeeRequest);
            var response2 = await httpClient.GetAsync($"/api/companies/{company.Id}/employees");
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            var employees = await response2.Content.ReadFromJsonAsync<List<Employee>>();
            Assert.NotNull(employees);
            Assert.NotEmpty(employees);
        }

        private async Task<T?> DeserializeTo<T>(HttpResponseMessage httpResponseMessage)
        {
            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            T? deserializedObject = JsonConvert.DeserializeObject<T>(response);
            return deserializedObject;
        }

        private static StringContent SerializeObjectToContent<T>(T objectGiven)
        {
            return new StringContent(JsonConvert.SerializeObject(objectGiven), Encoding.UTF8, "application/json");
        }

        private async Task ClearDataAsync()
        {
            await httpClient.DeleteAsync("/api/companies");
        }
    }
}