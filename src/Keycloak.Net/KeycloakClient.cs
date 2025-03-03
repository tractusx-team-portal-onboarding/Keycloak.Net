using System;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Keycloak.Net.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace Keycloak.Net
{
    public partial class KeycloakClient
    {
        private ISerializer _serializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
        });

        private readonly Url _url;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _clientSecret;
        private readonly Func<Task<string>> _getTokenAsync;
        private readonly string _authRealm;

        private KeycloakClient(string url)
        {
            _url = url;
        }

        public KeycloakClient(string url, string userName, string password, string authRealm = null)
            : this(url)
        {
            _userName = userName;
            _password = password;
            _authRealm = authRealm;
        }

        public KeycloakClient(string url, string clientSecret, string authRealm = null)
            : this(url)
        {
            _clientSecret = clientSecret;
            _authRealm = authRealm;
        }

        public KeycloakClient(string url, Func<string> getToken, string authRealm = null)
            : this(url)
        {
            _getTokenAsync = () => Task.FromResult(getToken());
            _authRealm = authRealm;
        }

        public KeycloakClient(string url, Func<Task<string>> getTokenAsync, string authRealm = null)
            : this(url)
        {
            _getTokenAsync = getTokenAsync;
            _authRealm = authRealm;
        }

        public void SetSerializer(ISerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        private Task<IFlurlRequest> GetBaseUrlAsync(string targetRealm) => new Url(_url)
            .AppendPathSegment("/auth")
            .ConfigureRequest(settings => settings.JsonSerializer = _serializer)
            .WithAuthenticationAsync(_getTokenAsync, _url, _authRealm ?? targetRealm, _userName, _password, _clientSecret);
    }
}
