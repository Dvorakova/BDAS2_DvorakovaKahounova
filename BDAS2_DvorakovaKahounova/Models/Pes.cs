namespace BDAS2_DvorakovaKahounova.Models
{
    public class Pes
    {
        public int ID_PSA { get; set; }
        public string JMENO { get; set; } //jméno psa
        public string CISLO_CIPU { get; set; }
        public DateTime? NAROZENI { get; set; }

        public string BARVA { get; set; }
        public string PLEMENO { get; set; }
        public string VLASTNOSTI { get; set; }
        public int POHLAVI { get; set; }

        public string REZERVOVANO { get; set; }
        public DateTime? KARANTENA_DO { get; set; }

        public int? ID_FOTOGRAFIE { get; set; }

        public string DUVOD_POBYTU { get; set; } 
        public decimal KRMNA_DAVKA { get; set; } 
        public decimal VAHA { get; set; } // hmotnost psa
        public string MAJITEL { get; set; } // jméno majitele

        public int? ID_MAJITEL { get; set; } // id majitele

        public int ID_BARVA { get; set; } 
        public int ID_PLEMENO { get; set; }
        public int ID_DUVOD_POBYTU { get; set; }
    }
}
