using BDAS2_DvorakovaKahounova.Models;
using Oracle.ManagedDataAccess.Client;

namespace BDAS2_DvorakovaKahounova.DataAcessLayer
{
    public class OsobaDataAccess
    {
        private string _connectionString;

        public OsobaDataAccess(string configuration)
        {
            _connectionString = configuration;
        }

        // Metoda pro registraci uživatele
        public bool RegisterUser(Osoba novaOsoba)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("INSERT INTO OSOBY (JMENO, PRIJMENI, TELEFON, TYP_OSOBY, EMAIL, HESLO) VALUES (:jmeno, :prijmeni, :telefon, :typ_osoby, :email, :heslo)", con))
                {
                    cmd.Parameters.Add(new OracleParameter("jmeno", novaOsoba.JMENO));
                    cmd.Parameters.Add(new OracleParameter("prijmeni", novaOsoba.PRIJMENI));
                    cmd.Parameters.Add(new OracleParameter("telefon", novaOsoba.TELEFON));
                    cmd.Parameters.Add(new OracleParameter("typ_osoby", novaOsoba.TYP_OSOBY));
                    cmd.Parameters.Add(new OracleParameter("email", novaOsoba.EMAIL));
                    cmd.Parameters.Add(new OracleParameter("heslo", novaOsoba.HESLO));

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
                using (var cmd = new OracleCommand("SELECT * FROM OSOBY WHERE EMAIL = :email AND HESLO = :heslo", con))
                {
                    cmd.Parameters.Add(new OracleParameter("email", email));
                    cmd.Parameters.Add(new OracleParameter("heslo", heslo));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Osoba
                            {
                                ID_OSOBA = reader.GetInt32(0),
                                JMENO = reader.GetString(1),
                                PRIJMENI = reader.GetString(2),
                                TELEFON = reader.GetString(3),
                                TYP_OSOBY = reader.GetString(4),
                                EMAIL = reader.GetString(5),
                                HESLO = reader.GetString(6)
                            };
                        }
                    }
                }
            }
            return null;
        }

    }
}
