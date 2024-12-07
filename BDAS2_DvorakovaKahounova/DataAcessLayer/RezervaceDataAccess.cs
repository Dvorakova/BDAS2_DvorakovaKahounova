using BDAS2_DvorakovaKahounova.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace BDAS2_DvorakovaKahounova.DataAcessLayer
{
    public class RezervaceDataAccess
    {
        private string _connectionString;

        public RezervaceDataAccess(string configuration)
        {
            _connectionString = configuration;
        }

        //metoda pro zobrazení informací o rezervaci uživatele
        public List<Rezervace> GetRezervace(int rezervatorIdOsoba)
        {
            List<Rezervace> rezervaceList = new List<Rezervace>();

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                using (var cmd = new OracleCommand("VYPISREZERVACE", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Parametr pro proceduru
                    cmd.Parameters.Add(new OracleParameter("p_rezervator_id_osoba", rezervatorIdOsoba));

                    // Parametr pro REF CURSOR (výstup)
                    var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };
                    cmd.Parameters.Add(cursorParam);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var rezervace = new Rezervace
                            {
                                RezervaceDatum = reader.GetDateTime(reader.GetOrdinal("rezervace_datum")),
                                RezervaceKod = reader.GetString(reader.GetOrdinal("rezervace_kod")),
                                Pes = new Pes
                                {
                                    JMENO = reader.GetString(reader.GetOrdinal("pes_jmeno")),
                                    NAROZENI = reader.IsDBNull(reader.GetOrdinal("pes_narozeni")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("pes_narozeni")),
                                    POHLAVI = reader.GetInt32(reader.GetOrdinal("pes_pohlavi")),
                                    BARVA = reader.GetString(reader.GetOrdinal("barva_nazev")),
                                    PLEMENO = reader.GetString(reader.GetOrdinal("plemeno_nazev")),
                                    ID_FOTOGRAFIE = reader.IsDBNull(reader.GetOrdinal("id_fotografie")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("id_fotografie")),
                                    VLASTNOSTI = reader.GetString(reader.GetOrdinal("vlastnosti"))
                                }
                            };

                            rezervaceList.Add(rezervace);
                        }
                    }
                }
            }

            return rezervaceList;
        }

        // metoda rpo získání id psa na základě rezervačního kódu
        public int? ZiskejIdPsa(string rezervaceKod)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                string query = "BEGIN :cursor := ziskej_id_psa(:rezervaceKod); END;";

                using (var cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add(new OracleParameter("cursor", OracleDbType.RefCursor, ParameterDirection.Output));

                    cmd.Parameters.Add(new OracleParameter("rezervaceKod", rezervaceKod));

                    using (var reader = cmd.ExecuteReader())
                    {
                        // Pokud je v REF CURSOR nějaký výsledek (tzn. pes byl nalezen)
                        if (reader.HasRows && reader.Read())
                        {
                            return Convert.ToInt32(reader["id_psa"]); // Vrátíme id_psa
                        }
                        else
                        {
                            // Pokud není řádek v cursoru, vracíme NULL
                            return null;
                        }
                    }
                }
            }
        }

        // metoda pro zobrazení informací o psovi
        public Pes ZobrazInfoOPsovi(int pesId)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("ZobrazInfoOPsovi", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter("pesId", pesId));

                    cmd.Parameters.Add(new OracleParameter("cur", OracleDbType.RefCursor, ParameterDirection.Output));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Pes
                            {
                                ID_PSA = pesId,
                                JMENO = reader.GetString(0),
                                NAROZENI = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1),
                                POHLAVI = reader.GetInt32(2),
                                PLEMENO = reader.GetString(3),
                                BARVA = reader.GetString(4),
                                VLASTNOSTI = reader.GetString(5),
                                CISLO_CIPU = reader.GetString(6),
                                ID_FOTOGRAFIE = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7), // Ošetření null hodnot
                                ID_MAJITEL = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8)
                            };
                        }
                    }
                }
            }

            return null;
        }

        //metoda pro získání id rezervátora
        public int ZiskejIdRezervatora(int idPes)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                using (var cmd = new OracleCommand("SELECT rezervator_id_osoba FROM Rezervace WHERE id_psa = :idPes AND id_adopce IS NULL FETCH FIRST 1 ROWS ONLY", con))
                {
                    // Přidání parametru idPes pro SQL dotaz
                    cmd.Parameters.Add(new OracleParameter("idPes", idPes));

                    // Vykonání dotazu a získání výsledku
                    var result = cmd.ExecuteScalar();

                    // Pokud žádný záznam není nalezen, vrátíme -1
                    if (result == DBNull.Value || result == null)
                    {
                        return -1;
                    }

                    // Vrácení hodnoty jako int
                    return Convert.ToInt32(result);
                }
            }
        }

        //metoda pro adoptování psa na základě rezervace - volá ostatní metody, předává transakci
        public void ZpracujRezervaciVDatabazi(int majitelIdOsoba, int idPes)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                // Začínáme transakci
                var transaction = con.BeginTransaction();

                try
                {
                    Console.WriteLine("0");
                    // Zpracování změny rezervátora (aktualizace typu osoby)
                    ZpracujRezervatorZmena(majitelIdOsoba, idPes, con, transaction);
					Console.WriteLine("1");
					// Přidání majitele do tabulky majitele, pokud už tam není
					PridejMajitele(majitelIdOsoba, con, transaction);
					Console.WriteLine("2");
					// Přidání adopce do tabulky Adopce
					PridejAdopci(idPes, majitelIdOsoba, con, transaction);
					Console.WriteLine("3");
					// Pokud všechny operace proběhnou úspěšně, commitujeme transakci
					transaction.Commit();
                }
                catch (Exception)
                {
                    // Pokud dojde k jakékoliv chybě, transakci rollbackujeme
                    transaction.Rollback();
                    throw;  // Přeposíláme výjimku dál
                }
            }
        }


        //metoda pro adoptování psa
        public void PridejAdopci(int idPsa, int majitelIdOsoba, OracleConnection con, OracleTransaction transaction)
        {

            using (var cmd = new OracleCommand("PRIDEJ_ADOPCI", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Transaction = transaction;
                // Předání vstupních parametrů proceduře
                cmd.Parameters.Add("p_id_psa", OracleDbType.Int32).Value = idPsa;
                cmd.Parameters.Add("p_majitel_id_osoba", OracleDbType.Int32).Value = majitelIdOsoba;

                // Spuštění procedury
                cmd.ExecuteNonQuery();
            }
        }

        public bool ExistujeRezervace(int rezervatorIdOsoba, int idPes, OracleConnection con, OracleTransaction transaction)
        {
            Console.WriteLine("id osoby v Existuje rezervace:" + rezervatorIdOsoba);
            using (var cmd = new OracleCommand("SELECT ExistujeRezervace(:rezervatorIdOsoba, :idPes) FROM dual", con))
            {
				cmd.Transaction = transaction;
				// Přidání parametru pro funkci
				cmd.Parameters.Add(new OracleParameter("rezervatorIdOsoba", rezervatorIdOsoba));
                cmd.Parameters.Add(new OracleParameter("idPes", idPes));
                // Získání výsledku z funkce
                var result = cmd.ExecuteScalar();

                // Pokud je výsledek null, vrátíme false
                if (result == DBNull.Value)
                {
                    Console.WriteLine("Výsledek funkce je NULL");
                    return false;
                }

                // Kontrola, zda výsledek je 1 (TRUE) nebo 0 (FALSE)
                return Convert.ToInt32(result) == 1;

            }
        }




		//metoda pro zpracování, zda se mění typ osoby
		public void ZpracujRezervatorZmena(int majitelIdOsoba, int idPes, OracleConnection con, OracleTransaction transaction)
		{
			// 1. Zkontroluj, zda existuje rezervace pro daného rezervátora a psa
			bool existujeRezervace = ExistujeRezervace(majitelIdOsoba, idPes, con, transaction);

			if (existujeRezervace)
			{
				// 2. Pokud existuje, zavoláme proceduru pro změnu typu osoby na 'P'
				using (var cmd = new OracleCommand("ZMEN_TYP_OSOBY_P", con))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Transaction = transaction;
					cmd.Parameters.Add("p_id_osoba", OracleDbType.Int32).Value = majitelIdOsoba;
					cmd.ExecuteNonQuery();
				}
			}
			else
			{
				// 3. Pokud neexistuje, zavoláme proceduru pro změnu typu osoby na 'M'
				using (var cmd = new OracleCommand("ZMEN_TYP_OSOBY_M", con))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Transaction = transaction;
					cmd.Parameters.Add("p_id_osoba", OracleDbType.Int32).Value = majitelIdOsoba;
					cmd.ExecuteNonQuery();
				}
			}
		}


		//metoda pro přidání majitele do tabulky Majitele, pokud už tam není
		internal void PridejMajitele(int majitelIdOsoba, OracleConnection con, OracleTransaction transaction)
        {
            using (var cmd = new OracleCommand("pridat_majitele", con))
            {
                cmd.CommandType = CommandType.StoredProcedure; // Nastavení typu příkazu na proceduru
                cmd.Transaction = transaction; // Přiřazení transakce

                // Přidání parametru pro ID osoby
                cmd.Parameters.Add(new OracleParameter("p_id_osoba", majitelIdOsoba));

                // Spuštění procedury
                cmd.ExecuteNonQuery();
            }
        }

        //metoda pro určení, zda je pes stále v karanténě
        public string ZjistiKarantenu(int idPes)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                // Použijeme SELECT s voláním funkce z DUAL
                using (var cmd = new OracleCommand("SELECT ZjistiKarantenu(:idPes) FROM dual", con))
                {
                    // Přidáme parametr pro funkci
                    cmd.Parameters.Add(new OracleParameter("idPes", idPes));

                    // Získáme výsledek funkce
                    var result = cmd.ExecuteScalar();

                    // Pokud je výsledek null, vrátíme 'Pes nemá aktivní pobyt'
                    if (result == DBNull.Value)
                    {
                        Console.WriteLine("Výsledek funkce je NULL");
                        return "Pes nemá aktivní pobyt.";
                    }

                    return result.ToString();
                }
            }
        }
    }
}
