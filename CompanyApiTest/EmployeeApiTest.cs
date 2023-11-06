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

        [Fact]
        public async Task Should_return_bad_reqeust_when_add_employee_given_an_existed_employee_name()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Google");
            CreateEmployeeRequest employeeGiven = new CreateEmployeeRequest("Jack");

            // When
            HttpResponseMessage httpResponseMessageCompany = await httpClient.PostAsJsonAsync<CreateCompanyRequest>("/api/companies", companyGiven);
            Company companyPost = await DeserializeTo<Company>(httpResponseMessageCompany);
            await httpClient.PostAsJsonAsync($"/api/companies/{companyPost.Id}/employees", employeeGiven);
            HttpResponseMessage httpResponseMessageEmployee = await httpClient.PostAsJsonAsync($"/api/companies/{companyPost.Id}/employees", employeeGiven);

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessageEmployee.StatusCode);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_add_employee_given_an_employee_with_unknown_field()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Google");
            StringContent content = new StringContent("{\"unknownField\": \"Jack\"}", Encoding.UTF8, "application/json");

            // When
            HttpResponseMessage httpResponseMessageCompany = await httpClient.PostAsJsonAsync<CreateCompanyRequest>("/api/companies", companyGiven);
            Company companyPost = await DeserializeTo<Company>(httpResponseMessageCompany);
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync($"/api/companies/{companyPost.Id}/employees", content);

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_delete_an_employee_when_given_existed_employeeID()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Google");
            CreateEmployeeRequest employeeGiven = new CreateEmployeeRequest("Jack");

            // When
            HttpResponseMessage httpResponseMessageCompany = await httpClient.PostAsJsonAsync<CreateCompanyRequest>("/api/companies", companyGiven);
            Company companyPost = await DeserializeTo<Company>(httpResponseMessageCompany);
            HttpResponseMessage httpResponseMessageEmployee = await httpClient.PostAsJsonAsync($"/api/companies/{companyPost.Id}/employees", employeeGiven);
            Employee employeeReceived = await DeserializeTo<Employee>(httpResponseMessageEmployee);
            HttpResponseMessage httpResponseMessage = await httpClient.DeleteAsync($"/api/companies/{companyPost.Id}/employees/{employeeReceived.Id}");

            // Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_delete_fail_when_employee_not_found()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Google");
            CreateEmployeeRequest employeeGiven = new CreateEmployeeRequest("Jack");

            // When
            HttpResponseMessage httpResponseMessageCompany = await httpClient.PostAsJsonAsync<CreateCompanyRequest>("/api/companies", companyGiven);
            Company companyPost = await DeserializeTo<Company>(httpResponseMessageCompany);
            HttpResponseMessage httpResponseMessageEmployee = await httpClient.PostAsJsonAsync($"/api/companies/{companyPost.Id}/employees", employeeGiven);
            Employee employeeReceived = await DeserializeTo<Employee>(httpResponseMessageEmployee);
            HttpResponseMessage httpResponseMessage = await httpClient.DeleteAsync($"/api/companies/{companyPost.Id}/employees/{employeeReceived.Id}new");

            // Then
            Assert.Equal(HttpStatusCode.NotFound, httpResponseMessage.StatusCode);
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
