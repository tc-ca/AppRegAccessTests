using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace AppRegAccessTests.Pages
{
    public class GetEnvModel : PageModel
    {
        private readonly IConfiguration _config;
        public GetEnvModel(IConfiguration config)
        {
            _config = config;
        }
        public string? DigiSignEnv { get; set; }
        public string? DocSvcEnv { get; private set; }
        public string ImageUrl { get; set; }

        public async Task OnGetAsync()
        {
            DigiSignEnv = await GetDigiSignEnvironment();
            DocSvcEnv = await GetDocSvcEnvironment(true);
        }

        private async Task<string> GetDocSvcEnvironment(bool getImage)
        {
            AuthenticationResult? result = null;
            IConfidentialClientApplication app;
            string? env = string.Empty;

            string tenantId = _config["AzureAd:TenantId"];
            string resource = _config["AzureAd:Audience"] + "/.default";
            string instance = _config["AzureAd:Instance"];
            //Doc svc
            //string clientId = _config["DocService:ClientId"];
            //string clientSecret = _config["DocService:ClientSecret"];

            //Mitrack svc
            string clientId = _config["DocService:ClientId"];
            string clientSecret = _config["DocService:ClientSecret"];

            try
            {

                app = ConfidentialClientApplicationBuilder.Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri(instance + tenantId)).Build();
                string[] ResourceIds = { resource
                    //"https://034gc.onmicrosoft.com/ncd-dms-dev" + "/DocumentService.Read.All",
                    //"https://034gc.onmicrosoft.com/ncd-dms-dev/DocumentService.CreateUpdate.All" + "/.default"
            };
                result = await app.AcquireTokenForClient(ResourceIds).ExecuteAsync();                

                Debug.WriteLine(@"Token acquired \n"); 
                Debug.WriteLine(result.AccessToken);

                using var client = new HttpClient();

                // This returns 403 forbidden: 
                //client.BaseAddress = new Uri("https://ncdsafsecsurapp-document-management-service.azurewebsites.net");

                // This works. Clone and run this locally https://github.com/tc-ca/DocumentService.git comment out attribute: //[RequiredScope(RequiredScopesConfigurationKey = ScopePolicy.ReadWritePermission)]
                client.BaseAddress = new Uri("https://localhost:44341"); 

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var response = await client.GetAsync("/api/v1/documents/environment");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync(); using var responseStream = await response.Content.ReadAsStreamAsync();
                    env = await System.Text.Json.JsonSerializer.DeserializeAsync<string>(responseStream);
                }

                if (getImage)
                {
                    response = await client.GetAsync("/api/v1/documents/viewlink/8de99489-603f-4b4d-9b38-5d2761f15459");
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = await response.Content.ReadAsStringAsync(); using var responseStream = await response.Content.ReadAsStreamAsync();
                        ImageUrl = await System.Text.Json.JsonSerializer.DeserializeAsync<string>(responseStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return env;
        }

        private async Task<string> GetDigiSignEnvironment()
        {
            AuthenticationResult? result = null;
            IConfidentialClientApplication app;
            string? env = string.Empty;

            string tenantId = _config["AzureAd:TenantId"];
            string clientId = _config["DigiSignService:ClientId"];
            string clientSecret = _config["DigiSignService:ClientSecret"];
            string resource = "api://" + clientId + "/.default";
            string instance = _config["AzureAd:Instance"];
            try
            {
                app = ConfidentialClientApplicationBuilder
                        .Create(clientId)
                        .WithClientSecret(clientSecret)
                        .WithAuthority(new Uri(instance + tenantId)).Build();
                string[] ResourceIds = {resource};

                result = await app.AcquireTokenForClient(ResourceIds).ExecuteAsync();

                Debug.WriteLine(@"Token acquired \n");
                Debug.WriteLine(result.AccessToken);

                using var client = new HttpClient();
                client.BaseAddress = new Uri("https://tc-digital-signature.azurewebsites.net/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync("/api/v1/Documents/CurrentEnvironment");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync(); using var responseStream = await response.Content.ReadAsStreamAsync();
                    env = await System.Text.Json.JsonSerializer.DeserializeAsync<string>(responseStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
            return env;
        }
    }
}
