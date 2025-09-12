using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace NotesWise.API.Middleware;

public class SupabaseAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SupabaseAuthMiddleware> _logger;
    private readonly string _supabaseUrl;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfigurationManager<OpenIdConnectConfiguration>? _configurationManager;

    public SupabaseAuthMiddleware(RequestDelegate next, ILogger<SupabaseAuthMiddleware> logger, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
        _supabaseUrl = configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase URL not configured");
        
        // Try to configure JWKS endpoint, but don't fail if it doesn't work
        try
        {
            var jwksUri = $"{_supabaseUrl}/auth/v1/jwks";
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                jwksUri,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());
            _logger.LogDebug("JWKS configuration manager initialized for {JwksUri}", jwksUri);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize JWKS configuration manager. Will use fallback validation.");
            _configurationManager = null;
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractBearerToken(context.Request);
        _logger.LogDebug("Token extracted: {HasToken}", !string.IsNullOrEmpty(token));
        
        if (!string.IsNullOrEmpty(token))
        {
            var userId = await ValidateSupabaseTokenAsync(token);
            if (!string.IsNullOrEmpty(userId))
            {
                context.Items["UserId"] = userId;
                
                // Add user claim for authorization
                var claims = new List<Claim>
                {
                    new("sub", userId),
                    new("user_id", userId)
                };
                
                var identity = new ClaimsIdentity(claims, "Supabase");
                context.User = new ClaimsPrincipal(identity);
                _logger.LogDebug("User authenticated successfully: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Token validation failed");
            }
        }
        else
        {
            _logger.LogDebug("No authorization token found in request");
        }

        await _next(context);
    }

    private static string? ExtractBearerToken(HttpRequest request)
    {
        var authorizationHeader = request.Headers.Authorization.FirstOrDefault();
        if (authorizationHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
        {
            return authorizationHeader["Bearer ".Length..].Trim();
        }
        return null;
    }

    private async Task<string?> ValidateSupabaseTokenAsync(string token)
    {
        // Try JWKS validation first
        if (_configurationManager != null)
        {
            var jwksResult = await TryValidateWithJwksAsync(token);
            if (!string.IsNullOrEmpty(jwksResult))
            {
                return jwksResult;
            }
        }

        // Fallback: In development, allow tokens without signature validation (NOT for production)
        if (_environment.IsDevelopment())
        {
            _logger.LogWarning("Using fallback token validation (development only) - signature not verified!");
            return ValidateTokenWithoutSignature(token);
        }

        _logger.LogWarning("Token validation failed and no fallback available");
        return null;
    }

    private async Task<string?> TryValidateWithJwksAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.MapInboundClaims = false;

            var configuration = await _configurationManager!.GetConfigurationAsync(CancellationToken.None);
            _logger.LogDebug("JWKS configuration loaded successfully. Signing keys count: {KeysCount}", configuration.SigningKeys.Count);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = configuration.SigningKeys,
                ValidateIssuer = true,
                ValidIssuer = $"{_supabaseUrl}/auth/v1",
                ValidateAudience = true,
                ValidAudience = "authenticated",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            var userId = ExtractUserIdFromPrincipal(principal, validatedToken);

            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogDebug("Token validated successfully with JWKS for user: {UserId}", userId);
                return userId;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "JWKS validation failed: {Message}", ex.Message);
        }

        return null;
    }

    private string? ValidateTokenWithoutSignature(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Parse token without validation for development
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Check basic token structure and expiration
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                _logger.LogWarning("Token is expired");
                return null;
            }

            // Extract user ID
            var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value ??
                        jwtToken.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value ??
                        jwtToken.Subject;

            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogDebug("Token parsed successfully (no signature verification) for user: {UserId}", userId);
                return userId;
            }

            _logger.LogWarning("Token does not contain a valid user ID");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing JWT token");
            return null;
        }
    }

    private static string? ExtractUserIdFromPrincipal(ClaimsPrincipal principal, SecurityToken validatedToken)
    {
        return principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
               principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               (validatedToken as JwtSecurityToken)?.Subject;
    }
}

// Extension method for easier registration
public static class SupabaseAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseSupabaseAuth(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SupabaseAuthMiddleware>();
    }
}