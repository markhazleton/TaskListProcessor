namespace CityThingsToDo.Tests;

[TestClass]
public class CityThingsToDoServiceTests
{
    private readonly CityThingsToDoService _service;

    public CityThingsToDoServiceTests()
    {
        _service = new CityThingsToDoService();
    }

    [TestMethod]
    public async Task GetThingsToDoAsync_ValidCity_ReturnsActivities()
    {
        // Arrange
        var city = "Rome";

        // Act
        var activities = await _service.GetThingsToDoAsync(city);

        // Assert
        Assert.IsNotNull(activities);
        Assert.IsTrue(activities.Any(), "Expected to find activities for Rome, but found none.");
    }

    [TestMethod]
    public async Task GetThingsToDoAsync_UnknownCity_ThrowsArgumentException()
    {
        // Arrange
        var city = "Atlantis";

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            async () => await _service.GetThingsToDoAsync(city),
            "Expected an ArgumentException for an unknown city, but none was thrown."
        );
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public async Task GetThingsToDoAsync_InvalidCity_ThrowsArgumentNullException(string city)
    {
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            async () => await _service.GetThingsToDoAsync(city),
            "Expected an ArgumentNullException for a null or empty city string, but none was thrown."
        );
    }
}

