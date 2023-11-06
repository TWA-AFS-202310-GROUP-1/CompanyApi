using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
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

        [Fact]
        public async Task Should_return_all_companies_with_status_200_when_get_all_companies()
        {
            // Given
            await ClearDataAsync();
            Company companiesGiven = new Company("Company 1");

            await httpClient.PostAsJsonAsync("api/companies", companiesGiven);

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies");

            // Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            List<Company> companiesReturned = await DeserializeTo<List<Company>>(httpResponseMessage);
            Assert.NotNull(companiesReturned);
            Assert.Equal(companiesGiven.Name, companiesReturned[0].Name);
        }

        [Fact]
        public async Task Should_return_company_with_status_200_when_get_specific_company()
        {
            // Given
            await ClearDataAsync();
            string companyName = "Company 1";
            Company companiesGiven = new Company(companyName);

            await httpClient.PostAsJsonAsync("api/companies", companiesGiven);

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies/Company 1");

            // Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Company companiesReturned = await DeserializeTo<Company>(httpResponseMessage);
            //Assert.NotNull(companiesReturned);
            Assert.Equal(companyName, companiesReturned.Name);
        }

        [Fact]
        public async Task Should_return_lists_when_get_companies_given_page_and_page_size()
        {
            // Given
            await ClearDataAsync();
            Company companyGiven1 = new Company("Company 1");
            Company companyGiven2 = new Company("Company 2");
            Company companyGiven3 = new Company("Company 3");
            //When

            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "/api/companies",
                SerializeObjectToContent(companyGiven1)
            );

            HttpResponseMessage httpResponseMessage2 = await httpClient.PostAsync(
                "/api/companies",
                SerializeObjectToContent(companyGiven2)
            );

            HttpResponseMessage httpResponseMessage3 = await httpClient.PostAsync(
                "/api/companies",
                SerializeObjectToContent(companyGiven3)
            );

            HttpResponseMessage httpResponseMessage4 = await httpClient.GetAsync(
                "/api/companies/1/1"
            );

            // Then
            List<Company>? company = await DeserializeTo<List<Company>>(httpResponseMessage4);

            Assert.Equal(HttpStatusCode.OK, httpResponseMessage4.StatusCode);
            Assert.Equal(companyGiven1.Name, company[0].Name);
        }

        [Fact]
        public async Task Should_return_updated_company_with_status_200_when_put()
        {
            // Given
            await ClearDataAsync();
            Company companyGiven = new Company("Company 1");

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "/api/companies",
                SerializeObjectToContent(companyGiven)
            );

            Company? company = await DeserializeTo<Company>(httpResponseMessage);

            Company newCompany = new Company("new Company 1");

            HttpResponseMessage httpResponseMessage2 = await httpClient.PutAsync(
                "/api/companies/" + company.Id,
                SerializeObjectToContent(newCompany)
            );

            // Then
            Company? company2 = await DeserializeTo<Company>(httpResponseMessage2);

            Assert.Equal(HttpStatusCode.OK, httpResponseMessage2.StatusCode);
            Assert.Equal(newCompany.Name, company2.Name);
        }

        [Fact]
        public async Task Should_return_employee_with_status_201_when_create_employee_given_company_id_and_employee_info()
        {
            // Given
            await ClearDataAsync();
            var companyGiven = new CreateCompanyRequest
            {
                Name = "Company 1"
            };
            var httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var companyCreated = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            Assert.NotNull(companyCreated);
            Assert.NotNull(companyCreated.Id);
            var employeeGiven = new CreateEmployeeRequest
            {
                Name = "Peter",
                Salary = 5000,
                CompanyId = companyCreated.Id
            };

            // When
            httpResponseMessage = await httpClient.PostAsJsonAsync($"/api/companies/{companyCreated.Id}/employees", employeeGiven);

            // Then
            Assert.Equal(HttpStatusCode.Created, httpResponseMessage.StatusCode);
            var employeeCreated = await httpResponseMessage.Content.ReadFromJsonAsync<Employee>();
            Assert.NotNull(employeeCreated);
            Assert.NotNull(employeeCreated.Id);
            Assert.Equal(employeeGiven.Name, employeeCreated.Name);
            Assert.Equal(employeeGiven.Salary, employeeCreated.Salary);
            Assert.Equal(companyCreated.Id, employeeCreated.CompanyId);
        }

        [Fact]
        public async Task Should_return_404_when_create_employee_given_fake_company_id()
        {
            // Given
            await ClearDataAsync();
            var employeeGiven = new CreateEmployeeRequest
            {
                Name = "Peter",
                Salary = 5000,
                CompanyId = Guid.NewGuid().ToString()
            };

            // When
            var httpResponseMessage = await httpClient.PostAsJsonAsync($"/api/companies/{employeeGiven.CompanyId}/employees", employeeGiven);

            // Then
            Assert.Equal(HttpStatusCode.NotFound, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_no_content_when_delete_employee_given_existing_employee_id()
        {
            // Given
            await ClearDataAsync();
            var companyGiven = new CreateCompanyRequest
            {
                Name = "Company 1"
            };
            var httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var companyCreated = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            Assert.NotNull(companyCreated);
            Assert.NotNull(companyCreated.Id);
            var employeeGiven = new CreateEmployeeRequest
            {
                Name = "Peter",
                Salary = 5000,
                CompanyId = companyCreated.Id
            };
            httpResponseMessage = await httpClient.PostAsJsonAsync($"/api/companies/{companyCreated.Id}/employees", employeeGiven);
            var employeeCreated = await httpResponseMessage.Content.ReadFromJsonAsync<Employee>();
            Assert.NotNull(employeeCreated);
            Assert.NotNull(employeeCreated.Id);
            var companyAll = await httpClient.GetFromJsonAsync<List<Company>>("/api/companies");

            // When
            httpResponseMessage = await httpClient.DeleteAsync($"/api/companies/{companyCreated.Id}/employees/{employeeCreated.Id}");

            // Then
            Assert.Equal(HttpStatusCode.NoContent, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_404_when_delete_employee_given_fake_company_id()
        {
            // Given
            await ClearDataAsync();
            var httpResponseMessage = await httpClient.DeleteAsync($"/api/companies/{Guid.NewGuid().ToString()}/employees/{Guid.NewGuid().ToString()}");

            // When
            Assert.Equal(HttpStatusCode.NotFound, httpResponseMessage.StatusCode);
        }
    }
}