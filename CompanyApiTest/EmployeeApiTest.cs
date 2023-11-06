/*using CompanyApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace CompanyApiTest
{
    public class EmployeeApiTest
    {
        private HttpClient httpClient;

        public EmployeeApiTest()
        {
            WebApplicationFactory<Program> webApplicationFactory = new WebApplicationFactory<Program>();
            httpClient = webApplicationFactory.CreateClient();
        }

        [Fact]
        public async Task Should_add_employee_to_specific_company_with_status_201()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("ACME Inc.");
            var companyResponse = await httpClient.PostAsJsonAsync("api/companies", companyGiven);
            var company = await companyResponse.Content.ReadFromJsonAsync<Company>();

            EmployeeRequest employeeRequest = new EmployeeRequest
            {
                Name = "wx",
                Salary = 1000
            };


            var response = await httpClient.PostAsJsonAsync($"api/companies/{company.Id}/employees", employeeRequest);
            var addedEmployee = await response.Content.ReadFromJsonAsync<Employee>();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(addedEmployee);
            Assert.Equal("wx", addedEmployee.Name);
            Assert.Equal(1000, addedEmployee.Salary);
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
*/