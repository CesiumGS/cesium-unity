using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Manages PKCE (Proof Key for Code Exchange) parameters for OAuth security
/// </summary>
public class PKCEManager
{
    private const int CODE_VERIFIER_LENGTH = 128;
    private const string CODE_VERIFIER_CHARSET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
    
    /// <summary>
    /// Generates a cryptographically secure code verifier for PKCE
    /// </summary>
    public string GenerateCodeVerifier()
    {
        var bytes = new byte[CODE_VERIFIER_LENGTH];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        
        var result = new StringBuilder(CODE_VERIFIER_LENGTH);
        foreach (byte b in bytes)
        {
            result.Append(CODE_VERIFIER_CHARSET[b % CODE_VERIFIER_CHARSET.Length]);
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// Generates the code challenge from the code verifier using SHA256
    /// </summary>
    public string GenerateCodeChallenge(string codeVerifier)
    {
        if (string.IsNullOrEmpty(codeVerifier))
            throw new ArgumentException("Code verifier cannot be null or empty", nameof(codeVerifier));
        
        using (var sha256 = SHA256.Create())
        {
            byte[] challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Convert.ToBase64String(challengeBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
    
    /// <summary>
    /// Generates a secure random state parameter for CSRF protection
    /// </summary>
    public string GenerateSecureState()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
    
    /// <summary>
    /// Validates that a code verifier meets PKCE requirements
    /// </summary>
    public bool ValidateCodeVerifier(string codeVerifier)
    {
        if (string.IsNullOrEmpty(codeVerifier))
            return false;
        
        if (codeVerifier.Length < 43 || codeVerifier.Length > 128)
            return false;
        
        // Check that all characters are valid for PKCE
        foreach (char c in codeVerifier)
        {
            if (!CODE_VERIFIER_CHARSET.Contains(c.ToString()))
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Verifies that a code challenge was correctly generated from a code verifier
    /// </summary>
    public bool VerifyCodeChallenge(string codeVerifier, string codeChallenge)
    {
        if (string.IsNullOrEmpty(codeVerifier) || string.IsNullOrEmpty(codeChallenge))
            return false;
        
        try
        {
            string expectedChallenge = GenerateCodeChallenge(codeVerifier);
            return expectedChallenge == codeChallenge;
        }
        catch
        {
            return false;
        }
    }
}
