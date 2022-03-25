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
    public class PairConversionTests
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

        [TestCase("USD", "EUR")]
        [TestCase("EUR", "GBP")]
        [TestCase("GBP", "CHF")]
        [TestCase("CHF", "PLN")]
        public void GivenValidCurrency_WhenSendGetRequest_ThenSuccess(string baseCurrency, string targetCurrency)
        {
            //Given
            var request = new RestRequest($"{Pair}/{baseCurrency}/{targetCurrency}");

            //When
            var response = _client.ExecuteAsync<Rate>(request).Result;

            //Then
            Assert.That(response.Data.result, Is.EqualTo("success"));
            Assert.That(response.Data.base_code, Is.EqualTo(baseCurrency));
            Assert.That(response.Data.target_code, Is.EqualTo(targetCurrency));
        }

        [Test]
        public void GivenValidCurrency_WhenSendGetRequest_ThenConversionRateIsCalculatedProperly()
        {
            //Given
            var request = new RestRequest($"{Pair}/USD/USD");

            //When
            var response = _client.ExecuteAsync<Rate>(request).Result;

            //Then
            Assert.That(response.Data.result, Is.EqualTo("success"));
            Assert.That(response.Data.base_code, Is.EqualTo("USD"));
            Assert.That(response.Data.target_code, Is.EqualTo("USD"));
            Assert.That(response.Data.conversion_rate, Is.EqualTo(1));
        }

        #region Validation tests

        [Test]
        public void GivenInvalidCurrencyCode_WhenSendGetRequest_ThenUnsupportedCodeErrorReturned()
        {
            //Given
            var request = new RestRequest($"{Pair}/USD/PPP");

            //When
            var response = _client.ExecuteAsync<Rate>(request).Result;

            //Then
            var responseErrorType = JObject.Parse(response.Content).GetValue(ErrorType);

            Assert.That(response.Data.result, Is.EqualTo("error"));
            Assert.That(responseErrorType.ToString(), Is.EqualTo(UnsupportedCode));
        }

        [Test]
        public void GivenInvalidCurrencyCode_WhenSendGetRequest_ThenMalformedErrorReturned()
        {
            //Given
            var request = new RestRequest($"{Pair}/USD/UnsupportedRateCode");

            //When
            var response = _client.ExecuteAsync<Rate>(request).Result;

            //Then
            var responseErrorType = JObject.Parse(response.Content).GetValue(ErrorType);

            Assert.That(response.Data.result, Is.EqualTo("error"));
            Assert.That(responseErrorType.ToString(), Is.EqualTo(MalformedRequest));
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
            var request = new RestRequest($"{Pair}/USD/USD");

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
            var request = new RestRequest($"{Pair}/USD/USD");

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
