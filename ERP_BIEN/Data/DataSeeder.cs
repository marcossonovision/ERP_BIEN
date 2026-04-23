using Bogus;
using ERP_BIEN.Common.Enums; // Asegúrate de que los enums estén accesibles
using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.EntityFrameworkCore;

public static class DataSeeder
{
    public static void SeedData(AppDbContext context)
    {
        // ===================================================
        // 1. LIMPIEZA DE TABLAS (orden seguro para FK)
        // ===================================================

        // Tablas puente primero
        context.UserRoles.RemoveRange(context.UserRoles);
        context.RolePermissions.RemoveRange(context.RolePermissions);

        // Dependientes de User
        context.Licenses.RemoveRange(context.Licenses);
        context.Software.RemoveRange(context.Software);

        // Dispositivos (TPH): si tienes DbSet<Device>, con este basta
        context.Devices.RemoveRange(context.Devices);

        // Si además estás usando DbSet derivados, puedes dejarlo, pero NO es obligatorio:
        context.Computers.RemoveRange(context.Computers);
        context.Phones.RemoveRange(context.Phones);
        context.Screens.RemoveRange(context.Screens);

        // Principales
        context.Users.RemoveRange(context.Users);

        // 1:1 / dependencias que pueden quedarse huérfanas
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

        // ===================================================
        // 2. ROLES (oficiales del ERP)
        // ===================================================
        var roles = new List<Role>
        {
            new Role { Code = "EMP",  Name = "Empleado" },
            new Role { Code = "JEF",  Name = "Jefe" },
            new Role { Code = "RRHH", Name = "Recursos Humanos" },
            new Role { Code = "ITM",  Name = "IT Management" },
            new Role { Code = "PM",   Name = "Project Manager" },
            new Role { Code = "ADM",  Name = "SuperAdmin" }
        };
        context.Roles.AddRange(roles);
        context.SaveChanges();

        var empleadoRole = roles.Single(r => r.Code == "EMP");
        var jefeRole = roles.Single(r => r.Code == "JEF");
        var rrhhRole = roles.Single(r => r.Code == "RRHH");
        var itRole = roles.Single(r => r.Code == "ITM");
        var pmRole = roles.Single(r => r.Code == "PM");
        var adminRole = roles.Single(r => r.Code == "ADM");

        // ===================================================
        // 3. TEAMS
        // ===================================================
        var teams = new Faker<Team>()
            .RuleFor(t => t.Name, f => f.Commerce.Department())
            .Generate(3);

        context.Teams.AddRange(teams);
        context.SaveChanges();

        // ===================================================
        // 4. PERSONAL INFORMATION
        // ===================================================
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

        // ===================================================
        // 5. COMPANY INFORMATION
        // ===================================================
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

        // ===================================================
        // 6. USERS
        // ===================================================
        var userGenerator = new Faker<User>("es")
            .RuleFor(u => u.Name, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.DomainUser, (f, u) => $"{u.Name[0]}{u.LastName}".ToLower())
            .RuleFor(u => u.TeamId, f => f.PickRandom(teams).Id)
            .RuleFor(u => u.PersonalInfo, f => personalInfoGen.Generate())
            .RuleFor(u => u.CompanyInfo, f => companyInfoGen.Generate());

        var users = userGenerator.Generate(50);

        // ✅ USUARIO REAL (NORMALIZADO, SIN DOMINIO)
        users[0].Name = "Marcos";
        users[0].LastName = "Gutierrez";
        users[0].DomainUser = "marcos.gutierrez";

        context.Users.AddRange(users);
        context.SaveChanges();

        // ===================================================
        // 7. LICENSES
        // ===================================================
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

        // ===================================================
        // 8. COMPUTERS + PHONES (ejemplo)
        // ===================================================
        var compGen = new Faker<Computer>("es")
            .RuleFor(c => c.Hostname, f => f.Internet.DomainWord().ToUpper() + "-PC")
            .RuleFor(c => c.SN, f => f.Random.AlphaNumeric(12).ToUpper())
            .RuleFor(c => c.Model, f => f.PickRandom(
                "MacBook Pro M3", "MacBook Air M2", "Dell XPS 13", "HP Spectre x360",
                "Lenovo ThinkPad X1 Carbon", "Asus ZenBook 14", "Microsoft Surface Laptop 6",
                "Acer Swift 5", "Lenovo Legion 5", "MSI Stealth 16 Studio",
                "HP Omen 16", "Asus ROG Zephyrus G14", "Dell Latitude 7440",
                "Samsung Galaxy Book4 Pro", "Razer Blade 15"))
            .RuleFor(c => c.Status, f => f.PickRandom<StatusDevice>())
            .RuleFor(c => c.NumberOfDevice, f => f.Random.Number(100, 999))
            .RuleFor(c => c.isClient, f => f.Random.Bool())
            .RuleFor(c => c.ComputerType, f => f.PickRandom<ComputerType>())
            .RuleFor(c => c.UserId, f => f.PickRandom(users).Id);

        var phoneGen = new Faker<Phone>("es")
            .RuleFor(p => p.Hostname, f => f.Person.FirstName.ToUpper() + "-TEL")
            .RuleFor(p => p.SN, f => f.Random.AlphaNumeric(12).ToUpper())
            .RuleFor(p => p.Model, f => f.PickRandom(
                "iPhone 15 Pro", "Samsung Galaxy S24 Ultra", "Xiaomi 14 Pro", "Google Pixel 8 Pro",
                "OnePlus 12", "Samsung Galaxy Z Flip 5", "iPhone 14", "Xiaomi Redmi Note 13 Pro",
                "Realme GT 5", "Motorola Edge 40", "Huawei P60 Pro", "Sony Xperia 1 V",
                "Nothing Phone (2)", "Oppo Find X6 Pro", "Honor Magic6 Pro"))
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

        // ===================================================
        // 9. PERMISSIONS
        // ===================================================
        var permissions = new List<Permission>
        {
            new Permission { Code = "USR_VIEW",  Description = "Ver usuarios" },
            new Permission { Code = "USR_CREATE",Description = "Crear usuarios" },
            new Permission { Code = "USR_EDIT",  Description = "Editar usuarios" },
            new Permission { Code = "USR_DELETE",Description = "Eliminar usuarios" },

            new Permission { Code = "DEV_VIEW",  Description = "Ver dispositivos" },
            new Permission { Code = "DEV_ASSIGN",Description = "Asignar dispositivos a usuarios" },
            new Permission { Code = "DEV_EDIT",  Description = "Editar dispositivos" },
            new Permission { Code = "DEV_DELETE",Description = "Eliminar dispositivos" },

            new Permission { Code = "LIC_VIEW",  Description = "Ver licencias de software" },
            new Permission { Code = "LIC_ASSIGN",Description = "Asignar licencias a usuarios" },
            new Permission { Code = "LIC_EDIT",  Description = "Editar licencias" },
            new Permission { Code = "LIC_DELETE",Description = "Eliminar licencias" },

            new Permission { Code = "ROLE_MANAGE",       Description = "Gestionar roles" },
            new Permission { Code = "PERMISSION_MANAGE", Description = "Gestionar permisos" },
            new Permission { Code = "REPORT_VIEW",       Description = "Ver informes y listados" }
        };

        context.Permissions.AddRange(permissions);
        context.SaveChanges();

        // ===================================================
        // 10. USERROLES
        // ===================================================
        var userRoles = new List<UserRole>();

        // ADM para los primeros 5
        foreach (var user in users.Take(5))
        {
            userRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = adminRole.Id
            });
        }

        // EMP para el resto
        foreach (var user in users.Skip(5))
        {
            userRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = empleadoRole.Id
            });
        }

        context.UserRoles.AddRange(userRoles);
        context.SaveChanges();

        // ===================================================
        // 11. ROLEPERMISSIONS (ADMIN = TODO)
        // ===================================================
        var rolePermisions = new List<RolePermission>();
        foreach (Permission per in permissions)
        {
            rolePermisions.Add(new RolePermission
            {
                PermissionId = per.Id,
                RoleId = adminRole.Id
            });
        }

        context.RolePermissions.AddRange(rolePermisions);
        context.SaveChanges();
    }

    public static void RellenarDatos(IServiceScope scope)
    {
        Console.WriteLine(">>> DATASEEDER EJECUTÁNDOSE <<<");

        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();

            // 👇👇👇 ESTE ES EL SEGUNDO PASO 👇👇👇
            Console.WriteLine(
                $"Seeder usando BD: " +
                $"{context.Database.GetDbConnection().DataSource} / " +
                $"{context.Database.GetDbConnection().Database}"
            );

            context.Database.Migrate();

            SeedData(context);

            Console.WriteLine(">>> DATASEEDER TERMINADO <<<");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar datos: {ex.Message}");
        }
    }
}
