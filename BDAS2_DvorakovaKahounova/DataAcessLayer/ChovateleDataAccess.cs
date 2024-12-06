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
                using (var cmd = new OracleCommand("save_fotografie", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů
                    cmd.Parameters.Add(new OracleParameter("p_nazev", fotografie.nazev_souboru));
                    cmd.Parameters.Add(new OracleParameter("p_typ", fotografie.typ_souboru));
                    cmd.Parameters.Add(new OracleParameter("p_pripona", fotografie.pripona_souboru));
                    cmd.Parameters.Add(new OracleParameter("p_datum", fotografie.datum_nahrani));
                    cmd.Parameters.Add(new OracleParameter("p_osoba_id", fotografie.nahrano_id_osoba));
                    cmd.Parameters.Add(new OracleParameter("p_obsah", OracleDbType.Blob)
                    {
                        Value = fotografie.obsah_souboru
                    });

                    // Výstupní parametr
                    var outputParam = new OracleParameter("p_new_id", OracleDbType.Int32, ParameterDirection.Output);
                    cmd.Parameters.Add(outputParam);

                    cmd.ExecuteNonQuery();

                    // Získání nového ID
                    newId = Convert.ToInt32(outputParam.Value.ToString());
                }
            }

            return newId;
        }

		//metoda nastaví v tabulce pes u daného psa (podle id) id fotografie na nové id fotografie
        public void UpdatePesFotografie(int pesId, int fotografieId)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("update_pes_fotografie", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů
                    cmd.Parameters.Add(new OracleParameter("p_pes_id", pesId));
                    cmd.Parameters.Add(new OracleParameter("p_fotografie_id", fotografieId));

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateFotografie(Fotografie fotografie)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("update_fotografie", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů
                    cmd.Parameters.Add(new OracleParameter("p_nazev", fotografie.nazev_souboru));
                    cmd.Parameters.Add(new OracleParameter("p_typ", fotografie.typ_souboru));
                    cmd.Parameters.Add(new OracleParameter("p_pripona", fotografie.pripona_souboru));
                    cmd.Parameters.Add(new OracleParameter("p_obsah", OracleDbType.Blob)
                    {
                        Value = fotografie.obsah_souboru
                    });
                    cmd.Parameters.Add(new OracleParameter("p_datum", DateTime.Now)); // datum_zmeny
                    cmd.Parameters.Add(new OracleParameter("p_zmeneno_id_osoba", fotografie.nahrano_id_osoba)); // zmeneno_id_osoba
                    cmd.Parameters.Add(new OracleParameter("p_id_fotografie", fotografie.id_fotografie)); // id_fotografie

                    // Spuštění procedury
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
                    "SELECT id_fotografie, nazev_souboru, typ_souboru, obsah_souboru FROM view_fotografie_by_pes WHERE id_fotografie = (SELECT id_fotografie FROM Psi WHERE id_psa = :pesId)", con))
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

                using (var cmd = new OracleCommand("vloz_psa", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů
                    cmd.Parameters.Add(new OracleParameter("p_jmeno", string.IsNullOrEmpty(jmeno) ? DBNull.Value : jmeno));
                    cmd.Parameters.Add(new OracleParameter("p_cislo_cipu", string.IsNullOrEmpty(cisloCipu) ? DBNull.Value : cisloCipu));
                    cmd.Parameters.Add(new OracleParameter("p_narozeni", datumNarozeni.HasValue ? (object)datumNarozeni.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_plemeno_id", plemenoId));
                    cmd.Parameters.Add(new OracleParameter("p_barva_id", barvaId));
                    cmd.Parameters.Add(new OracleParameter("p_fotografie_id", fotografieId.HasValue ? (object)fotografieId.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_pohlavi_id", pohlaviId));
                    cmd.Parameters.Add(new OracleParameter("p_vaha", vaha));

                    // Výstupní parametr pro ID nově vloženého psa
                    var newIdParam = new OracleParameter("p_new_id", OracleDbType.Int32)
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

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();

                foreach (var vlastnostId in vlastnostiIds)
                {
                    using (var cmd = new OracleCommand("pridat_vlastnost_psovi", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Přidání parametrů do procedury
                        cmd.Parameters.Add(new OracleParameter("p_pes_id", pesId));
                        cmd.Parameters.Add(new OracleParameter("p_vlastnost_id", vlastnostId));

                        // Spuštění procedury
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        //metody pro přidání pobytu psa, který už v útulku kdysi byl
        public void AktualizujMajitelePsaNaNull(int pesId)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("aktualizuj_majitele_psa_na_null", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametru do procedury
                    cmd.Parameters.Add(new OracleParameter("p_pes_id", pesId));

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AktualizujVahuPsa(int pesId, double vaha)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("aktualizuj_vahu_psa", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů
                    cmd.Parameters.Add(new OracleParameter("p_pes_id", pesId));
                    cmd.Parameters.Add(new OracleParameter("p_vaha", vaha));

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }

		///////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void ZpracujMajiteleBezPsa(int osobaId)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();

				// 1. Zjistit, zda osoba stále vlastní nějaké psy
				using (var checkCmd = new OracleCommand("SELECT COUNT(*) FROM PSI WHERE MAJITEL_ID_OSOBA = :osobaId", con))
				{
					checkCmd.Parameters.Add(new OracleParameter("osobaId", osobaId));

					int pocetPsu = Convert.ToInt32(checkCmd.ExecuteScalar());
					if (pocetPsu > 0)
					{
						return; // Osoba stále vlastní psy, nic neděláme
					}
				}

				// 2. Odebrat záznam z tabulky MAJITELE
				using (var deleteCmd = new OracleCommand("DELETE FROM MAJITELE WHERE ID_OSOBA = :osobaId", con))
				{
					deleteCmd.Parameters.Add(new OracleParameter("osobaId", osobaId));
					deleteCmd.ExecuteNonQuery();
				}

				// 3. Aktualizovat typ osoby
				using (var getTypeCmd = new OracleCommand("SELECT TYP_OSOBY FROM OSOBY WHERE ID_OSOBA = :osobaId", con))
				{
					getTypeCmd.Parameters.Add(new OracleParameter("osobaId", osobaId));

					var typOsoby = getTypeCmd.ExecuteScalar()?.ToString();
					if (!string.IsNullOrEmpty(typOsoby))
					{
						string novyTyp = typOsoby == "M" ? "U" : typOsoby == "P" ? "R" : null;

						if (novyTyp != null)
						{
							using (var updateCmd = new OracleCommand("UPDATE OSOBY SET TYP_OSOBY = :novyTyp WHERE ID_OSOBA = :osobaId", con))
							{
								updateCmd.Parameters.Add(new OracleParameter("novyTyp", novyTyp));
								updateCmd.Parameters.Add(new OracleParameter("osobaId", osobaId));
								updateCmd.ExecuteNonQuery();
							}
						}
					}
				}
			}
		}

		public int? GetMajitelIdByPesId(int pesId)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();

				using (var cmd = new OracleCommand("SELECT MAJITEL_ID_OSOBA FROM PSI WHERE ID_PSA = :pesId", con))
				{
					cmd.Parameters.Add(new OracleParameter("pesId", pesId));

					var result = cmd.ExecuteScalar();
					if (result != DBNull.Value && result != null)
					{
						return Convert.ToInt32(result);
					}
				}
			}
			return null; // Pokud nebyl nalezen žádný majitel
		}


		//metoda pro získání osoby podle mailu
		// Metoda pro přihlášení uživatele
		public Osoba OveritEmail(string email)
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
							return new Osoba
							{
								JMENO = reader.GetString(0),
								PRIJMENI = reader.GetString(1),
								TELEFON = reader.GetString(2),
								ID_OSOBA = reader.GetInt32(3),
								TYP_OSOBY = reader.GetString(4),
								EMAIL = reader.GetString(5),
								HESLO = reader.GetString(6),
								SALT = reader.GetString(7)
							};

						}
					}
				}
			}
			return null;
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

		//metoda pro vyřízení adopce
		public void PridejAdopci(int idPsa, int majitelIdOsoba)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();

				using (var cmd = new OracleCommand("PRIDEJ_ADOPCI", con))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					Console.WriteLine("id psa v PrideAdopci Adopce:" + idPsa);
					Console.WriteLine("id majitele v PrideAdopci Adopce:" + majitelIdOsoba);
					// Předání vstupních parametrů proceduře
					cmd.Parameters.Add("p_id_psa", OracleDbType.Int32).Value = idPsa;
					cmd.Parameters.Add("p_majitel_id_osoba", OracleDbType.Int32).Value = majitelIdOsoba;

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void UkonciPobyt(int pesId)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("AktualizujKonecPobytu", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Přidání parametru pro ID psa
					cmd.Parameters.Add(new OracleParameter("p_id_psa", pesId));

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}



	}
}
