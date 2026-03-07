namespace DogTeams.Api.Models;

public class Owner
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TeamId { get; set; } = string.Empty;  // partition key
    public string UserId { get; set; } = string.Empty;  // maps to auth identity
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<Dog> Dogs { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
