namespace BDAS2_DvorakovaKahounova.Models
{
    public class Rezervace
    {
        public DateTime RezervaceDatum { get; set; }     // datum rezervace
        public string RezervaceKod { get; set; }         // kód rezervace
        public Pes Pes { get; set; }                     // objekt typu Pes, který obsahuje detaily o psu
    }
}
