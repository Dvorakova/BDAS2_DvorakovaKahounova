namespace BDAS2_DvorakovaKahounova.Models
{
    public class Fotografie
    {
        public int id_fotografie { get; set; } // Identifikátor fotografie
        public string nazev_souboru { get; set; } // Název souboru
        public string typ_souboru { get; set; } // Typ souboru (např. image/jpeg)
        public string pripona_souboru { get; set; } // Přípona souboru (např. .jpg)
        public DateTime datum_nahrani { get; set; } // Datum nahrání
        public DateTime? datum_zmeny { get; set; } // Datum změny (pokud je potřeba)
        public int nahrano_id_osoba { get; set; } // ID osoby, která nahrála obrázek
        public int? zmeneno_id_osoba { get; set; } // ID osoby, která změnila obrázek (pokud je potřeba)
        public byte[] obsah_souboru { get; set; } // Obsah souboru (BLOB)
    }
}
