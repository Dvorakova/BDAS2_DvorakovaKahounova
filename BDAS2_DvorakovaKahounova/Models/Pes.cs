namespace BDAS2_DvorakovaKahounova.Models
{
    public class Pes
    {
        public int ID_PSA { get; set; }
        public string JMENO { get; set; }
        public string CISLO_CIPU { get; set; }
        public DateTime? NAROZENI { get; set; }

        //pridano ND - předělávka Psi k adopci pro nepřihlášeného uživatele
        public string BARVA { get; set; }
        public string PLEMENO { get; set; }
        public string VLASTNOSTI { get; set; }
        public int POHLAVI { get; set; }

        // Přidané vlastnosti pro přihlášené uživatele
        public string REZERVOVANO { get; set; }
        public DateTime? KARANTENA_DO { get; set; }

        // Přidání pro fotografii
        public int? ID_FOTOGRAFIE { get; set; }

        //public int ID_OSOBA { get; set; }  // pridano KK

        //přidáno pro chovatele
        public string DUVOD_POBYTU { get; set; } // Důvod pobytu
        public decimal KRMNA_DAVKA { get; set; }  // Krmná dávka
        public decimal VAHA { get; set; }        // Váha psa
        public string MAJITEL { get; set; }

        public int? ID_MAJITEL { get; set; }

        //přidáno pro PřidatPsa
        public int ID_BARVA { get; set; }  // ID barvy
        public int ID_PLEMENO { get; set; }  // ID plemene
        public int ID_DUVOD_POBYTU { get; set; }  // ID důvodu pobytu
    }
}
