using System.ComponentModel.DataAnnotations;

namespace BDAS2_DvorakovaKahounova.Models
{
    public class Osoba
    {
        public int ID_OSOBA { get; set; }
        [Required(ErrorMessage = "Jméno je povinné.")]
        public string JMENO { get; set; }
        [Required(ErrorMessage = "Příjmení je povinné.")]
        public string PRIJMENI { get; set; }

        [Required(ErrorMessage = "Telefon je povinný.")]
        public string TELEFON { get; set; }
        public string TYP_OSOBY { get; set; } = "";

        [Required(ErrorMessage = "Email je povinný.")]
        [EmailAddress(ErrorMessage = "Zadejte platnou emailovou adresu.")]
        public string EMAIL { get; set; }
        [Required(ErrorMessage = "Heslo je povinné.")]
        [StringLength(100, ErrorMessage = "Heslo musí mít alespoň {2} a maximálně {1} znaků.", MinimumLength = 8)]
        public string HESLO { get; set; }

        public string SALT { get; set; } = string.Empty;

    }
}
