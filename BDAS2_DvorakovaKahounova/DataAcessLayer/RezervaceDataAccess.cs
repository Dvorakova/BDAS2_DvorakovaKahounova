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

        //pro práci s rezervacemi (s databází)

        //pridano KK
        public void VytvorRezervaci(int pesId, int osobaId, DateTime datumRezervace)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                // Zavolání uložené procedury VYPISREZERVACE
                using (var cmd = new OracleCommand("VYPISREZERVACE", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Přidání parametrů pro proceduru
                    cmd.Parameters.Add(new OracleParameter("pPesId", pesId));
                    cmd.Parameters.Add(new OracleParameter("pOsobaId", osobaId));
                    cmd.Parameters.Add(new OracleParameter("pDatumRezervace", datumRezervace));

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public int? ZiskejIdPsa(string rezervaceKod)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                //using (var cmd = new OracleCommand("ziskej_id_psa", con))
                //{
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

                    // Pokud žádný záznam není nalezen, vrátíme -1 nebo jinou hodnotu podle potřeby
                    if (result == DBNull.Value || result == null)
                    {
                        return -1; // Nebo jiná hodnota podle potřeby (např. throw new Exception("Rezervace nenalezena"))
                    }

                    // Vrácení hodnoty jako int
                    return Convert.ToInt32(result);
                }
            }
        }


        //metoda pro vyřízení rezervace (lze použít i pro adopci)
        public void PridejAdopci(int idPsa, int majitelIdOsoba)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                using (var cmd = new OracleCommand("PRIDEJ_ADOPCI", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    Console.WriteLine("id psa v PrideAdopci:" + idPsa);
                    Console.WriteLine("id majitele v PrideAdopci:" + majitelIdOsoba);
                    // Předání vstupních parametrů proceduře
                    cmd.Parameters.Add("p_id_psa", OracleDbType.Int32).Value = idPsa;
                    cmd.Parameters.Add("p_majitel_id_osoba", OracleDbType.Int32).Value = majitelIdOsoba;

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AktualizujKonecPobytu(int idPsa)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                // Vytvoření příkazu pro volání procedury
                using (var cmd = new OracleCommand("AktualizujKonecPobytu", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Přidání parametru pro proceduru
                    cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa));

                    // Spuštění příkazu
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public bool ExistujeRezervace(int rezervatorIdOsoba, int idPes)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                Console.WriteLine("id osoby v Existuje rezervace:" + rezervatorIdOsoba);
                using (var cmd = new OracleCommand("SELECT ExistujeRezervace(:rezervatorIdOsoba, :idPes) FROM dual", con))
                {
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

                    // Výpis výsledku pro debugování
                    Console.WriteLine("result v ExistujeRezervace je: " + result + " (1 znamená TRUE, 0 znamená FALSE)");

                    // Kontrola, zda výsledek je 1 (TRUE) nebo 0 (FALSE)
                    return Convert.ToInt32(result) == 1;
                }
            }
        }




        //metoda rpo zpracování, zda se mění typ osoby
        public void ZpracujRezervatorZmena(int majitelIdOsoba, int idPes)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                // 1. Zkontroluj, zda existuje rezervace pro daného rezervátora a psa
                bool existujeRezervace = ExistujeRezervace(majitelIdOsoba, idPes);

                if (existujeRezervace)
                {
                    // 2. Pokud existuje, změň typ osoby na "P" v tabulce Osoby
                    using (var updateCmd = new OracleCommand("UPDATE Osoby SET typ_osoby = 'P' WHERE id_osoba = :majitelIdOsoba", con))
                    {
                        updateCmd.Parameters.Add(new OracleParameter("majitelIdOsoba", majitelIdOsoba));
                        updateCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Změň typ osoby na "M" v tabulce Osoby
                    using (var updateCmd = new OracleCommand("UPDATE Osoby SET typ_osoby = 'M' WHERE id_osoba = :majitelIdOsoba", con))
                    {
                        updateCmd.Parameters.Add(new OracleParameter("majitelIdOsoba", majitelIdOsoba));
                        updateCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        internal void PridejMajitele(int majitelIdOsoba)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                using (var checkMajitelCmd = new OracleCommand("SELECT COUNT(*) FROM Majitele WHERE id_osoba = :majitelIdOsoba", con))
                {
                    checkMajitelCmd.Parameters.Add(new OracleParameter("majitelIdOsoba", majitelIdOsoba));
                    int count = Convert.ToInt32(checkMajitelCmd.ExecuteScalar());

                    if (count == 0)  // Pokud ještě není v tabulce Majitele
                    {
                        using (var insertCmd = new OracleCommand("INSERT INTO Majitele (id_osoba) VALUES (:majitelIdOsoba)", con))
                        {
                            insertCmd.Parameters.Add(new OracleParameter("majitelIdOsoba", majitelIdOsoba));
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }

            }
        }

        //public string ZjistiKarantenu(int idPes)
        //{
        //    Console.WriteLine("Id psa v Zjisti karantenu: "+idPes);
        //    using (var con = new OracleConnection(_connectionString))
        //    {
        //        con.Open();
        //        using (var cmd = new OracleCommand("BEGIN :result := ZjistiKarantenu(:idPes); END;", con))
        //        {
        //            cmd.Parameters.Add(new OracleParameter("idPes", idPes));
        //            //var resultParam = new OracleParameter("result", OracleDbType.Varchar2, 4000);
        //            //resultParam.Direction = ParameterDirection.Output;
        //            var resultParam = new OracleParameter("result", OracleDbType.Clob);
        //            resultParam.Direction = ParameterDirection.Output;
        //            cmd.Parameters.Add(resultParam);

        //            cmd.ExecuteNonQuery();
        //            Console.WriteLine($"Výstup funkce: {resultParam.Value}");
        //            return resultParam.Value.ToString();
        //        }
        //    }
        //}

        public string ZjistiKarantenu(int idPes)
        {
            Console.WriteLine("Id psa v Zjisti karantenu: " + idPes);
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

                    // Výpis výsledku pro debugování
                    Console.WriteLine("Výsledek funkce ZjistiKarantenu: " + result);

                    return result.ToString();
                }
            }
        }




    }
}
