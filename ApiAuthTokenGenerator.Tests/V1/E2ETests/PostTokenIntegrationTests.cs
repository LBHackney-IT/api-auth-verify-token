using ApiAuthTokenGenerator.Tests.V1.TestHelper;
using ApiAuthTokenGenerator.V1.Boundary;
using ApiAuthTokenGenerator.V1.Boundary.Request;
using ApiAuthTokenGenerator.V1.Boundary.Response;
using ApiAuthTokenGenerator.V1.Factories;
using ApiAuthTokenGenerator.V1.Helpers;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.Tests.V1.E2ETests
{
    //For guidance on writing integration tests see the wiki page https://github.com/LBHackney-IT/lbh-base-api/wiki/Integration-Tests
    public class PostTokenIntegrationTests : IntegrationTestsPost<Startup>
    {
        private readonly Faker _faker = new Faker();

        [Test]
        public async Task CanGenerateAnAuthTokenAsync()
        {
            var tokenRequest = new TokenRequestObject
            {
                Consumer = _faker.Random.AlphaNumeric(10),
                ConsumerType = _faker.Random.Int(5),
                ExpiresAt = _faker.Date.Future(),
                ApiEndpoint = _faker.Random.Int(0, 10),
                ApiName = _faker.Random.Int(0, 10),
                AuthorizedBy = _faker.Person.Email,
                DateRequested = DateTime.Now,
                Environment = _faker.Random.AlphaNumeric(5),
                RequestedBy = _faker.Person.Email
            };
            Environment.SetEnvironmentVariable("jwtSecret", _faker.Random.String());
            var jwtSecret = Environment.GetEnvironmentVariable("jwtSecret");

            var url = new Uri($"/api/v1/tokens", UriKind.Relative);
            var content = new StringContent(JsonConvert.SerializeObject(tokenRequest), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content).ConfigureAwait(true);
            content.Dispose();

            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var apiResponse = JsonConvert.DeserializeObject<GenerateTokenResponse>(data);

            var claimsDecrypted = ValidateJwtTokenHelper.GetJwtClaims(apiResponse.Token, jwtSecret);

            response.StatusCode.Should().Be(201);

            claimsDecrypted.Find(x => x.Type == "id").Value.Should().Be(apiResponse.Id.ToString(CultureInfo.InvariantCulture));
            claimsDecrypted.Find(x => x.Type == "consumerName").Value.Should().Be(tokenRequest.Consumer);
            claimsDecrypted.Find(x => x.Type == "consumerType").Value.Should()
                .Be(tokenRequest.ConsumerType.ToString(CultureInfo.InvariantCulture));
            apiResponse.Should().BeOfType<GenerateTokenResponse>();
            apiResponse.GeneratedAt.Date.Should().Be(DateTime.Now.Date);
            apiResponse.ExpiresAt.Value.Should().BeSameDateAs(tokenRequest.ExpiresAt.Value);
        }
        [Test]
        public async Task Return400IfRequestParametersAreMissing()
        {
            var request = new TokenRequestObject()
            {
                ApiEndpoint = 1,
                ApiName = 2,
                Consumer = "test"
            };

            var url = new Uri($"/api/v1/tokens", UriKind.Relative);
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content).ConfigureAwait(true);
            content.Dispose();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task Return400IfExpiryDateSuppliedIsNotInTheFuture()
        {
            var tokenRequest = new TokenRequestObject
            {
                Consumer = _faker.Random.AlphaNumeric(10),
                ConsumerType = _faker.Random.Int(5),
                ExpiresAt = _faker.Date.Past(),
                ApiEndpoint = _faker.Random.Int(0, 10),
                ApiName = _faker.Random.Int(0, 10),
                AuthorizedBy = _faker.Person.Email,
                DateRequested = DateTime.Now,
                Environment = _faker.Random.AlphaNumeric(5),
                RequestedBy = _faker.Person.Email
            };

            var url = new Uri($"/api/v1/tokens", UriKind.Relative);
            var content = new StringContent(JsonConvert.SerializeObject(tokenRequest), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content).ConfigureAwait(true);
            content.Dispose();
            response.StatusCode.Should().Be(400);
        }
        [Test]
        public async Task Returns201IfAllRequestParametersButExpiresAtAreSupplied()
        {
            var tokenRequest = new TokenRequestObject
            {
                Consumer = _faker.Random.AlphaNumeric(10),
                ConsumerType = _faker.Random.Int(5),
                ApiEndpoint = _faker.Random.Int(0, 10),
                ApiName = _faker.Random.Int(0, 10),
                AuthorizedBy = _faker.Person.Email,
                DateRequested = DateTime.Now,
                Environment = _faker.Random.AlphaNumeric(5),
                RequestedBy = _faker.Person.Email
            };
            Environment.SetEnvironmentVariable("jwtSecret", _faker.Random.String());

            var url = new Uri($"/api/v1/tokens", UriKind.Relative);
            var content = new StringContent(JsonConvert.SerializeObject(tokenRequest), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content).ConfigureAwait(true);
            content.Dispose();
            response.StatusCode.Should().Be(201);
        }
    }
}