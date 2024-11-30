using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace BDAS2_DvorakovaKahounova.DataAcessLayer
{
    public class ChovateleDataAccess
    {
        private string _connectionString;

        public ChovateleDataAccess(string configuration)
        {
            // Načtení connection string z appsettings.json
            _connectionString = configuration;
        }

        public List<Pes> GetAllPsiProChovatele()
        {
            List<Pes> psi = new List<Pes>();

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                using (var cmd = new OracleCommand("SELECT id_psa, jmenoPsa, cislo_cipu, datumNarozeni, Pohlavi, Barva, Plemeno, Vlastnosti, KarantenaDo, DuvodPobytu, KrmnaDavka, Vaha, Majitel, Rezervace, Fotografie, IdMajitel FROM ViewVypisVsechPsu", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
							Pes pes = new Pes
							{
								ID_PSA = reader.GetInt32(0),
								JMENO = reader.IsDBNull(1) ? null : reader.GetString(1),
								CISLO_CIPU = reader.IsDBNull(2) ? null : reader.GetString(2),
								NAROZENI = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
								POHLAVI = reader.GetInt32(4),
								BARVA = reader.GetString(5),
								PLEMENO = reader.GetString(6),
								VLASTNOSTI = reader.IsDBNull(7) ? null : reader.GetString(7),
								KARANTENA_DO = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
								DUVOD_POBYTU = reader.IsDBNull(9) ? null : reader.GetString(9),
								KRMNA_DAVKA = reader.GetDecimal(10),
								VAHA = reader.GetDecimal(11),
								MAJITEL = reader.IsDBNull(12) ? null : reader.GetString(12),
								REZERVOVANO = reader.IsDBNull(13) ? null : reader.GetString(13),
								ID_FOTOGRAFIE = reader.IsDBNull(14) ? (int?)null : reader.GetInt32(14),
								ID_MAJITEL = reader.IsDBNull(15) ? (int?)null : reader.GetInt32(15)
								
							};

							psi.Add(pes);
                        }
                    }
                }
            }

            return psi;
        }

		//fotografie
		public int SaveFotografie(Fotografie fotografie)
		{
			int newId = 0;

			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var cmd = new OracleCommand(
					@"INSERT INTO Fotografie (nazev_souboru, typ_souboru, pripona_souboru, datum_nahrani, nahrano_id_osoba, obsah_souboru) 
              VALUES (:nazev, :typ, :pripona, :datum, :osobaId, :obsah)
              RETURNING id_fotografie INTO :newId", con))
				{
					cmd.Parameters.Add(new OracleParameter("nazev", fotografie.nazev_souboru));
					cmd.Parameters.Add(new OracleParameter("typ", fotografie.typ_souboru));
					cmd.Parameters.Add(new OracleParameter("pripona", fotografie.pripona_souboru));
					cmd.Parameters.Add(new OracleParameter("datum", fotografie.datum_nahrani));
					cmd.Parameters.Add(new OracleParameter("osobaId", fotografie.nahrano_id_osoba));
					cmd.Parameters.Add(new OracleParameter("obsah", fotografie.obsah_souboru));
					cmd.Parameters.Add(new OracleParameter("newId", OracleDbType.Int32, ParameterDirection.Output));

					cmd.ExecuteNonQuery();
					newId = Convert.ToInt32(cmd.Parameters["newId"].Value.ToString());
				}
			}

			return newId;
		}


		public void UpdatePesFotografie(int pesId, int fotografieId)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var cmd = new OracleCommand(
					"UPDATE Psi SET ID_FOTOGRAFIE = :fotografieId WHERE ID_PSA = :pesId", con))
				{
					cmd.Parameters.Add(new OracleParameter("fotografieId", fotografieId));
					cmd.Parameters.Add(new OracleParameter("pesId", pesId));
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void UpdateFotografie(Fotografie fotografie)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var cmd = new OracleCommand(
					"UPDATE Fotografie SET nazev_souboru = :nazev, typ_souboru = :typ, pripona_souboru = :pripona, obsah_souboru = :obsah, datum_zmeny = :datum, zmeneno_id_osoba = :zmeneno_id_osoba WHERE id_fotografie = :id", con))
				{
					cmd.Parameters.Add(new OracleParameter("nazev", fotografie.nazev_souboru));
					cmd.Parameters.Add(new OracleParameter("typ", fotografie.typ_souboru));
					cmd.Parameters.Add(new OracleParameter("pripona", fotografie.pripona_souboru));
					cmd.Parameters.Add(new OracleParameter("obsah", fotografie.obsah_souboru));
					cmd.Parameters.Add(new OracleParameter("datum", DateTime.Now)); // datum_zmeny
					cmd.Parameters.Add(new OracleParameter("zmeneno_id_osoba",fotografie.nahrano_id_osoba)); // zmeneno_id_osoba
					cmd.Parameters.Add(new OracleParameter("id", fotografie.id_fotografie));

					cmd.ExecuteNonQuery();
				}
			}
		}


		public Fotografie GetFotografieByPesId(int pesId)
		{
			Fotografie fotografie = null;

			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var cmd = new OracleCommand(
					"SELECT id_fotografie, nazev_souboru, typ_souboru, obsah_souboru FROM Fotografie WHERE id_fotografie = (SELECT id_fotografie FROM Psi WHERE id_psa = :pesId)", con))
				{
					cmd.Parameters.Add(new OracleParameter("pesId", pesId));

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


		public void PridatOdcerveni(int pesId, DateTime datumOdcerveni)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var cmd = new OracleCommand("PRIDAT_ODCERVENI", con))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Parametry procedury
					cmd.Parameters.Add(new OracleParameter("v_id_psa", OracleDbType.Int32)).Value = pesId;
					cmd.Parameters.Add(new OracleParameter("v_datum", OracleDbType.Date)).Value = datumOdcerveni;

					try
					{
						// Spuštění procedury
						cmd.ExecuteNonQuery();
					}
					catch (OracleException ex)
					{
						// Zpracování chyb
						if (ex.Number == 20001)
						{
							throw new Exception("Nebyl nalezen žádný neukončený pobyt pro tohoto psa.");
						}
						else
						{
							throw new Exception($"Nastala neočekávaná chyba: {ex.Message}");
						}
					}
				}
			}
		}

		public void PridatOckovani(int pesId, DateTime datumOckovani)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var cmd = new OracleCommand("PRIDAT_OCKOVANI", con))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Parametry procedury
					cmd.Parameters.Add(new OracleParameter("v_id_psa", OracleDbType.Int32)).Value = pesId;
					cmd.Parameters.Add(new OracleParameter("v_datum", OracleDbType.Date)).Value = datumOckovani;

					try
					{
						// Spuštění procedury
						cmd.ExecuteNonQuery();
					}
					catch (OracleException ex)
					{
						// Zpracování chyb
						if (ex.Number == 20001)
						{
							throw new Exception("Nebyl nalezen žádný neukončený pobyt pro tohoto psa.");
						}
						else
						{
							throw new Exception($"Nastala neočekávaná chyba: {ex.Message}");
						}
					}
				}
			}
		}

        //Metody pro Pridat psa:

        public int? GetPesIdByCisloCipu(string cisloCipu)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();

                    // PL/SQL blok pro volání funkce s REF CURSOR
                    string query = "BEGIN :cursor := GETPESIDBYCISLOCIPU(:cisloCipu); END;";

                    using (var command = new OracleCommand(query, connection))
                    {
                        // Přidání parametru pro výstupní SYS_REFCURSOR
                        command.Parameters.Add(new OracleParameter("cursor", OracleDbType.RefCursor, ParameterDirection.Output));
                        // Parametr pro číslo čipu
                        command.Parameters.Add(new OracleParameter("cisloCipu", cisloCipu));

                        using (var reader = command.ExecuteReader())
                        {
                            // Pokud je v REF CURSOR nějaký výsledek (tzn. pes byl nalezen)
                            if (reader.HasRows && reader.Read())
                            {
                                return Convert.ToInt32(reader["id_psa"]); // Vrátíme id_psa
                            }
                            else
                            {
                                // Pokud není řádek v cursoru, vracíme NULL
                                Console.WriteLine("Pes nenalezen.");
                                return null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při volání funkce: {ex.Message}");
                throw;
            }
        }

        //meotdy pro načítání do comboboxů:
        public List<Barva> GetBarvy()
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                var command = new OracleCommand("SELECT id_barva, nazev FROM barvy", connection);
                var reader = command.ExecuteReader();
                var barvy = new List<Barva>();

                while (reader.Read())
                {
                    barvy.Add(new Barva
                    {
                        Id = Convert.ToInt32(reader["id_barva"]),
                        Nazev = reader["nazev"].ToString()
                    });
                }
                Console.WriteLine($"Počet načtených barev: {barvy.Count}");
                return barvy;
            }
        }

        public List<Plemeno> GetPlemene()
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                var command = new OracleCommand("SELECT id_plemeno, nazev FROM plemena", connection);
                var reader = command.ExecuteReader();
                var plemena = new List<Plemeno>();

                while (reader.Read())
                {
                    plemena.Add(new Plemeno
                    {
                        Id = Convert.ToInt32(reader["id_plemeno"]),
                        Nazev = reader["nazev"].ToString()
                    });
                }

                return plemena;
            }
        }

        public List<DuvodPobytu> GetDuvodyPobytu()
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                var command = new OracleCommand("SELECT id_duvod, duvod FROM duvody_pobytu", connection);
                var reader = command.ExecuteReader();
                var duvody = new List<DuvodPobytu>();

                while (reader.Read())
                {
                    duvody.Add(new DuvodPobytu
                    {
                        Id = Convert.ToInt32(reader["id_duvod"]),
                        Nazev = reader["duvod"].ToString()
                    });
                }

                return duvody;
            }
        }

        public List<Vlastnost> GetVlastnosti()
        {
            // Předpokládám, že máte třídu Vlastnost s ID a názvem.
            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                var command = new OracleCommand("SELECT id_vlastnost, nazev FROM Vlastnosti", connection);
                var reader = command.ExecuteReader();
                var vlastnosti = new List<Vlastnost>();
                while (reader.Read())
                {
                    vlastnosti.Add(new Vlastnost
                    {
                        Id = Convert.ToInt32(reader["id_vlastnost"]),
                        Nazev = reader["nazev"].ToString()
                    });
                }
                return vlastnosti;
            }
        }

        //metoda pro vypsání informací o nalezeném psu:

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
                                NAROZENI = reader.GetDateTime(1),
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

        //metoda pro zobrazení informací majitele psa
        public Osoba GetUserProfileById(int? majitelId)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("SELECT JMENO, PRIJMENI, TELEFON, EMAIL FROM OSOBY WHERE ID_OSOBA = :majitelId", con))
                {
                    cmd.Parameters.Add(new OracleParameter("majitelId", majitelId));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Osoba
                            {
                                JMENO = reader.GetString(0),
                                PRIJMENI = reader.GetString(1),
                                TELEFON = reader.GetString(2),
                                EMAIL = reader.GetString(3)
                            };
                        }
                    }
                }
            }
            return null; // Pokud uživatel nebyl nalezen
        }

        //Samotná metoda přidání nového psa do tabulky psi
        public int VlozPsa(string jmeno, string cisloCipu, DateTime? datumNarozeni, int plemenoId, int barvaId, int pohlaviId, int? fotografieId, int vaha)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                // SQL dotaz pro vložení nového psa
                var sql = @"
            INSERT INTO Psi (JMENO, CISLO_CIPU, NAROZENI, ID_PLEMENO, ID_BARVA, ID_FOTOGRAFIE, ID_POHLAVI, VAHA)
            VALUES (:jmeno, :cisloCipu, :datumNarozeni, :plemenoId, :barvaId, :fotografieId, :pohlaviId, :vaha)
            RETURNING ID_PSA INTO :newId";

                using (var cmd = new OracleCommand(sql, con))
                {
                    // Přidání parametrů
                    cmd.Parameters.Add(new OracleParameter("jmeno", string.IsNullOrEmpty(jmeno) ? DBNull.Value : jmeno));
                    cmd.Parameters.Add(new OracleParameter("cisloCipu", string.IsNullOrEmpty(cisloCipu) ? DBNull.Value : cisloCipu));
                    cmd.Parameters.Add(new OracleParameter("datumNarozeni", datumNarozeni.HasValue ? (object)datumNarozeni.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("plemenoId", plemenoId));
                    cmd.Parameters.Add(new OracleParameter("barvaId", barvaId));
                    cmd.Parameters.Add(new OracleParameter("fotografieId", fotografieId.HasValue ? (object)fotografieId.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("pohlaviId", pohlaviId));
                    cmd.Parameters.Add(new OracleParameter("vaha", vaha));


                    // Výstupní parametr pro získání ID nově vloženého záznamu
                    var newIdParam = new OracleParameter("newId", OracleDbType.Int32)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(newIdParam);

                    // Spuštění příkazu
                    cmd.ExecuteNonQuery();

                    // Vrácení nově vygenerovaného ID
                    return Convert.ToInt32(newIdParam.Value.ToString());
                }
            }
        }


        //vlozeni pobytu (voláno po vložení psa)
        public int VlozPobyt(int idPes, DateTime zacatekPobytu, int idDuvod, double vaha)
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    using (var command = new OracleCommand("VlozPobyt", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Parametry pro proceduru
                        command.Parameters.Add("p_id_psa", OracleDbType.Int32).Value = idPes;
                        command.Parameters.Add("p_zacatek_pobytu", OracleDbType.Date).Value = zacatekPobytu;
                        command.Parameters.Add("p_id_duvod", OracleDbType.Int32).Value = idDuvod;
                        command.Parameters.Add("p_vaha", OracleDbType.Double).Value = vaha;

                        // Výstupní parametr
                        var outputParam = new OracleParameter("p_id_pobyt", OracleDbType.Int32)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputParam);



                        // Spuštění procedury
                        command.ExecuteNonQuery();

                        // Návrat ID nově vytvořeného pobytu
                        return Convert.ToInt32(outputParam.Value.ToString());
                        //return Convert.ToInt32(outputParam.Value);
                    }
                }
                catch (Exception ex)
                {
                    // Logování chyby
                    Console.WriteLine($"Chyba při volání procedury VlozPobyt: {ex.Message}");
                    throw;
                }
            }
        }

        //poté voláno
        public void VlozZaznamOPobytu(int pobytId)
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    using (var command = new OracleCommand("VlozZaznamOPobytu", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Parametr procedury
                        command.Parameters.Add("p_id_pobyt", OracleDbType.Int32).Value = pobytId;

                        // Spuštění procedury
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // Logování chyby
                    Console.WriteLine($"Chyba při volání procedury VlozZaznamOPobytu: {ex.Message}");
                    throw;
                }
            }
        }

        //metoda pro přidání vlastností do databáze pro nového psa
        public void PridatVlastnostiDoDatabaze(int pesId, int[] vlastnostiIds)
        {
            // Pokud není žádná vlastnost zaškrtnutá, nic neprovádíme
            if (vlastnostiIds == null || vlastnostiIds.Length == 0)
                return;

            // Otevření připojení k Oracle databázi
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                // Pro každý ID vlastnosti zaškrtnuté v checkboxech
                foreach (var vlastnostId in vlastnostiIds)
                {
                    // Vytvoření SQL dotazu pro vložení do tabulky psi_vlastnosti
                    string query = "INSERT INTO psi_vlastnosti (id_psa, id_vlastnost) VALUES (:PesId, :VlastnostId)";

                    using (var cmd = new OracleCommand(query, con))
                    {
                        // Přidání parametrů do dotazu
                        cmd.Parameters.Add(new OracleParameter(":PesId", pesId));
                        cmd.Parameters.Add(new OracleParameter(":VlastnostId", vlastnostId));

                        // Provedení dotazu
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }



    }
}
