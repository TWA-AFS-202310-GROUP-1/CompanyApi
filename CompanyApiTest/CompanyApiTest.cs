using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Xml.Linq;

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
        public async Task Should_return_all_companies_with_200_code_when_getAllCompany()
        {
            // Given
            await ClearDataAsync();
            Company companyGiven = new Company("BlueSky Digital Media");
           
            // When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies");
            List<Company> companiesExpected = new List<Company>(); 
            // Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(companiesExpected, await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>());
        }


        [Fact]
        public async Task Should_return_correct_company_with_200_code_when_getone_given_correct_code()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media") { Name = "BlueSky Digital Media" };

            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "/api/companies",
                SerializeObjectToContent(companyGiven)
            );
            Company? companyCreated = await DeserializeTo<Company>(httpResponseMessage);
            string? givenId = companyCreated.Id;

            HttpResponseMessage httpResponseMessage1 = await httpClient.GetAsync(
                $"/api/companies/{givenId}");
            
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage1.StatusCode);
            Assert.Equal(companyCreated.Id, (await httpResponseMessage1.Content.ReadFromJsonAsync<Company>()).Id);
        }

        [Fact]
        public async Task Should_return_404_code_when_getone_given_wrong_code()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media") { Name = "BlueSky Digital Media" }; 

            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "/api/companies",
                SerializeObjectToContent(companyGiven)
            );
            Company? companyCreated = await DeserializeTo<Company>(httpResponseMessage);
            string? givenId = "wrongID";

            HttpResponseMessage httpResponseMessage1 = await httpClient.GetAsync(
                $"/api/companies/{givenId}");

            Assert.Equal(HttpStatusCode.NotFound, httpResponseMessage1.StatusCode);
        }

        [Fact]
        public async Task Should_return_companies_by_page_with_200_code_when_requested_by_page_and_size()
        {
            await ClearDataAsync();
            for (int i = 0; i < 10; i++)
            {
                Company companyGiven = new Company($"Company {i + 1}");
                await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
            }

            int pageSize = 5;
            int pageIndex = 2; 

            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"/api/companies/page/{pageSize}/{pageIndex}");

            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            List<Company>? companiesReturned = await DeserializeTo<List<Company>>(httpResponseMessage);
            Assert.Equal(pageSize, companiesReturned.Count);
        }

        [Fact]
        public async Task Should_update_company_info_and_return_204_when_given_correct_id_and_details()
        {
            await ClearDataAsync();
            Company companyGiven = new Company("Initial Company Name");
            HttpResponseMessage initialPostResponse = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
            Company? companyCreated = await DeserializeTo<Company>(initialPostResponse);
            string? givenId = companyCreated.Id;

            Company updatedCompany = new Company("Updated Company Name") { Id = givenId };

            HttpResponseMessage httpResponseMessage = await httpClient.PutAsync($"/api/companies/{givenId}", SerializeObjectToContent(updatedCompany));

            Assert.Equal(HttpStatusCode.NoContent, httpResponseMessage.StatusCode);

            HttpResponseMessage getResponse = await httpClient.GetAsync($"/api/companies/{givenId}");
            Company? companyAfterUpdate = await DeserializeTo<Company>(getResponse);

            Assert.Equal("Updated Company Name", companyAfterUpdate.Name);
        }

        [Fact]
        public async Task Should_add_employee_to_company_successfully()
        {
            await ClearDataAsync();
            Company companyGiven = new Company("Initial Company Name");
            HttpResponseMessage initialPostResponse = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
            Company? companyCreated = await DeserializeTo<Company>(initialPostResponse);
            var employeeRequest = new CreateEmployeeRequest
            {
                Name = "John",
                Salary = 60000
            };

            var response = await httpClient.PostAsync($"/api/companies/{companyCreated.Id}/employees", SerializeObjectToContent(employeeRequest));

            response.EnsureSuccessStatusCode();
            var employee = await DeserializeTo<Employee>(response);
            Assert.NotNull(employee);
            Assert.NotEmpty(employee.Id);
            Assert.Equal(employeeRequest.Name, employee.Name);
            Assert.Equal(employeeRequest.Salary, employee.Salary);
        }

        [Fact]
        public async Task Should_return_404_when_adding_employee_to_non_existing_company()
        {
            var employeeRequest = new CreateEmployeeRequest
            {
                Name = "John",
                Salary = 70000
            };

            var response = await httpClient.PostAsync("/api/companies/non-existing-id/employees", SerializeObjectToContent(employeeRequest));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }



        [Fact]
        public async Task Should_delete_employee_from_company_successfully()
        {
            await ClearDataAsync();
            Company companyGiven = new Company("Initial Company Name");
            HttpResponseMessage initialPostResponse = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
            Company? companyCreated = await DeserializeTo<Company>(initialPostResponse);
            var employeeRequest = new CreateEmployeeRequest
            {
                Name = "John",
                Salary = 60000
            };
            var response = await httpClient.PostAsync($"/api/companies/{companyCreated.Id}/employees", SerializeObjectToContent(employeeRequest));
            var employee = await DeserializeTo<Employee>(response);

            var response1 = await httpClient.DeleteAsync($"/api/companies/{companyCreated.Id}/employees/{employee.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        }

        [Fact]
        public async Task Should_return_404_when_deleting_non_existing_employee()
        {
            await ClearDataAsync();
            Company companyGiven = new Company("Initial Company Name");
            HttpResponseMessage initialPostResponse = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
            Company? companyCreated = await DeserializeTo<Company>(initialPostResponse);

            var response = await httpClient.DeleteAsync($"/api/companies/{companyCreated.Id}/employees/non-existing-id");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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