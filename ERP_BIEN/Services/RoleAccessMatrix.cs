namespace ERP_BIEN.Services
{
    public static class RoleAccessMatrix
    {
        // =========================
        // MÓDULOS
        // =========================
        public const string DASHBOARD = "DASHBOARD";

        // Gestión
        public const string USERS = "USERS";
        public const string ROLES = "ROLES";
        public const string EMPLOYEES = "EMPLOYEES";
        public const string DEVICES = "DEVICES";
        public const string LICENSES = "LICENSES";

        // Mi área
        public const string MY_DEVICES = "MY_DEVICES";
        public const string MY_LICENSES = "MY_LICENSES";
        public const string MY_DATA = "MY_DATA";
        public const string TEAM = "TEAM";

        // =========================
        // MATRIZ ROL → MÓDULOS
        // =========================
        private static readonly Dictionary<string, string[]> _matrix = new()
        {
            // 🔥 SUPERADMIN VE TODO
            ["SUPERADMIN"] = new[]
            {
                DASHBOARD,
                USERS,
                ROLES,
                EMPLOYEES,
                DEVICES,
                LICENSES,
                MY_DEVICES,
                MY_LICENSES,
                MY_DATA,
                TEAM          // aunque no tenga equipo, puede entrar
            },

            // Project Manager
            ["PM"] = new[]
            {
                DASHBOARD,
                EMPLOYEES,
                LICENSES,
                MY_DEVICES,
                MY_LICENSES,
                MY_DATA,
                TEAM          // ✅ solo PM y SuperAdmin
            },

            // Recursos Humanos
            ["RRHH"] = new[]
            {
                DASHBOARD,
                EMPLOYEES,
                USERS,
                MY_DEVICES,
                MY_LICENSES,
                MY_DATA
            },

            // IT Management
            ["ITM"] = new[]
            {
                DASHBOARD,
                DEVICES,
                LICENSES,
                MY_DEVICES,
                MY_LICENSES,
                MY_DATA
            },

            // Empleado
            ["EMP"] = new[]
            {
                DASHBOARD,
                MY_DEVICES,
                MY_LICENSES,
                MY_DATA
            }
        };

        public static IEnumerable<string> GetModulesForRoles(IEnumerable<string> roles)
        {
            return roles
                .Where(r => _matrix.ContainsKey(r))
                .SelectMany(r => _matrix[r])
                .Distinct();
        }
    }
}
