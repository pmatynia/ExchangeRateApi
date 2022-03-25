using ExchangeRateApi.Config;
using ExchangeRateApi.Model;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System;
using static ExchangeRateApi.Utilities.ErrorType;
using static ExchangeRateApi.Utilities.Constants;

namespace ExchangeRateApi.Tests
{
    [TestFixture]
    public class StandardResponseTests
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

        [TestCase("USD")]
        [TestCase("EUR")]
        [TestCase("GBP")]
        [TestCase("CHF")]
        [TestCase("PLN")]
        public void GivenValidCurrency_WhenSendGetRequest_ThenSuccess(string currency)
        {
            //Given
            var request = new RestRequest($"{LatestRate}/{currency}");

            //When
            var response = _client.ExecuteAsync<Rate>(request).Result;

            //Then
            Assert.That(response.Data.result, Is.EqualTo("success"));
            Assert.That(response.Data.base_code, Is.EqualTo(currency));
        }

        [Test]
        public void GivenValidCurrency_WhenSendGetRequest_ThenConversionRateIsCalculatedProperly()
        {
            //Given
            var request = new RestRequest($"{LatestRate}/USD");

            //When
            var response = _client.ExecuteAsync<Rate>(request).Result;

            //Then
            Assert.That(response.Data.result, Is.EqualTo("success"));
            Assert.That(response.Data.base_code, Is.EqualTo("USD"));
            Assert.That(response.Data.conversion_rates.USD, Is.EqualTo(1));
        }

        #region Validation tests

        [Test]
        public void GivenInvalidCurrencyCode_WhenSendGetRequest_ThenUnsupportedCodeErrorReturned()
        {
            //Given
            var request = new RestRequest($"{LatestRate}/UnsupportedCurrencyCode");

            //When
            var response = _client.ExecuteAsync<Rate>(request).Result;

            //Then
            var responseErrorType = JObject.Parse(response.Content).GetValue(ErrorType);

            Assert.That(response.Data.result, Is.EqualTo("error"));
            Assert.That(responseErrorType.ToString(), Is.EqualTo(UnsupportedCode));
        }

        [Test]
        public void GivenInvalidKey_WhenSendGetRequest_ThenInvalidKeyErrorReturned()
        {
            var randomName = $"InvalidBearer{new Random().Next(1, 100000000)}";

            var client = new RestClient(AppSettings.BaseAddress)
            {
                Authenticator = new JwtAuthenticator(randomName)
            };

            //Given
            var request = new RestRequest($"{LatestRate}/USD");

            //When
            var response = client.ExecuteAsync<Rate>(request).Result;

            //Then
            var responseErrorType = JObject.Parse(response.Content).GetValue(ErrorType);

            Assert.That(response.Data.result, Is.EqualTo("error"));
            Assert.That(responseErrorType.ToString(), Is.EqualTo(InvalidKey));
        }

        [Test]
        public void GivenInvalidHttpMethod_WhenSendGetRequest_ThenIncorrectHttpMethodCodeErrorReturned()
        {
            //Given
            var request = new RestRequest($"{LatestRate}/USD");

            //When
            var response = _client.ExecutePostAsync(request).Result;

            //Then
            var responseErrorType = JObject.Parse(response.Content).GetValue(ErrorType);

            Assert.That(response.ResponseStatus.ToString(), Is.EqualTo("Error"));
            Assert.That(responseErrorType.ToString(), Is.EqualTo(IncorrectHttpMethod));
        }

        #endregion Validation tests
    }
}
