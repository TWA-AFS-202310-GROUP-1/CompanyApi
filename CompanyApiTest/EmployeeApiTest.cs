using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        public async Task Should_return_added_employee_with_status_201_when_create_employee_given_a_employee_name()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Google");
            CreateEmployeeRequest employeeGiven = new CreateEmployeeRequest("Jack");

            //When
            HttpResponseMessage httpResponseMessageCompany = await httpClient.PostAsJsonAsync<CreateCompanyRequest>("/api/companies", companyGiven);
            Company companyPost = await DeserializeTo<Company>(httpResponseMessageCompany);
            HttpResponseMessage httpResponseMessageEmployee = await httpClient.PostAsJsonAsync($"/api/companies/{companyPost.Id}/employees", employeeGiven);

            // Then
            Assert.Equal(HttpStatusCode.Created, httpResponseMessageEmployee.StatusCode);
            Employee? employeeCreated = await DeserializeTo<Employee>(httpResponseMessageEmployee);
            Assert.NotNull(employeeCreated);
            Assert.NotNull(employeeCreated.Id);
            Assert.Equal(employeeGiven.Name, employeeCreated.Name);
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
