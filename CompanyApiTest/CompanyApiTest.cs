using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using NuGet.Frameworks;
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
            int pageIndex = 1; //pageIndex start with 1

            for (int i = 0; i < 3; i++)
            {
                CreateCompanyRequest companyGiven = new CreateCompanyRequest($"Company{i}");
                await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            }

            HttpResponseMessage responseMessage = await httpClient.GetAsync($"/api/companies?pageSize={pageSize}&pageIndex={pageIndex}");
            var resultCompany = await responseMessage.Content.ReadFromJsonAsync<List<Company>>();

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.Equal(pageSize, resultCompany.Count);
            Assert.Equal("Company0", resultCompany[0].Name);
            Assert.Equal("Company1", resultCompany[1].Name);
        }

        [Fact]
        public async Task Should_return_status_200_when_put_given_request()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Google");
            HttpResponseMessage postResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var companyPost = await postResponseMessage.Content.ReadFromJsonAsync<Company>();

            CreateCompanyRequest companyUpdateGiven = new CreateCompanyRequest("Meta");
            HttpResponseMessage putResponseMessage = await httpClient.PutAsJsonAsync($"/api/companies/{companyPost.Id}", companyUpdateGiven);

            HttpResponseMessage getResponseMessage = await httpClient.GetAsync($"/api/companies/{companyPost.Id}");
            var updatedCompany = await getResponseMessage.Content.ReadFromJsonAsync<Company>();

            Assert.Equal(HttpStatusCode.OK, putResponseMessage.StatusCode);
            Assert.Equal("Meta", updatedCompany.Name);
        }

        [Fact]
        public async Task Should_return_status_404_when_put_given_not_exist()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyUpdateGiven = new CreateCompanyRequest("Meta");
            HttpResponseMessage putResponseMessage = await httpClient.PutAsJsonAsync($"/api/companies/1", companyUpdateGiven);

            Assert.Equal(HttpStatusCode.NotFound, putResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_created_employee_with_status_201_when_create_employee_given_a_employee_under_existing_company()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Google");
            HttpResponseMessage postCompanyResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var companyPost = await postCompanyResponseMessage.Content.ReadFromJsonAsync<Company>();

            CreateEmployeeRequest employeeGiven = new CreateEmployeeRequest("Amy", 10);
            HttpResponseMessage postEmployeeResponseMessage = await httpClient.PostAsJsonAsync($"/api/companies/{companyPost.Id}", employeeGiven);
            var employeePost = await postEmployeeResponseMessage.Content.ReadFromJsonAsync<Employee>();

            Assert.Equal(HttpStatusCode.Created, postEmployeeResponseMessage.StatusCode);
            Assert.Equal("Amy", employeePost.Name);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_create_employee_given_a_existed_employee_name()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Google");
            HttpResponseMessage postCompanyResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var companyPost = await postCompanyResponseMessage.Content.ReadFromJsonAsync<Company>();

            CreateEmployeeRequest employeeGiven = new CreateEmployeeRequest("Amy", 10);
            await httpClient.PostAsJsonAsync($"/api/companies/{companyPost.Id}", employeeGiven);
            HttpResponseMessage postEmployeeResponseMessage = await httpClient.PostAsJsonAsync($"/api/companies/{companyPost.Id}", employeeGiven);

            Assert.Equal(HttpStatusCode.BadRequest, postEmployeeResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_not_found_when_create_employee_given_not_existed_company()
        {
            await ClearDataAsync();
            CreateEmployeeRequest employeeGiven = new CreateEmployeeRequest("Amy", 10);
            HttpResponseMessage postEmployeeResponseMessage = await httpClient.PostAsJsonAsync($"/api/companies/123", employeeGiven);

            Assert.Equal(HttpStatusCode.NotFound, postEmployeeResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_bad_request_when_create_employee_given_a_employee_with_unknown_field()
        {
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Google");
            HttpResponseMessage postCompanyResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var companyPost = await postCompanyResponseMessage.Content.ReadFromJsonAsync<Company>();

            StringContent content = new StringContent("{\"unknownField\": \"BlueSky Digital Media\"}", Encoding.UTF8, "application/json");
            HttpResponseMessage postEmployeeResponseMessage = await httpClient.PostAsJsonAsync($"/api/companies/{companyPost.Id}", content);

            Assert.Equal(HttpStatusCode.BadRequest, postEmployeeResponseMessage.StatusCode);
        }
    }
}