using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var token = GetTokenUsingClientCredentialsFlow();

Console.WriteLine(token?.AccessToken);
Console.WriteLine(token?.AccessTokenExpireTime);


TokenResponse? GetTokenUsingClientCredentialsFlow()
{
    string clientId = "";
    string clientSecret = "";
    string tokenEndpoint = "";
    string? scope = null;
    string? audience = null;

    var client = new HttpClient();

    var body = new StringBuilder();
    body.Append("grant_type=client_credentials");
    body.Append("&client_id=").Append(clientId);
    body.Append("&client_secret=").Append(clientSecret);
    if (!string.IsNullOrWhiteSpace(scope))
    {
        body.Append("&scope=").Append(scope);
    }
    if (!string.IsNullOrWhiteSpace(audience))
    {
        body.Append("&audience=").Append(audience);
    }

    var content = new StringContent(body.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");
    var response = client.PostAsync(tokenEndpoint, content).GetAwaiter().GetResult();

    if (response.IsSuccessStatusCode)
    {
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var token = JsonSerializer.Deserialize<TokenResponse>(json);
        return token;
    }
    else
    {
        var error = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        Console.WriteLine($"{response.StatusCode}: {error}");
    }

    return null;
}

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public long AccessTokenExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("refresh_expires_in")]
    public long? RefreshTokenExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "";

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = "";

    private DateTimeOffset _createTime = DateTimeOffset.UtcNow;

    public DateTimeOffset AccessTokenExpireTime
    {
        // Give one minute to perform request before token actually expires
        get { return _createTime.AddSeconds(AccessTokenExpiresIn).AddMinutes(-1); }
    }

    public DateTimeOffset? RefreshTokenExpireTime
    {
        // Give one minute to perform request before token actually expires
        get { return RefreshTokenExpiresIn != null ? _createTime.AddSeconds(RefreshTokenExpiresIn.Value).AddMinutes(-1) : null; }
    }

    public bool IsAccessTokenExpired { get { return DateTimeOffset.UtcNow > AccessTokenExpireTime; } }

    public bool IsRefreshTokenExpired { get { return RefreshTokenExpireTime != null ? DateTimeOffset.UtcNow > RefreshTokenExpireTime : true; } }
}