
using System.Net;
using NUnit.Framework;
using RestSharp;
using System.Configuration;
using APITesting.Models;
using Newtonsoft.Json;

namespace APITesting.Tests;
[TestFixture]
public class Current
{
    
    private IRestClient _restClient;
    private RestRequest _restRequest;
    private RestResponse _restResponse;
    private string _token;
    private string BaseUrl;// = "https://api.openweathermap.org/data/2.5/";
    
    [SetUp]
    public void Setup()
    {
        _restClient = new RestClient(ConfigurationManager.AppSettings["API_BASE_URL"]);
        _token = ConfigurationManager.AppSettings["API_TOKEN"];
    }
        
    [TestCase(44.9855, 19.6214, HttpStatusCode.OK)]
    [TestCase(0, 0, HttpStatusCode.OK)]
    [TestCase(600, 44.9855, HttpStatusCode.BadRequest)]
    [TestCase(19.6214, 485, HttpStatusCode.BadRequest)]
    [TestCase(500, 500, HttpStatusCode.BadRequest)]
    public void GetCurrentWeatherForCoordinates(double lat, double lon, HttpStatusCode expectCode = HttpStatusCode.OK)
    {
        //Arrange
        _restRequest = new RestRequest($"weather?lat={lat}&lon={lon}&appid={_token}&units=metric");
                
        //Act
        _restResponse = _restClient.Execute(_restRequest);
               
        //Assert
        Assert.That(_restResponse.StatusCode, Is.EqualTo(expectCode));
        //var mita = _restResponse.des;
        var jsonObj = new JsonSerializer().Deserialize<WeatherResponse.WeatherResponseData>(new JsonTextReader(new StringReader(_restResponse.Content)));
    }
    
    [TestCase("invalid")]
    [TestCase("")]
    [TestCase("0000")]
    [TestCase("-1")]
    [TestCase("%12")]
    [TestCase("noAppId")]
    public void GetCurrentWeatherInvalidToken(string token, double lat=44.9855, double lon=19.6214, HttpStatusCode expectCode=HttpStatusCode.Unauthorized)
    {
        //Arrange
        if(token == "noAppId")
            _restRequest = new RestRequest($"weather?lat={lat}&lon={lon}&units=metric");
        else
            _restRequest = new RestRequest($"weather?lat={lat}&lon={lon}&appid={token}&units=metric");
                
        //Act
        _restResponse = _restClient.Execute(_restRequest);
               
        //Assert
        Assert.That(_restResponse.StatusCode, Is.EqualTo(expectCode));
    }
    
    [TestCase("standard")]
    [TestCase("metric")]
    [TestCase("imperial")]
    public void GetCurrentWeatherCheckUnits(string units, double lat=44.985, double lon=19.6214)
    {
        //Arrange
        _restRequest = new RestRequest($"weather?lat={lat}&lon={lon}&appid={_token}&units={units}");
                
        //Act
        _restResponse = _restClient.Execute(_restRequest);
               
        //Assert
        Assert.That(_restResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var jsonObj = new JsonSerializer().Deserialize<WeatherResponse.WeatherResponseData>(new JsonTextReader(new StringReader(_restResponse.Content)));
        switch (units)
        {
            case "standard":
                Assert.Multiple(() =>
                {
                    Assert.That(jsonObj.main.temp > 193,
                        "Temperature does not look good in Kelvins (it's too low), is it end of the world or conversion does not work?");
                    Assert.That(jsonObj.main.temp < 353,
                        "Temperature does not look good in Kelvins (it's too high), is it end of the world or conversion does not work?");
                    //Assert.That(jsonObj.);
                });
                break;
            case "metric":
                Assert.Multiple(() =>
                {
                    Assert.That(jsonObj.main.temp > -85,
                        "Temperature does not look good in Celsius (it's too low), is it end of the world or conversion does not work?");
                    Assert.That(jsonObj.main.temp < 85,
                        "Temperature does not look good in Celsius (it's too high), is it end of the world or conversion does not work?");
                });
                break;
            case "imperial":
                Assert.Multiple(() =>
                {
                    Assert.That(jsonObj.main.temp > -112,
                        "Temperature does not look good in Fahrenheit (it's too low), is it end of the world or conversion does not work?");
                    Assert.That(jsonObj.main.temp < 176,
                        "Temperature does not look good in Fahrenheit (it's too high), is it end of the world or conversion does not work?");
                });
                break;
        }
        
    }
    
    [TestCase("hr")]
    [TestCase("sr")]
    public void GetCurrentWeatherLanguageTests( string lang,double lat = 44.9855, double lon = 19.6214, HttpStatusCode expectCode = HttpStatusCode.OK)
    {
        //Arrange
        _restRequest = new RestRequest($"weather?lat={lat}&lon={lon}&appid={_token}&units=metric&lang={lang}");
                
        //Act
        _restResponse = _restClient.Execute(_restRequest);
               
        //Assert
        Assert.That(_restResponse.StatusCode, Is.EqualTo(expectCode));
        //ToDo: perhaps create a list of possible cloud translations and compare them to whatever is returned.
        //ToDo: example: if the word returned from API is found in the list of possible words for given language
        //var jsonObj = new JsonSerializer().Deserialize<WeatherResponse.WeatherResponseData>(new JsonTextReader(new StringReader(_restResponse.Content)));

    }
    
    [TestCase("xml")]
    [TestCase("html")]
    public void GetCurrentWeatherModesTests(string mode,double lat = 44.9855, double lon = 19.6214, HttpStatusCode expectCode = HttpStatusCode.OK)
    {
        //Arrange
        _restRequest = new RestRequest($"weather?lat={lat}&lon={lon}&appid={_token}&units=metric&mode={mode}");
                
        //Act
        _restResponse = _restClient.Execute(_restRequest);
               
        //Assert
        Assert.That(_restResponse.StatusCode, Is.EqualTo(expectCode), "Response status code does not match");
       
        switch (mode)
        {
            case "xml":
                Assert.That(_restResponse.Content.StartsWith("<?xml version="), "Not valid format (expected xml)");
                break;
            case "html":
                Assert.That(_restResponse.Content.StartsWith("<!DOCTYPE html>\n<html lang"), "Not valid format (expected html)");
                break;
        }

    }
}