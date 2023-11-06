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
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            
            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
           
            // Then
            Assert.Equal(HttpStatusCode.Created, httpResponseMessage.StatusCode);
            Company companyCreated = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            Assert.NotNull(companyCreated);
            Assert.NotNull(companyCreated.Id);
            Assert.Equal(companyGiven.Name, companyCreated.Name);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_create_company_given_a_existed_company_name()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");

            // When
            await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
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

        private async Task ClearDataAsync()
        {
            await httpClient.DeleteAsync("/api/companies");
        }

        [Fact]
        public async Task Should_return_all_companies_with_status_200_when_get_all_given_nothing()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven1 = new CreateCompanyRequest("BlueSky Digital Media");
            CreateCompanyRequest companyGiven2 = new CreateCompanyRequest("GreenLand Market");
            await httpClient.PostAsJsonAsync("/api/companies", companyGiven1);
            await httpClient.PostAsJsonAsync("/api/companies", companyGiven2);

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies");
            List<Company> responseCompanies = await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>();

            // Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(companyGiven1.Name, responseCompanies[0].Name);
            Assert.Equal(companyGiven2.Name, responseCompanies[1].Name);
        }

        [Fact]
        public async Task Should_return_correct_company_with_status_200_when_get_by_name_given_an_existing_company()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyRequest = new CreateCompanyRequest("BlueSky Digital Media");
            var tempMessage = await httpClient.PostAsJsonAsync("/api/companies", companyRequest);
            var createdCompany = await tempMessage.Content.ReadFromJsonAsync<Company>();

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"/api/companies/{createdCompany.Id}");
            Company responseCompany = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();

            // Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal("BlueSky Digital Media", responseCompany.Name);
        }

        [Fact]
        public async Task Should_return_not_found_with_status_404_when_get_by_name_given_an_unvalid_company()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyRequest = new CreateCompanyRequest("BlueSky Digital Media");
            await httpClient.PostAsJsonAsync("/api/companies", companyRequest);

            string wrongId = "123456";

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"/api/companies/{wrongId}");
            Company responseCompany = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();

            // Then
            Assert.Equal(HttpStatusCode.NotFound, httpResponseMessage.StatusCode);
        }
    }
}