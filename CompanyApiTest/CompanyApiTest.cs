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
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            
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
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");

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
        public async Task Should_return_all_companies_with_status_200_when_get_all()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Google");
            await httpClient.PostAsJsonAsync("/api/companies", companyGiven);

            HttpResponseMessage responseMessage = await httpClient.GetAsync("/api/companies");
            var companyList = await responseMessage.Content.ReadFromJsonAsync<List<Company>>();

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.Equal(companyGiven.Name, companyList[0].Name);
        }

        [Fact]
        public async Task Should_return_existing_company_with_status_200_when_get_by_id_given_company_id()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Google");
            HttpResponseMessage responseMessagePost = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);

            var companyReceive = await responseMessagePost.Content.ReadFromJsonAsync<Company>();


            HttpResponseMessage responseMessage = await httpClient.GetAsync("/api/companies/" + companyReceive.Id);
            var resultCompany = await responseMessage.Content.ReadFromJsonAsync<Company>();

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.Equal(companyReceive.Name, resultCompany.Name);
        }

        [Fact]
        public async Task Should_return_status_404_when_get_by_id_given_not_exist()
        {
            await ClearDataAsync();
            HttpResponseMessage responseMessage = await httpClient.GetAsync("/api/companies/" + "Google");
            var resultCompany = await responseMessage.Content.ReadFromJsonAsync<Company>();

            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_status_200_when_get_company_with_paging_given_page_size_and_page_index()
        {
            await ClearDataAsync();
            int pageSize = 2;
            int pageIndex = 0;

            for(int i = 0; i < 3; i++)
            {
                CreateCompanyRequest companyGiven = new CreateCompanyRequest($"Company{i}");
                await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            }

            HttpResponseMessage responseMessage = await httpClient.GetAsync($"/api/companies?pageSize={pageSize}&pageIndex={pageIndex}" );
            var resultCompany = await responseMessage.Content.ReadFromJsonAsync<List<Company>>();


            Assert.Equal(pageSize, resultCompany.Count);
            Assert.Equal("Company0", resultCompany[0].Name);
            Assert.Equal("Company1", resultCompany[1].Name);
        }
    }
}