using System.ComponentModel.DataAnnotations;

namespace BDAS2_DvorakovaKahounova.Models
{
    public class EmailModel
    {
        [Required(ErrorMessage = "Předmět je povinný.")]
        [StringLength(100, ErrorMessage = "Předmět může mít maximálně 100 znaků.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Zpráva je povinná.")]
        [StringLength(1000, ErrorMessage = "Předmět může mít maximálně 1000 znaků.")]
        public string Message { get; set; }
    }
}
