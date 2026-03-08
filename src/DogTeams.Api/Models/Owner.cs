using Newtonsoft.Json;

namespace DogTeams.Api.Models;

public class Owner
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("teamId")]
    public string TeamId { get; set; } = string.Empty;  // partition key
    
    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;  // maps to auth identity
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonProperty("dogs")]
    public List<Dog> Dogs { get; set; } = new();
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
