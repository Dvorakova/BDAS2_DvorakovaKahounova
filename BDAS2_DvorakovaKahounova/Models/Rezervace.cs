namespace BDAS2_DvorakovaKahounova.Models
{
    public class Rezervace
    {
        //pridano KK
        public int IdRezervace { get; set; }
        public int IdPsa { get; set; }
        public DateTime Datum { get; set; }
        public int? IdAdopce { get; set; }
        public int RezervatorIdOsoba { get; set; }
    }
}
