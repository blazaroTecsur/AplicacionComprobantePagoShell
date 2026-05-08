using Shell.Web.Services;

namespace Shell.Web.Extensions
{
    public static class PermisosExtensions
    {
        private const string ItemsKey = "Shell_Permisos";

        public static void SetPermisos(this HttpContext ctx, IReadOnlyCollection<string> permisos)
            => ctx.Items[ItemsKey] = permisos;

        public static IReadOnlyCollection<string> GetPermisos(this HttpContext ctx)
            => ctx.Items.TryGetValue(ItemsKey, out var v) && v is IReadOnlyCollection<string> p
                ? p
                : [];

        public static bool TienePermiso(this HttpContext ctx, string codigo)
            => ctx.GetPermisos().Contains(codigo, StringComparer.OrdinalIgnoreCase);
    }
}
