using Microsoft.AspNetCore.Mvc;
using UltraNet.Core.Interfaces.Caching;
using UltraNet.Core.Interfaces.Logging;
using UltraNet.Core.Interfaces.Otp;
using UltraNet.Core.Interfaces.RateLimiting;

namespace UltraNetDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModuleController : ControllerBase
{
    private readonly ICacheService _cache;
    private readonly ICompositeLogger _logger;
    private readonly IOTPService _otp;
    private readonly IRateLimiter _rateLimiter;

    public ModuleController(ICacheService cache, ICompositeLogger logger, IOTPService otp, IRateLimiter rateLimiter)
    {
        _cache = cache;
        _logger = logger;
        _otp = otp;
        _rateLimiter = rateLimiter;
    }

    [HttpGet("CacheData {id}")]
    public async Task<IActionResult> CacheData(int id)
    {
        var key = $"product:{id}";

        var cached = await _cache.GetAsync<string>(key);
        if (cached != null)
            return Ok($"[From cache] {cached}");

        var result = $"Product #{id} [From database]";
        await _cache.SetAsync(key, result, TimeSpan.FromMinutes(1));

        return Ok(result);
    }

    [HttpGet("GetLog")]
    public IActionResult GetLog()
    {
        _logger.Info("This is an information log!");
        _logger.Warning("This is a Warning log!");
        _logger.Error("error", new Exception("Test exception error!"));
        return Ok("Logs inserted");
    }

    [HttpPost("SendOtp")]
    public async Task<IActionResult> SendOtp(string receiver)
    {
        var code = await _otp.GenerateAndSendOtpAsync(receiver);
        return Ok(new { code });
    }

    [HttpGet("ControlRateLimit")]
    public async Task<IActionResult> ControlRateLimit()
    {
        var key = "test-user";

        if (!await _rateLimiter.IsRequestAllowedAsync(key))
            return StatusCode(429, "Too Many Requests");

        return Ok("Request allowed!");
    }


}