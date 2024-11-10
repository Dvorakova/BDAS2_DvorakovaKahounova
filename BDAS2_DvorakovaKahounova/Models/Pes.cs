﻿namespace BDAS2_DvorakovaKahounova.Models
{
    public class Pes
    {
        public int ID_PSA { get; set; }
        public string JMENO { get; set; }
        public string CISLO_CIPU { get; set; }
        public DateTime NAROZENI { get; set; }

        //pridano ND - předělávka Psi k adopci pro nepřihlášeného uživatele
        public string BARVA { get; set; }
        public string PLEMENO { get; set; }
        public string VLASTNOSTI { get; set; }

        //public int ID_OSOBA { get; set; }  // pridano KK
    }
}
