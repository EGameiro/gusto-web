using System.Security.Claims;

namespace GustoConvenio.Web.Auth;

public class TenantService(IHttpContextAccessor accessor)
{
    public int EmpresaId =>
        int.Parse(accessor.HttpContext!.User.FindFirstValue("EmpresaId") ?? "0");

    public string EmpresaNome =>
        accessor.HttpContext!.User.FindFirstValue("EmpresaNome") ?? "";

    public bool IsAdmin =>
        accessor.HttpContext!.User.FindFirstValue("IsAdmin") == "true";

    public int RestauranteId =>
        int.Parse(accessor.HttpContext!.User.FindFirstValue("RestauranteId") ?? "1");

    public bool IsSuperAdmin =>
        accessor.HttpContext!.User.FindFirstValue("IsSuperAdmin") == "true";
}
