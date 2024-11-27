using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BDAS2_DvorakovaKahounova.Models
{
    public class Osoba
    {
        public int ID_OSOBA { get; set; }

        [Required(ErrorMessage = "Jméno je povinné.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Jméno musí mít mezi 2 a 50 znaky.")]
        [CustomValidation(typeof(Osoba), "ValidateJmenoFormat")]

        public string JMENO { get; set; }

        [Required(ErrorMessage = "Příjmení je povinné.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Jméno musí mít mezi 2 a 50 znaky.")]
        [CustomValidation(typeof(Osoba), "ValidateJmenoFormat")]

        public string PRIJMENI { get; set; }

        [Required(ErrorMessage = "Telefon je povinný.")]
        [StringLength(15, MinimumLength = 8, ErrorMessage = "Telefonní číslo musí mít mezi 8 a 15 znaky.")]
        [RegularExpression(@"^\+?\d{8,15}$", ErrorMessage = "Telefonní číslo může obsahovat pouze čísla a na začátku může být znak '+'.")]
        public string TELEFON { get; set; }

        public string TYP_OSOBY { get; set; } = "";

        [Required(ErrorMessage = "Email je povinný.")]
        [StringLength(50, MinimumLength = 7, ErrorMessage = "Email musí mít mezi 7 a 50 znaky.")]
        [CustomEmailValidation]
        public string EMAIL { get; set; }

        [Required(ErrorMessage = "Heslo je povinné.")]
        [CustomPasswordValidation]

        public string HESLO { get; set; }

        public string SALT { get; set; } = string.Empty;


        public static ValidationResult ValidateJmenoFormat(string value, ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return ValidationResult.Success;
            }

            // Kontrola, že první písmeno je velké
            if (!char.IsUpper(value[0]))
            {
                return new ValidationResult("Údaj musí začínat velkým písmenem.");
            }

            // Kontrola, že obsahuje pouze písmena
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-ZáčďéěíňóřšťúůýžÁČĎÉĚÍŇÓŘŠŤÚŮÝŽ]+$"))
            {
                return new ValidationResult("Údaj může obsahovat pouze písmena.");
            }

            return ValidationResult.Success;
        }

        public class CustomEmailValidationAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                string email = value as string;

                // Kontrola na prázdnou hodnotu
                if (string.IsNullOrEmpty(email))
                {
                    return false; // E-mail nemůže být prázdný
                }

                var parts = email.Split('@');
                if (parts.Length != 2)
                {
                    ErrorMessage = "E-mail musí obsahovat přesně jeden znak '@'.";
                    return false;
                }

                // Kontrola minimálně 3 znaků před '@'
                if (parts[0].Length < 3)
                {
                    ErrorMessage = "Část před '@' musí mít alespoň 3 znaky.";
                    return false;
                }

                var domainParts = parts[1].Split('.');

                // Kontrola, že část po '@' má alespoň 2 znaky před tečkou a alespoň 2 za tečkou
                if (domainParts.Length < 2 || domainParts[0].Length < 2)
                {
                    ErrorMessage = "Část po '@' a před '.' musí mít alespoň 2 znaky.";
                    return false;
                }

                // Kontrola, že po '.' je alespoň 2 znaky
                if (domainParts[domainParts.Length - 1].Length < 2)
                {
                    ErrorMessage = "Po '.' musí být alespoň 2 znaky.";
                    return false;
                }

                // Kontrola, že po '@' a po '.' jsou pouze písmena
                if (!domainParts[0].All(c => Char.IsLetter(c)))
                {
                    ErrorMessage = "Za '@' musí být pouze písmena.";
                    return false;
                }

                if (!domainParts[domainParts.Length - 1].All(c => Char.IsLetter(c)))
                {
                    ErrorMessage = "Za '.' musí být pouze písmena.";
                    return false;
                }

                return true;
            }
        }

        public class CustomPasswordValidationAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                string password = value as string;

                // Kontrola na prázdnou hodnotu
                if (string.IsNullOrEmpty(password))
                {
                    ErrorMessage = "Heslo je povinné.";
                    return false;
                }

                // Kontrola délky hesla
                if (password.Length < 8)
                {
                    ErrorMessage = "Heslo musí mít alespoň 8 znaků.";
                    return false;
                }
                if (password.Length > 25)
                {
                    ErrorMessage = "Heslo nesmí být delší než 25 znaků.";
                    return false;
                }

                // Kontrola, zda heslo obsahuje alespoň jedno velké písmeno
                if (!password.Any(char.IsUpper))
                {
                    ErrorMessage = "Heslo musí obsahovat alespoň jedno velké písmeno.";
                    return false;
                }

                // Kontrola, zda heslo obsahuje alespoň jedno malé písmeno
                if (!password.Any(char.IsLower))
                {
                    ErrorMessage = "Heslo musí obsahovat alespoň jedno malé písmeno.";
                    return false;
                }

                // Kontrola, zda heslo obsahuje alespoň jednu číslici
                if (!password.Any(char.IsDigit))
                {
                    ErrorMessage = "Heslo musí obsahovat alespoň jednu číslici.";
                    return false;
                }

                return true;
            }
        }

       




    }
}
