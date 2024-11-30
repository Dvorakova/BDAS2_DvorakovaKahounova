using BDAS2_DvorakovaKahounova.Models;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace BDAS2_DvorakovaKahounova.DataAcessLayer
{
    public class PesDataAcess
    {
        private string _connectionString;

        public PesDataAcess(string configuration)
        {
            // Načtení connection string z appsettings.json
            _connectionString = configuration;
        }

        //pridano ND pro upravu i rpo prihlasene uživatele
        //metoda pro stránku Psi k adopci
        public List<Pes> GetAllPsi()
        {
            List<Pes> psi = new List<Pes>();

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                // Dotaz na pohled s novými sloupci
                using (var cmd = new OracleCommand("SELECT id_psa, jmeno, datum_narozeni, id_pohlavi, barva, plemeno, vlastnosti, karantena_do, rezervovano, id_fotografie FROM View_PsiVUtulkuBezMajitele", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Pes pes = new Pes
                            {
                                ID_PSA = reader.GetInt32(0),
                                JMENO = reader.IsDBNull(1) ? null : reader.GetString(1),
                                NAROZENI = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                                POHLAVI = reader.GetInt32(3),
                                BARVA = reader.GetString(4),
                                PLEMENO = reader.GetString(5),
                                VLASTNOSTI = reader.IsDBNull(6) ? null : reader.GetString(6),
                                KARANTENA_DO = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7), // Přidání kontroly pro karanténa
                                REZERVOVANO = reader.IsDBNull(8) ? null : reader.GetString(8), // Přidání rezervace
                                ID_FOTOGRAFIE = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9)
                            };
                            psi.Add(pes);
                        }
                    }
                }
            }

            return psi;
        }



        //pridano KK
        public List<Pes> GetPsiProOsobu(int osobaId)
        {
            List<Pes> psi = new List<Pes>();

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("GetPsiProOsobu", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter("osobaId", osobaId));

                    cmd.Parameters.Add(new OracleParameter("cur", OracleDbType.RefCursor, ParameterDirection.Output));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Pes pes = new Pes
                            {
                                ID_PSA = reader.GetInt32(0),
                                JMENO = reader.GetString(1),
                                NAROZENI = reader.GetDateTime(2),
                                POHLAVI = reader.GetInt32(3),
                                PLEMENO = reader.GetString(4),
                                BARVA = reader.GetString(5),
                                VLASTNOSTI = reader.GetString(6),
                                CISLO_CIPU = reader.GetString(7),
                                ID_FOTOGRAFIE = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8) // Ošetření null hodnot
                            };
                            psi.Add(pes);
                        }
                    }
                }
            }

            return psi;
        }

        public Fotografie GetFotografieById(int id)
        {
            Fotografie fotografie = null;

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand(
                    "SELECT id_fotografie, nazev_souboru, typ_souboru, obsah_souboru FROM Fotografie WHERE id_fotografie = :id", con))
                {
                    cmd.Parameters.Add(new OracleParameter("id", id));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fotografie = new Fotografie
                            {
                                id_fotografie = reader.GetInt32(0),
                                nazev_souboru = reader.GetString(1),
                                typ_souboru = reader.GetString(2),
                                obsah_souboru = reader["obsah_souboru"] as byte[]
                            };
                        }
                    }
                }
            }

            return fotografie;
        }

        public void VytvorRezervaci(int idPsa, int rezervatorId)
        {
            string rezervacniKod = CodeGenerator.GenerateRandomCode(); // Vygenerování kódu

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("VlozRezervaci", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure; // Nastavení příkazu jako procedury

                    // Přidání parametrů pro proceduru
                    cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa));
                    cmd.Parameters.Add(new OracleParameter("p_rezervator_id_osoba", rezervatorId));
                    cmd.Parameters.Add(new OracleParameter("p_rezervace_kod", rezervacniKod));

                    cmd.ExecuteNonQuery(); // Spuštění procedury
                }
            }
        }

        public void ZmenTypOsoby(int rezervatorId)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                // Vytvoříme SQL dotaz pro aktualizaci typu osoby
                string sql = @"
            UPDATE osoby 
            SET typ_osoby = 
                CASE 
                    WHEN typ_osoby = 'U' THEN 'R' 
                    WHEN typ_osoby = 'M' THEN 'P'
                END
            WHERE id_osoba = :rezervatorId";

                using (var cmd = new OracleCommand(sql, con))
                {
                    // Přidáme parametr pro ID osoby
                    cmd.Parameters.Add(new OracleParameter("rezervatorId", rezervatorId));

                    // Provedeme UPDATE
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void PridatDoRezervatori(int rezervatorId)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                // Vytvoříme SQL dotaz pro vložení ID osoby do tabulky REZERVATORI
                string sql = @"
            INSERT INTO REZERVATORI (id_osoba)
            VALUES (:rezervatorId)";

                using (var cmd = new OracleCommand(sql, con))
                {
                    // Přidáme parametr pro ID rezervátora
                    cmd.Parameters.Add(new OracleParameter("rezervatorId", rezervatorId));

                    // Provedeme INSERT
                    cmd.ExecuteNonQuery();
                }
            }
        }






        public class CodeGenerator
        {
            private static readonly char[] Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

            public static string GenerateRandomCode(int length = 10)
            {
                var random = new Random();
                return new string(Enumerable.Range(0, length)
                    .Select(_ => Characters[random.Next(Characters.Length)])
                    .ToArray());
            }
        }
    }
}
