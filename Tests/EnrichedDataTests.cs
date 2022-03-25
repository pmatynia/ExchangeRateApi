using ExchangeRateApi.Config;
using ExchangeRateApi.Model;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using static ExchangeRateApi.Utilities.Constants;
using static ExchangeRateApi.Utilities.ErrorType;

namespace ExchangeRateApi.Tests
{
    [TestFixture]
    public class EnrichedDataTests
    {
        private RestClient _client;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _client = new RestClient(AppSettings.BaseAddress)
            {
                Authenticator = new JwtAuthenticator(AppSettings.BearerToken)
            };
        }

        #region Validation tests

        [Test]
        public void GivenOperationForBusinessPlan_WhenSendGetRequest_ThenPlanUpgradeRequiredErrorReturned()
        {
            //Given
            var request = new RestRequest($"{Enriched}/GBP/JPY");

            //When
            var response = _client.ExecuteAsync<Rate>(request).Result;

            //Then
            var responseErrorType = JObject.Parse(response.Content).GetValue(ErrorType);

            Assert.That(response.Data.result, Is.EqualTo("error"));
            Assert.That(responseErrorType.ToString(), Is.EqualTo(PlanUpgradeRequired));
        }

        #endregion
    }
}
