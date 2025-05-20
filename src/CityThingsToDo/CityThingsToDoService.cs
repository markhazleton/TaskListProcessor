namespace CityThingsToDo;

/// <summary>
/// Provides a list of things to do for various cities, simulating a data source for demonstration purposes.
/// </summary>
public class CityThingsToDoService
{
    public CityThingsToDoService()
    {
        activitiesByCity.Add("Rome",
            [
                new("Tour the Colosseum", 12),
                new("Visit the Vatican Museums", 20),
                new("Explore the Pantheon", 0),
                new("Toss a coin into Trevi Fountain", 0)
                ]);
        activitiesByCity.Add("London",
            [
                new("Ride the London Eye", 30),
                new("Tour the Tower of London", 25),
                new("Visit the British Museum", 0),
                new("Explore Camden Market", 0)
                ]);
        activitiesByCity.Add("Chicago",
            [
                new("Visit the Art Institute of Chicago", 25),
                new("Take an architecture river cruise", 40),
                new("Stand on the Skydeck at Willis Tower", 23),
                new("Explore Millennium Park", 0)
                ]);
        activitiesByCity.Add("Dallas",
            [
                new("Visit the Sixth Floor Museum at Dealey Plaza", 18),
                new("Stroll through the Dallas Arboretum and Botanical Garden", 15),
                new("Experience the Perot Museum of Nature and Science", 20),
                new("Explore the Dallas World Aquarium", 25)
                ]);
        activitiesByCity.Add("Houston",
            [
                new("Space Center Houston tour", 30),
                new("Visit the Houston Museum of Natural Science", 20),
                new("Explore the Houston Zoo", 18),
                new("Relax in Hermann Park", 0)
                ]);
        activitiesByCity.Add("Wichita",
            [
                new("Visit the Sedgwick County Zoo", 15),
                new("Explore the Botanica Wichita Gardens", 10),
                new("Discover the Old Cowtown Museum", 9),
                new("Tour the Wichita Art Museum", 5)
                ]);
    }

    // A mock database of things to do in various cities
    private readonly Dictionary<string, List<Activity>> activitiesByCity = new()
    {
        {
            "Paris", new List<Activity>
            {
                new("Visit the Louvre Museum", 15),
                new("Climb the Eiffel Tower", 25),
                new("Walk along the Seine", 0),
                new("Explore Montmartre", 0)
            }
        },
        {
            "New York", new List<Activity>
            {
                new("See a Broadway show", 120),
                new("Visit Central Park", 0),
                new("Tour the Metropolitan Museum of Art", 25),
                new("Walk the High Line", 0)
            }
        },
        // Add more cities and activities as needed
    };

    /// <summary>
    /// Gets a list of things to do for a given city asynchronously.
    /// </summary>
    /// <param name="city">The name of the city.</param>
    /// <returns>A task that represents the asynchronous operation and contains the list of activities for the city.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the city argument is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when no activities are found for the specified city.</exception>
    public async Task<IEnumerable<Activity>> GetThingsToDoAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentNullException(nameof(city), "City name cannot be null or empty.");
        }

        await Task.Delay(Random.Shared.Next(100, 1000)); // Simulate latency

        if (activitiesByCity.TryGetValue(city, out var activities))
        {
            return activities;
        }
        else
        {
            throw new ArgumentException($"No activities found for the city: {city}", nameof(city));
        }
    }

    /// <summary>
    /// Represents an activity with a name and a price per person.
    /// </summary>
    /// <param name="name">The name of the activity.</param>
    /// <param name="pricePerPerson">The price per person for the activity.</param>
    public class Activity(string name, decimal pricePerPerson)
    {
        /// <summary>
        /// Gets the name of the activity.
        /// </summary>
        public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
        /// <summary>
        /// Gets the price per person for the activity.
        /// </summary>
        public decimal PricePerPerson { get; } = pricePerPerson;

        /// <summary>
        /// Returns a string that represents the current activity.
        /// </summary>
        /// <returns>A string that represents the current activity.</returns>
        public override string ToString()
        {
            return $"{Name} - {PricePerPerson:C}";
        }
    }
}
