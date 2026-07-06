//-----------------------------------------------------------------------
// <copyright file="RatingUserKeyResolver.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Resolves a stable, opaque user key suitable for identifying the rater of a joke.
// Authenticated users receive their identity claim value.
// Anonymous users receive a deterministic prefix + SHA-256 hash derived from the
// client IP address and an application-level salt — never the raw IP itself.
// </summary>
//-----------------------------------------------------------------------
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace DadABase.Web.Services;

/// <summary>
/// Resolves a stable, opaque key that identifies a joke rater.
/// </summary>
/// <remarks>
/// <para>For authenticated users the key is the identity name from the claims principal.</para>
/// <para>
/// For anonymous users the key takes the form <c>ANON_IP_&lt;hex&gt;</c> where
/// <c>&lt;hex&gt;</c> is the lower-case SHA-256 of (normalised IP + salt).
/// The raw IP address is never stored, satisfying basic PII hygiene.
/// </para>
/// <para>
/// Different anonymous callers from different IPs produce different keys, so the
/// unique-per-joke constraint in the database allows them to rate independently.
/// A shared NAT address will be treated as a single rater — an acceptable limitation
/// for a non-critical feature.
/// </para>
/// </remarks>
public class RatingUserKeyResolver
{
    // Salt injected via appsettings to make the hash non-reversible.
    // Defaults to a fixed value so the service works without explicit configuration.
    private readonly string _salt;

    /// <summary>
    /// Initialises the resolver.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public RatingUserKeyResolver(IConfiguration configuration)
    {
        _salt = configuration["RatingUserKeySalt"] ?? "dadabase-rating-default-salt-2026";
    }

    /// <summary>
    /// Returns the rating user key for the given HTTP context.
    /// </summary>
    /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
    /// <returns>
    /// Identity claim value for authenticated users; "ANON_IP_&lt;hash&gt;" for anonymous.
    /// </returns>
    public string Resolve(HttpContext httpContext)
    {
        if (httpContext == null)
        {
            return "ANON_UNKNOWN";
        }

        // --- Authenticated user ---
        var user = httpContext.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var name = user.Identity.Name;
            if (!string.IsNullOrWhiteSpace(name))
            {
                // Strip AAD domain prefix (#) as BaseAPIController does
                var hash = name.IndexOf('#');
                return hash >= 0 ? name[(hash + 1)..] : name;
            }
        }

        // --- Anonymous user: hash the client IP ---
        var ip = ResolveClientIp(httpContext);
        return "ANON_IP_" + HashIp(ip, _salt);
    }

    // Resolves the client IP, honouring X-Forwarded-For when the connection
    // comes from a loopback (trusted reverse proxy on the same host).
    private static string ResolveClientIp(HttpContext httpContext)
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress;

        // Only trust X-Forwarded-For when the direct connection is loopback
        // (i.e. a local reverse proxy), to guard against header spoofing.
        if (remoteIp != null && IPAddress.IsLoopback(remoteIp))
        {
            var forwarded = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
            {
                // X-Forwarded-For may be a comma-separated list; take the first entry.
                var firstIp = forwarded.Split(',')[0].Trim();
                if (IPAddress.TryParse(firstIp, out var parsed))
                {
                    return NormaliseIp(parsed);
                }
            }
        }

        return remoteIp != null ? NormaliseIp(remoteIp) : "unknown";
    }

    // Map IPv4-mapped IPv6 addresses (::ffff:x.x.x.x) to their IPv4 form.
    private static string NormaliseIp(IPAddress ip)
    {
        if (ip.IsIPv4MappedToIPv6)
        {
            return ip.MapToIPv4().ToString();
        }

        return ip.ToString();
    }

    // SHA-256(ip + salt) → lower-case hex string (64 chars, no PII).
    private static string HashIp(string ip, string salt)
    {
        var input = Encoding.UTF8.GetBytes(ip + salt);
        var hashBytes = SHA256.HashData(input);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
