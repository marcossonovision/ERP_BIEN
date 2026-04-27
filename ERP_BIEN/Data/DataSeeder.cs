using Bogus;
using ERP_BIEN.Common.Enums;
using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.EntityFrameworkCore;

public static class DataSeeder
{
    // ===================================================
    // CONFIGURACIÓN RÁPIDA
    // ===================================================
    // true  => borra y recrea todos los datos cada vez que arranca (modo DEV)
    // false => solo inserta lo que falte (modo seguro)
    private const bool RESET_DATABASE = true;

    // Si quieres que tu usuario Windows tenga SUPERADMIN automáticamente:
    // OJO: el seeder guarda DomainUser normalizado como "marcos.gutierrez"
    private const string DEFAULT_SUPERADMIN_DOMAINUSER = "marcos.gutierrez";

    // ===================================================
    // ENTRYPOINT (tu Program.cs lo llama así)
    // ===================================================
    public static void RellenarDatos(IServiceScope scope)
    {
        Console.WriteLine(">>> DATASEEDER EJECUTÁNDOSE <<<");

        try
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Console.WriteLine(
                $"Seeder usando BD: {context.Database.GetDbConnection().DataSource} / {context.Database.GetDbConnection().Database}"
            );

            // Asegura esquema
            context.Database.Migrate();

            // Ejecuta seed
            RellenarDatos(context);

            Console.WriteLine(">>> DATASEEDER TERMINADO <<<");
        }
        catch (Exception ex)
        {
            Console.WriteLine(">>> DATASEEDER ERROR <<<");
            Console.WriteLine(ex.ToString()); // IMPORTANTE: ToString() para ver stacktrace real
        }
    }

    // ===================================================
    // SEED PRINCIPAL
    // ===================================================
    public static void RellenarDatos(AppDbContext context)
    {
        // Transacción => o todo o nada (evita quedarte sin permisos si falla a mitad)
        using var tx = context.Database.BeginTransaction();

        Console.WriteLine("SEED 1/9 - INICIO");

        try
        {
            if (RESET_DATABASE)
            {
                Console.WriteLine("SEED 2/9 - RESET DATABASE (BORRANDO TABLAS)");
                ResetDatabase(context);
                Console.WriteLine("SEED 2/9 - RESET COMPLETADO");
            }

            // Seguridad SIEMPRE primero
            Console.WriteLine("SEED 3/9 - SEED SEGURIDAD (ROLES + PERMISOS)");
            SeedSecurity(context);
            Console.WriteLine("SEED 3/9 - SEGURIDAD OK");

            // Datos funcionales
            Console.WriteLine("SEED 4/9 - TEAMS");
            var teams = SeedTeams(context);
            Console.WriteLine("SEED 4/9 - TEAMS OK");

            Console.WriteLine("SEED 5/9 - USERS (con PersonalInfo + CompanyInfo)");
            var users = SeedUsers(context, teams);
            Console.WriteLine("SEED 5/9 - USERS OK");

            Console.WriteLine("SEED 6/9 - LICENSES");
            SeedLicenses(context, users);
            Console.WriteLine("SEED 6/9 - LICENSES OK");

            Console.WriteLine("SEED 7/9 - DEVICES (COMPUTERS + PHONES)");
            SeedDevices(context, users);
            Console.WriteLine("SEED 7/9 - DEVICES OK");

            Console.WriteLine("SEED 8/9 - USERROLES + ROLEPERMISSIONS");
            SeedUserRolesAndRolePermissions(context, users);
            Console.WriteLine("SEED 8/9 - USERROLES + ROLEPERMISSIONS OK");

            tx.Commit();
            Console.WriteLine("SEED 9/9 - FIN (COMMIT OK)");
        }
        catch
        {
            tx.Rollback();
            Console.WriteLine("SEED - ROLLBACK (se ha revertido todo)");
            throw;
        }
    }

    // ===================================================
    // RESET (BORRADO TOTAL)
    // ===================================================
    private static void ResetDatabase(AppDbContext context)
    {
        // Tablas puente primero
        context.UserRoles.RemoveRange(context.UserRoles);
        context.RolePermissions.RemoveRange(context.RolePermissions);

        // Dependientes de User
        context.Licenses.RemoveRange(context.Licenses);
        context.Software.RemoveRange(context.Software);

        // Dispositivos
        context.Devices.RemoveRange(context.Devices);
        context.Computers.RemoveRange(context.Computers);
        context.Phones.RemoveRange(context.Phones);
        context.Screens.RemoveRange(context.Screens);

        // Principales
        context.Users.RemoveRange(context.Users);

        // 1:1 dependencias User
        context.PersonalInformation.RemoveRange(context.PersonalInformation);
        context.PreceptorDetails.RemoveRange(context.PreceptorDetails);
        context.CompanyInformation.RemoveRange(context.CompanyInformation);

        // Otras principales
        context.Teams.RemoveRange(context.Teams);
        context.Hardware.RemoveRange(context.Hardware);

        // Seguridad
        context.Roles.RemoveRange(context.Roles);
        context.Permissions.RemoveRange(context.Permissions);

        context.SaveChanges();
    }

    // ===================================================
    // SEGURIDAD (ROLES + PERMISOS)
    // ===================================================
    private static void SeedSecurity(AppDbContext context)
    {
        // 1) ROLES (idempotente)
        var rolesToEnsure = new List<Role>
        {
            new Role { Code = "EMP",        Name = "Empleado" },
            new Role { Code = "JEF",        Name = "Jefe" },
            new Role { Code = "RRHH",       Name = "Recursos Humanos" },
            new Role { Code = "ITM",        Name = "IT Management" },
            new Role { Code = "PM",         Name = "Project Manager" },
            new Role { Code = "SUPERADMIN", Name = "SuperAdmin" } // ✅ importante (tu sistema real)
        };

        foreach (var r in rolesToEnsure)
        {
            if (!context.Roles.Any(x => x.Code == r.Code))
                context.Roles.Add(r);
        }
        context.SaveChanges();

        // 2) PERMISSIONS (idempotente)
        var permissionsToEnsure = new List<Permission>
        {
            // General
            new Permission { Code = "WRITE", Description = "Permiso de escritura general" },

            // Users
            new Permission { Code = "USR_VIEW",   Description = "Ver usuarios" },
            new Permission { Code = "USR_CREATE", Description = "Crear usuarios" },
            new Permission { Code = "USR_EDIT",   Description = "Editar usuarios" },
            new Permission { Code = "USR_DELETE", Description = "Eliminar usuarios" },

            // Devices
            new Permission { Code = "DEV_VIEW",   Description = "Ver dispositivos" },
            new Permission { Code = "DEV_ASSIGN", Description = "Asignar dispositivos a usuarios" },
            new Permission { Code = "DEV_EDIT",   Description = "Editar dispositivos" },
            new Permission { Code = "DEV_DELETE", Description = "Eliminar dispositivos" },

            // Licenses
            new Permission { Code = "LIC_VIEW",   Description = "Ver licencias de software" },
            new Permission { Code = "LIC_ASSIGN", Description = "Asignar licencias a usuarios" },
            new Permission { Code = "LIC_EDIT",   Description = "Editar licencias" },
            new Permission { Code = "LIC_DELETE", Description = "Eliminar licencias" },

            // Roles/Permisos
            new Permission { Code = "ROLE_MANAGE",       Description = "Gestionar roles" },
            new Permission { Code = "PERMISSION_MANAGE", Description = "Gestionar permisos" },

            // Reports
            new Permission { Code = "REPORT_VIEW", Description = "Ver informes y listados" }
        };

        foreach (var p in permissionsToEnsure)
        {
            if (!context.Permissions.Any(x => x.Code == p.Code))
                context.Permissions.Add(p);
        }
        context.SaveChanges();

        // 3) SUPERADMIN => TODOS los permisos
        var superAdminRole = context.Roles.Single(r => r.Code == "SUPERADMIN");
        var allPerms = context.Permissions.ToList();

        foreach (var perm in allPerms)
        {
            bool exists = context.RolePermissions.Any(rp =>
                rp.RoleId == superAdminRole.Id && rp.PermissionId == perm.Id);

            if (!exists)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = superAdminRole.Id,
                    PermissionId = perm.Id
                });
            }
        }
        context.SaveChanges();
    }

    // ===================================================
    // TEAMS
    // ===================================================
    private static List<Team> SeedTeams(AppDbContext context)
    {
        if (!RESET_DATABASE && context.Teams.Any())
            return context.Teams.AsNoTracking().ToList();

        var teams = new Faker<Team>()
            .RuleFor(t => t.Name, f => f.Commerce.Department())
            .Generate(3);

        context.Teams.AddRange(teams);
        context.SaveChanges();

        return teams;
    }

    // ===================================================
    // USERS + INFO
    // ===================================================
    private static List<User> SeedUsers(AppDbContext context, List<Team> teams)
    {
        if (!RESET_DATABASE && context.Users.Any())
            return context.Users.AsNoTracking().ToList();

        var personalInfoGen = new Faker<PersonalInformation>("es")
            .RuleFor(p => p.Phone, f => f.Random.Number(600000000, 799999999))
            .RuleFor(p => p.Email, f => f.Internet.Email())
            .RuleFor(p => p.Address, f => f.Address.FullAddress())
            .RuleFor(p => p.DNI, f => f.Random.ReplaceNumbers("#########L"))
            .RuleFor(p => p.Gender, f => f.PickRandom("M", "F"))
            .RuleFor(p => p.Nationality, f => "Española")
            .RuleFor(p => p.DateOfBirth, f => f.Date.Past(40, DateTime.Now.AddYears(-20)))
            .RuleFor(p => p.Studies, f => f.PickRandom("Grado en Informática", "Máster en Sistemas", "FP Desarrollo"))
            .RuleFor(p => p.TypeOfStudies, f => f.PickRandom<TypeOfStudies>())
            .RuleFor(p => p.FamilySituation, f => f.PickRandom<FamilySituation>())
            .RuleFor(p => p.Academy, f => f.Company.CompanyName())
            .RuleFor(p => p.PreceptorDetails, f => new PreceptorDetails
            {
                NIFCouple = f.Random.ReplaceNumbers("#########L"),
                Disability = f.Random.Bool(),
                GeographicMobility = "Local"
            });

        var companyInfoGen = new Faker<CompanyInformation>("es")
            .RuleFor(c => c.spotNumber, f => f.Random.Number(1000, 9999))
            .RuleFor(c => c.technicalCenter, f => f.PickRandom<TechnicalCenter>())
            .RuleFor(c => c.office, f => f.PickRandom<OfficeType>())
            .RuleFor(c => c.Contract, f => f.PickRandom<ContractType>())
            .RuleFor(c => c.ContractCode, f => f.Random.AlphaNumeric(8).ToUpper())
            .RuleFor(c => c.ContractStartDate, f => f.Date.Past(2))
            .RuleFor(c => c.EntryDate, f => f.Date.Past(3))
            .RuleFor(c => c.Seniority, f => f.Random.Double(1, 10))
            .RuleFor(c => c.YearlyGrossSalaryFT, f => (double)f.Finance.Amount(25000, 45000))
            .RuleFor(c => c.YearlyGrossSalaryPT, f => (double)f.Finance.Amount(15000, 24000))
            .RuleFor(c => c.JobTitle, f => f.Name.JobTitle())
            .RuleFor(c => c.SectorOfActivity, f => f.Commerce.Department())
            .RuleFor(c => c.TypeProfile, f => f.PickRandom<TypeOfProfile>())
            .RuleFor(c => c.WorkShifts, f => f.Random.Bool())
            .RuleFor(c => c.DailySchedule, f => f.Random.Bool())
            .RuleFor(c => c.MedicalInsurance, f => f.Random.Bool())
            .RuleFor(c => c.Presence, f => f.PickRandom<Presence>());

        var userGenerator = new Faker<User>("es")
            .RuleFor(u => u.Name, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.DomainUser, (f, u) => $"{u.Name[0]}{u.LastName}".ToLower())
            .RuleFor(u => u.TeamId, f => f.PickRandom(teams).Id)
            .RuleFor(u => u.PersonalInfo, f => personalInfoGen.Generate())
            .RuleFor(u => u.CompanyInfo, f => companyInfoGen.Generate());

        var users = userGenerator.Generate(50);

        // Usuario real normalizado
        users[0].Name = "Marcos";
        users[0].LastName = "Gutierrez";
        users[0].DomainUser = DEFAULT_SUPERADMIN_DOMAINUSER;

        context.Users.AddRange(users);
        context.SaveChanges();

        return users;
    }

    // ===================================================
    // LICENSES
    // ===================================================
    private static void SeedLicenses(AppDbContext context, List<User> users)
    {
        if (!RESET_DATABASE && context.Licenses.Any())
            return;

        var licenseGen = new Faker<License>()
            .RuleFor(l => l.Code, f => f.Random.Guid().ToString())
            .RuleFor(l => l.Producto, f => f.PickRandom(
                "Microsoft Word", "Microsoft Excel", "Microsoft Teams",
                "Adobe Photoshop", "Adobe Acrobat Reader", "Visual Studio Code",
                "Google Chrome", "Mozilla Firefox", "Slack", "Zoom",
                "SAP ERP", "Oracle Database", "AutoCAD", "Jira Software", "Power BI"))
            .RuleFor(l => l.Proveedor, f => f.PickRandom(
                "Microsoft", "Adobe", "Oracle", "SAP", "IBM", "Google", "Amazon Web Services",
                "Salesforce", "VMware", "Atlassian", "Intuit", "Autodesk", "ServiceNow", "Red Hat", "Cisco"))
            .RuleFor(l => l.Price, f => f.PickRandom("150.00", "130.00", "2550.00", "43.00", "87.00", "92.00"))
            .RuleFor(l => l.UserId, f => f.PickRandom(users).Id);

        var licenses = licenseGen.Generate(50);
        context.Licenses.AddRange(licenses);
        context.SaveChanges();
    }

    // ===================================================
    // DEVICES
    // ===================================================
    private static void SeedDevices(AppDbContext context, List<User> users)
    {
        if (!RESET_DATABASE && (context.Computers.Any() || context.Phones.Any()))
            return;

        var compGen = new Faker<Computer>("es")
        .RuleFor(c => c.Comment, f => "") // ✅ AÑADIR ESTA LÍNEA
        .RuleFor(c => c.Hostname, f => f.Internet.DomainWord().ToUpper() + "-PC")
        .RuleFor(c => c.SN, f => f.Random.AlphaNumeric(12).ToUpper())
        .RuleFor(c => c.Model, f => f.Commerce.ProductName())
        .RuleFor(c => c.Status, f => f.PickRandom<StatusDevice>())
        .RuleFor(c => c.NumberOfDevice, f => f.Random.Number(100, 999))
        .RuleFor(c => c.isClient, f => f.Random.Bool())
        .RuleFor(c => c.ComputerType, f => f.PickRandom<ComputerType>())
        .RuleFor(c => c.UserId, f => f.PickRandom(users).Id);

        var phoneGen = new Faker<Phone>("es")
     .RuleFor(p => p.Comment, f => "") // ✅ AÑADIR ESTA LÍNEA
     .RuleFor(p => p.Hostname, f => f.Person.FirstName.ToUpper() + "-TEL")
     .RuleFor(p => p.SN, f => f.Random.AlphaNumeric(12).ToUpper())
     .RuleFor(p => p.Model, f => f.Commerce.ProductName())
     .RuleFor(p => p.Status, f => f.PickRandom<StatusDevice>())
     .RuleFor(p => p.phonenumber, f => f.Random.Number(600000000, 699999999))
     .RuleFor(p => p.SIM, f => f.Random.ReplaceNumbers("8934###########"))
     .RuleFor(p => p.IMEI, f => f.Random.Number(1000000, 9999999))
     .RuleFor(p => p.PIN, f => f.Random.Number(1111, 9999))
     .RuleFor(p => p.PUK, f => f.Random.Number(11111111, 99999999))
     .RuleFor(p => p.UserId, f => f.PickRandom(users).Id);


        context.Computers.AddRange(compGen.Generate(25));
        context.Phones.AddRange(phoneGen.Generate(25));
        context.SaveChanges();
    }

    // ===================================================
    // USERROLES + ROLEPERMISSIONS
    // ===================================================
    private static void SeedUserRolesAndRolePermissions(AppDbContext context, List<User> users)
    {
        var empleadoRole = context.Roles.Single(r => r.Code == "EMP");
        var superAdminRole = context.Roles.Single(r => r.Code == "SUPERADMIN");

        // 1) SUPERADMIN para el user "marcos.gutierrez"
        var adminUser = context.Users.FirstOrDefault(u => u.DomainUser == DEFAULT_SUPERADMIN_DOMAINUSER);
        if (adminUser != null)
        {
            bool hasRole = context.UserRoles.Any(ur => ur.UserId == adminUser.Id && ur.RoleId == superAdminRole.Id);
            if (!hasRole)
            {
                context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = superAdminRole.Id });
            }
        }

        // 2) EMP para el resto si RESET
        if (RESET_DATABASE)
        {
            foreach (var u in users.Skip(1))
            {
                context.UserRoles.Add(new UserRole { UserId = u.Id, RoleId = empleadoRole.Id });
            }
        }

        context.SaveChanges();
    }
}