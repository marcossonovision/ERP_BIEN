using ERP_BIEN.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_BIEN.Models
{
    public class CompanyInformation
    {
        // PK requerida por EF6 (convención)
        public int Id { get; set; }

        public int spotNumber { get; set; }

        // Si estos son enums/catálogos fijos, EF6 los guarda como int
        public TechnicalCenter technicalCenter { get; set; }
        public OfficeType office { get; set; }

        public ContractType Contract { get; set; }

        public string ContractCode { get; set; }

        // En EF6, "Date" se representa como DateTime.
        // Si quieres solo fecha (sin hora) en SQL Server, luego lo mapeamos a "date" con Fluent API.
        [Column(TypeName = "datetime2")]
        public DateTime? ContractStartDate { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime? EndDateTrialPeriod { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime? ContratEndDate { get; set; }

        public double Seniority { get; set; }

        public string SectorOfActivity { get; set; }

        public TypeOfProfile TypeProfile { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? EntryDate { get; set; }

        public bool WorkShifts { get; set; }
        public bool DailySchedule { get; set; }
        public bool AplicableWorkCalendar { get; set; }

        public Presence Presence { get; set; }

        public bool DirectionAttach { get; set; }
        public bool CasqueDor { get; set; }
        public bool FunctionalComplement { get; set; }
        public bool ExpensesTeleworking { get; set; }
        public bool MedicalInsurance { get; set; }
        public bool Car { get; set; }

        public double Bonus { get; set; }
        public double DaosHourlyCosts { get; set; }
        public double PotencialBonus { get; set; }
        public double YearlyGrossSalaryPT { get; set; }
        public double YearlyGrossSalaryFT { get; set; }

        public string JobTitle { get; set; }

        public int Level { get; set; }

        public bool Executive { get; set; }

        public TypeOfProfile NewTypeProfile { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DateNewTypeProfile { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DateConversionUnlimitedContract { get; set; }
    }
}
