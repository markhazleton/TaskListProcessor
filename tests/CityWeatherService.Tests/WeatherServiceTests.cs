namespace CityWeatherService.Tests
{
    [TestClass]
    public class WeatherServiceTests
    {
        private WeatherService _weatherService = new();

        [TestInitialize]
        public void Initialize()
        {
            _weatherService = new WeatherService();
        }

        [TestMethod]
        public async Task GetWeather_ShouldReturnForecasts_WhenCityIsValid()
        {
            // Arrange
            var city = "TestCity";

            // Act
            var forecasts = await _weatherService.GetWeather(city, false);

            // Assert
            Assert.IsNotNull(forecasts);
            Assert.AreEqual(5, forecasts.Count());
            Assert.IsTrue(forecasts.All(f => f.City.Equals(city)));
            Assert.IsTrue(forecasts.All(f => f.Summary != null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetWeather_ShouldThrowArgumentNullException_WhenCityIsNull()
        {
            // Act
            await _weatherService.GetWeather(null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetWeather_ShouldThrowArgumentNullException_WhenCityIsEmpty()
        {
            // Act
            await _weatherService.GetWeather(string.Empty, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetWeather_ShouldThrowArgumentNullException_WhenCityIsWhitespace()
        {
            // Act
            await _weatherService.GetWeather(" ", false);
        }

        [TestMethod]
        public async Task GetWeather_ShouldReturnValidTemperatures()
        {
            // Arrange
            var city = "TestCity";

            // Act
            var forecasts = await _weatherService.GetWeather(city, false);

            // Assert
            Assert.IsTrue(forecasts.All(f => f.TemperatureC >= -20 && f.TemperatureC <= 55));
        }
    }
}
