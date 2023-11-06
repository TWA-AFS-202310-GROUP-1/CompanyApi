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

            //When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync<CreateCompanyRequest>("/api/companies", companyGiven);

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
            await httpClient.PostAsJsonAsync<CreateCompanyRequest>("/api/companies", companyGiven);
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync<CreateCompanyRequest>("/api/companies", companyGiven);

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
        public async Task Should_return_all_companies_with_status_200_when_get_all_companies()
        {
            //Given
            await ClearDataAsync();
            CreateCompanyRequest newCompany = new CreateCompanyRequest("company1");

            //When
            await httpClient.PostAsJsonAsync("api/companies", newCompany);
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("api/companies");
            List<Company> companies = await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>();

            //Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(1, companies.Count); 
            Assert.Equal("company1", companies[0].Name);
        }

        [Fact]
        public async Task Should_return_company_with_status_200_when_given_id_and_find_company()
        {
            //Given
            await ClearDataAsync();
            CreateCompanyRequest newCompany = new CreateCompanyRequest("company1");

            //When
            HttpResponseMessage postResponseMessage = await httpClient.PostAsJsonAsync("api/companies", newCompany);
            var companyReceive = await postResponseMessage.Content.ReadFromJsonAsync<Company>();
            HttpResponseMessage responseMessage = await httpClient.GetAsync("/api/companies/"+ companyReceive.Id);
            var company = await responseMessage.Content.ReadFromJsonAsync<Company>();

            //Then
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.Equal(companyReceive.Name, company.Name);
        }

        [Fact]
        public async Task Should_return_status_404_when_given_id_but_not_find_company()
        {
            //Given
            await ClearDataAsync();
            CreateCompanyRequest newCompany = new CreateCompanyRequest("company1");

            //When
            HttpResponseMessage responseMessage = await httpClient.GetAsync("/api/companies/company1");
            var company = await responseMessage.Content.ReadFromJsonAsync<Company>();

            //Then
            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_X_companies_from_Y_Page_when_given_X_and_Y()
        {
            //Given
            await ClearDataAsync();
            for(var i = 0; i < 4; i++)
            {
                CreateCompanyRequest newCompany = new CreateCompanyRequest("company"+(i+1).ToString());
                await httpClient.PostAsJsonAsync("api/companies", newCompany);
            }

            //When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies/details" + "?pageSize=2&&pageIndex=2");
            var companies = await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>();

            //Then
            Assert.Equal(2, companies.Count);
            Assert.Equal("company3", companies[0].Name);
            Assert.Equal("company4", companies[1].Name);
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