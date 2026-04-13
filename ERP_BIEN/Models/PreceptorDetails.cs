namespace ERP_BIEN.Models
{
    public class PreceptorDetails
    {
        // Clave primaria recomendada para EF
        public int Id { get; set; }

        // NIFCouple : string
        public string NIFCouple { get; set; }

        // Disability : bool
        public bool Disability { get; set; }

        // GeographicMobility : string
        public string GeographicMobility { get; set; }

        // Constructor opcional para evitar nulls en strings
        public PreceptorDetails()
        {
            NIFCouple = string.Empty;
            GeographicMobility = string.Empty;
        }
    }
}