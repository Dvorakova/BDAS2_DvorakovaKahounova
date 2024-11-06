﻿using BDAS2_DvorakovaKahounova.Models;
using Oracle.ManagedDataAccess.Client;
using System.Security.Cryptography;

namespace BDAS2_DvorakovaKahounova.DataAcessLayer
{
    public class OsobaDataAccess
    {
        private string _connectionString;

        public OsobaDataAccess(string configuration)
        {
            _connectionString = configuration;
        }

        // Metoda pro kontrolu existence emailu
        public bool EmailExists(string email)
        {
            Console.WriteLine("metoda emailExists v osobaDataAccess.");
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("SELECT COUNT(*) FROM OSOBY WHERE EMAIL = :email", con))
                {
                    cmd.Parameters.Add(new OracleParameter("email", email));

                    // Vrať počet záznamů s daným emailem
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0; // Pokud je počet větší než 0, již email existuje
                }
            }
        }
        // Metoda pro registraci uživatele
        public bool RegisterUser(Osoba novaOsoba)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("INSERT INTO OSOBY (JMENO, PRIJMENI, TELEFON, ID_OSOBA, TYP_OSOBY, EMAIL, HESLO, SALT) VALUES (:jmeno, :prijmeni, :telefon, seq_id_osoba.NEXTVAL, :typ_osoby, :email, :heslo, :salt)", con))
                {
                    cmd.Parameters.Add(new OracleParameter("jmeno", novaOsoba.JMENO));
                    cmd.Parameters.Add(new OracleParameter("prijmeni", novaOsoba.PRIJMENI));
                    cmd.Parameters.Add(new OracleParameter("telefon", novaOsoba.TELEFON));
                    cmd.Parameters.Add(new OracleParameter("typ_osoby", novaOsoba.TYP_OSOBY));
                    cmd.Parameters.Add(new OracleParameter("email", novaOsoba.EMAIL));
                    cmd.Parameters.Add(new OracleParameter("heslo", novaOsoba.HESLO));
                    cmd.Parameters.Add(new OracleParameter("salt", novaOsoba.SALT)); //

                    // Vrátí true, pokud byl vložen alespoň jeden řádek
                    return cmd.ExecuteNonQuery() > 0;
                }
                
            }
        }

        // Metoda pro přihlášení uživatele
        public Osoba LoginUser(string email, string heslo)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("SELECT JMENO, PRIJMENI, TELEFON, ID_OSOBA, TYP_OSOBY, EMAIL, HESLO, SALT FROM OSOBY WHERE EMAIL = :email", con))
                {
                    cmd.Parameters.Add(new OracleParameter("email", email));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Načíst hash hesla a salt z databáze
                            string storedHash = reader.GetString(6);
                            string storedSalt = reader.GetString(7);

                            if (VerifyPassword(heslo, storedHash, storedSalt))
                            {
                                return new Osoba
                                {
                                    JMENO = reader.GetString(0),
                                    PRIJMENI = reader.GetString(1),
                                    TELEFON = reader.GetString(2),
                                    ID_OSOBA = reader.GetInt32(3),
                                    TYP_OSOBY = reader.GetString(4),
                                    EMAIL = reader.GetString(5),
                                    HESLO = storedHash,
                                    SALT = storedSalt
                                };
                            }
                        }
                    }
                }
            }
            return null;
        }

        // Metoda pro ověření hesla
        private bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, saltBytes, 10000);
            byte[] hash = pbkdf2.GetBytes(32);

            // Porovnání hashů
            return Convert.ToBase64String(hash) == storedHash;
        }

    }
}
