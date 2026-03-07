namespace DogTeams.Api.Models;

public class Dog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OwnerId { get; set; } = string.Empty;
    public string TeamId { get; set; } = string.Empty;  // denormalized for queries
    public string Name { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
