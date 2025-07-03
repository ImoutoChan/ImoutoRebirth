using System.Reflection;
using System.Security.Claims;
using HarmonyLib;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Common.AutoMapper;

public static class AutoMapperFix
{
    private static Type? _licenseType;

    public static void Fix(ILogger? logger = null)
    {
        var harmony = new Harmony("app");

        var licenseAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "AutoMapper")!;

        var licenseValidatorType = licenseAssembly.GetType("AutoMapper.Licensing.LicenseValidator");
        var licenseAccessorType = licenseAssembly.GetType("AutoMapper.Licensing.LicenseAccessor");
        _licenseType = licenseAssembly.GetType("AutoMapper.Licensing.License");
        var productTypeEnum = licenseAssembly.GetType("AutoMapper.Licensing.ProductType");
        var editionTypeEnum = licenseAssembly.GetType("AutoMapper.Licensing.Edition");

        if (licenseValidatorType == null
            || licenseAccessorType == null
            || _licenseType == null
            || productTypeEnum == null
            || editionTypeEnum == null)
        {
            return;
        }

        try
        {
            var targetMethodValidate = licenseValidatorType
                .GetMethod("Validate", BindingFlags.Instance | BindingFlags.Public)!;

            harmony.Patch(
                targetMethodValidate,
                new HarmonyMethod(typeof(AutoMapperFix), nameof(PatchLicenseValidatorValidate)));
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Error patching LicenseValidator.Validate");
            throw new InvalidOperationException("Fix was broken", ex);
        }

        try
        {
            var targetMethodInitialize = licenseAccessorType
                .GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic)!;

            harmony.Patch(
                targetMethodInitialize,
                new HarmonyMethod(
                    typeof(AutoMapperFix),
                    nameof(PatchLicenseAccessorInitialize)));
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Error patching LicenseAccessor.Initialize");
            throw new InvalidOperationException("Fix was broken", ex);
        }
    }

    public static bool PatchLicenseValidatorValidate(object license, object ____logger)
    {
        return false;
    }

    public static bool PatchLicenseAccessorInitialize(
        ref object __result,
        object ____logger)
    {

        var claims = new List<Claim>();

        claims.Add(new Claim("account_id", Guid.NewGuid().ToString()));
        claims.Add(new Claim("customer_id", Guid.NewGuid().ToString()));
        claims.Add(new Claim("sub_id", Guid.NewGuid().ToString()));

        var startDateUnix = DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds().ToString();
        claims.Add(new Claim("iat", startDateUnix));

        var expirationDateUnix = DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds().ToString();
        claims.Add(new Claim("exp", expirationDateUnix));

        claims.Add(new Claim("edition", "Enterprise"));
        claims.Add(new Claim("type", "AutoMapper"));

        var claimsIdentity = new ClaimsIdentity(claims.ToArray());
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var fullyValidMockLicense = Activator.CreateInstance(_licenseType!, claimsPrincipal)!;

        __result = fullyValidMockLicense;
        return false;
    }
}
