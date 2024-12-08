using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Oracle.ManagedDataAccess.Client;
using Org.BouncyCastle.Ocsp;
using System.Data;
using System.Data.Common;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Transactions;

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
		//metoda volající transakci pro práce s fotografiemi
		public void UpravFotografiiTransakci(Fotografie fotografie, int pesId)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				var transaction = con.BeginTransaction();

				try
				{
					// Získáme existující fotografii psa
					var existujiciFotografie = GetFotografieByPesId(pesId, con, transaction);

					if (existujiciFotografie == null)
					{
						// Pokud pes nemá fotografii, přidáme novou
						int fotografieId = SaveFotografie(fotografie, con, transaction);
						UpdatePesFotografie(pesId, fotografieId, con, transaction);
					}
					else
					{
						// Pokud pes již má fotografii, aktualizujeme ji
						fotografie.id_fotografie = existujiciFotografie.id_fotografie;
						UpdateFotografie(fotografie, con, transaction);
					}

					// Commit transakce po úspěšném dokončení všech operací
					transaction.Commit();
				}
				catch (Exception ex)
				{
					// Rollback transakce při chybě
					transaction.Rollback();
					throw; // Vyhození chyby zpět k controlleru
				}
			}
		}

		public int SaveFotografie(Fotografie fotografie, OracleConnection con, OracleTransaction transaction)
		{
			int newId = 0;

			using (var cmd = new OracleCommand("save_fotografie", con))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Transaction = transaction; // Nastavení transakce
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


			return newId;
		}

		//metoda nastaví v tabulce pes u daného psa (podle id) id fotografie na nové id fotografie
		public void UpdatePesFotografie(int pesId, int fotografieId, OracleConnection con, OracleTransaction transaction)
		{
			using (var cmd = new OracleCommand("update_pes_fotografie", con))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Transaction = transaction; // Nastavení transakce

				// Přidání parametrů
				cmd.Parameters.Add(new OracleParameter("p_pes_id", pesId));
				cmd.Parameters.Add(new OracleParameter("p_fotografie_id", fotografieId));

				// Spuštění procedury
				cmd.ExecuteNonQuery();
			}

		}

		public void UpdateFotografie(Fotografie fotografie, OracleConnection con, OracleTransaction transaction)
		{
			using (var cmd = new OracleCommand("update_fotografie", con))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Transaction = transaction; // Nastavení transakce
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


		public Fotografie GetFotografieByPesId(int pesId, OracleConnection con, OracleTransaction transaction)
		{
			Fotografie fotografie = null;

			using (var cmd = new OracleCommand(
				"SELECT id_fotografie, nazev_souboru, typ_souboru, obsah_souboru FROM view_fotografie_by_pes WHERE id_fotografie = (SELECT id_fotografie FROM Psi WHERE id_psa = :pesId)", con))
			{
				cmd.Transaction = transaction; // Nastavení transakce
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


			return fotografie;
		}

		public void ZaznamenatUmrti(int pesId)
		{
			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();

				// Zavolání uložené procedury v databázi pro zaznamenání úmrtí
				using (var command = new OracleCommand("zaznamenat_umrti", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add("p_id_psa", OracleDbType.Int32).Value = pesId;
					command.ExecuteNonQuery();
				}
			}
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
								return null;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
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


		//metoda pro přidání psa do databáze
		internal void VlozNovehoPsa(StringValues jmeno, StringValues cisloNovehoCipu, DateTime? datumNarozeni, int plemenoId, int barvaId, int pohlaviId, int? fotografieId, int vaha, DateTime zacatekPobytu, int duvodPobytuId, int[] vlastnosti)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				var transaction = con.BeginTransaction();

				try
				{
					// 1. Vložení psa do tabulky Psi
					var pesId = VlozPsa(jmeno, cisloNovehoCipu, datumNarozeni, plemenoId, barvaId, pohlaviId, fotografieId, vaha, con, transaction);

					// 2. Vložení pobytu psa do tabulky Pobyty
					var pobytId = VlozPobyt(pesId, zacatekPobytu, duvodPobytuId, vaha, con, transaction);

					// 3. Vložení záznamu o pobytu
					VlozZaznamOPobytu(pobytId, con, transaction);

					// 4. Vložení vlastností do tabulky psi_vlastnosti
					PridatVlastnostiDoDatabaze(pesId, vlastnosti, con, transaction);

					// Commit transakce po úspěšném dokončení všech operací
					transaction.Commit();
				}
				catch (Exception ex)
				{
					// Rollback transakce při chybě
					transaction.Rollback();
					throw; // Vyhození chyby zpět k controlleru
				}
			}

		}


		//Samotná metoda přidání nového psa do tabulky psi
		public int VlozPsa(string jmeno, string cisloCipu, DateTime? datumNarozeni, int plemenoId, int barvaId, int pohlaviId, int? fotografieId, int vaha, OracleConnection con, OracleTransaction transaction)
		{
			using (var cmd = new OracleCommand("vloz_psa", con))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Transaction = transaction;
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

		//vlozeni pobytu (voláno po vložení psa)
		public int VlozPobyt(int idPes, DateTime zacatekPobytu, int idDuvod, double vaha, OracleConnection con, OracleTransaction transaction)
		{

			using (var command = new OracleCommand("VlozPobyt", con))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.Transaction = transaction;
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

		//poté voláno
		public void VlozZaznamOPobytu(int pobytId, OracleConnection con, OracleTransaction transaction)
		{

			using (var command = new OracleCommand("VlozZaznamOPobytu", con))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.Transaction = transaction;
				// Parametr procedury
				command.Parameters.Add("p_id_pobyt", OracleDbType.Int32).Value = pobytId;

				// Spuštění procedury
				command.ExecuteNonQuery();
			}


		}

		//metoda pro přidání vlastností do databáze pro nového psa
		public void PridatVlastnostiDoDatabaze(int pesId, int[] vlastnostiIds, OracleConnection con, OracleTransaction transaction)
		{
			// Pokud není žádná vlastnost zaškrtnutá, nic neprovádíme
			if (vlastnostiIds == null || vlastnostiIds.Length == 0)
				return;


			foreach (var vlastnostId in vlastnostiIds)
			{
				using (var cmd = new OracleCommand("pridat_vlastnost_psovi", con))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Transaction = transaction;
					// Přidání parametrů do procedury
					cmd.Parameters.Add(new OracleParameter("p_pes_id", pesId));
					cmd.Parameters.Add(new OracleParameter("p_vlastnost_id", vlastnostId));

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}

		}

		internal void PridatPobytPsovi(int pesId, DateTime zacatekPobytu, int idDuvod, double vaha)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				var transaction = con.BeginTransaction();

				try
				{
					// 1. Pokud je důvod pobytu "odložen majitelem", aktualizujeme psa
					if (idDuvod == 1)
					{
						var osobaId = GetMajitelIdByPesId(pesId, con, transaction);
						AktualizujMajitelePsaNaNull(pesId, con, transaction); // Nastavení majitele na null

						// Pokud je nějaký majitel, zpracujeme jej
						if (osobaId != null)
						{
							ZpracujMajiteleBezPsa(osobaId.Value, con, transaction);
						}
					}

					// 2. Aktualizace váhy psa
					AktualizujVahuPsa(pesId, vaha, con, transaction); // Metoda pro aktualizaci váhy

					// 3. Vložení nového pobytu
					int pobytId = VlozPobyt(pesId, zacatekPobytu, idDuvod, vaha, con, transaction); // Vložení pobytu do tabulky

					// 4. Vložení záznamu o pobytu
					VlozZaznamOPobytu(pobytId, con, transaction); // Vložení záznamu o pobytu

					// Pokud všechny operace proběhly bez chyb, commitujeme transakci
					transaction.Commit();
				}
				catch (Exception ex)
				{
					// Pokud nastane chyba, rollback transakce
					transaction.Rollback();
					throw; // Zpět nahoru k controlleru
				}
			}
		}


		//metody pro přidání pobytu psa, který už v útulku kdysi byl
		public void AktualizujMajitelePsaNaNull(int pesId, OracleConnection con, OracleTransaction transaction)
		{
			using (var cmd = new OracleCommand("aktualizuj_majitele_psa_na_null", con))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Transaction = transaction;
				// Přidání parametru do procedury
				cmd.Parameters.Add(new OracleParameter("p_pes_id", pesId));

				// Spuštění procedury
				cmd.ExecuteNonQuery();
			}

		}

		public void AktualizujVahuPsa(int pesId, double vaha, OracleConnection con, OracleTransaction transaction)
		{
			using (var cmd = new OracleCommand("aktualizuj_vahu_psa", con))
			{
				cmd.Transaction = transaction;  // Nastavíme transakci
				cmd.CommandType = CommandType.StoredProcedure;

				// Přidání parametrů
				cmd.Parameters.Add(new OracleParameter("p_pes_id", pesId));
				cmd.Parameters.Add(new OracleParameter("p_vaha", vaha));

				// Spuštění procedury
				cmd.ExecuteNonQuery();
			}

		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void ZpracujMajiteleBezPsa(int osobaId, OracleConnection con, OracleTransaction transaction)
		{

			// 1. Zjistit, zda osoba stále vlastní nějaké psy
			using (var checkCmd = new OracleCommand("SELECT COUNT(*) FROM PSI WHERE MAJITEL_ID_OSOBA = :osobaId", con))
			{
				checkCmd.Transaction = transaction;  // Nastavíme transakci
				checkCmd.Parameters.Add(new OracleParameter("osobaId", osobaId));

				int pocetPsu = Convert.ToInt32(checkCmd.ExecuteScalar());
				if (pocetPsu > 0)
				{
					return; // Osoba stále vlastní psy, nic neděláme
				}
			}

			// 2. Aktualizovat typ osoby
			using (var getTypeCmd = new OracleCommand("SELECT TYP_OSOBY FROM OSOBY WHERE ID_OSOBA = :osobaId", con))
			{
				getTypeCmd.Transaction = transaction;  // Nastavíme transakci
				getTypeCmd.Parameters.Add(new OracleParameter("osobaId", osobaId));

				var typOsoby = getTypeCmd.ExecuteScalar()?.ToString();
				if (!string.IsNullOrEmpty(typOsoby))
				{
					string novyTyp = typOsoby == "M" ? "U" : typOsoby == "P" ? "R" : null;

					if (novyTyp != null)
					{
						using (var cmd = new OracleCommand("UPDATE_TYP_OSOBY", con))
						{
							cmd.CommandType = CommandType.StoredProcedure;
							cmd.Transaction = transaction;

							// Parametry pro proceduru
							cmd.Parameters.Add(new OracleParameter("p_novy_typ", OracleDbType.Char)).Value = novyTyp;
							cmd.Parameters.Add(new OracleParameter("p_osoba_id", OracleDbType.Int32)).Value = osobaId;

							// Spustíme proceduru
							cmd.ExecuteNonQuery();
						}
					}
				}
			}

		}

		public int? GetMajitelIdByPesId(int pesId, OracleConnection con, OracleTransaction transaction)
		{

			using (var cmd = new OracleCommand("SELECT MAJITEL_ID_OSOBA FROM PSI WHERE ID_PSA = :pesId", con))
			{
				cmd.Transaction = transaction;
				cmd.Parameters.Add(new OracleParameter("pesId", pesId));

				var result = cmd.ExecuteScalar();
				if (result != DBNull.Value && result != null)
				{
					return Convert.ToInt32(result);
				}
			}

			return null; // Pokud nebyl nalezen žádný majitel
		}


		//metoda pro získání osoby podle mailu
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


		public bool ExistujeRezervace(int rezervatorIdOsoba, int idPes, OracleConnection con, OracleTransaction transaction)
		{
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
					return false;
				}


				// Kontrola, zda výsledek je 1 (TRUE) nebo 0 (FALSE)
				return Convert.ToInt32(result) == 1;
			}

		}

		//metoda pro adopci psa (bez rezervace) - s využitím transakce
		public void AdopcePsa(int majitelIdOsoba, int idPes)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var transaction = con.BeginTransaction())
				{
					try
					{
						// Zpracování změny rezervátora
						ZpracujRezervatorZmena(majitelIdOsoba, idPes, con, transaction);

						// Přidání majitele
						PridejMajitele(majitelIdOsoba, con, transaction);

						// Přidání adopce
						PridejAdopci(idPes, majitelIdOsoba, con, transaction);

						// Commit transakce
						transaction.Commit();
					}
					catch (Exception ex)
					{
						// Pokud dojde k chybě, rollback
						transaction.Rollback();
						throw new Exception("Chyba při adopci psa: " + ex.Message);
					}
				}
			}
		}


		//metoda rpo zpracování, zda se mění typ osoby
		public void ZpracujRezervatorZmena(int majitelIdOsoba, int idPes, OracleConnection con, OracleTransaction transaction)
		{
			// 1. Zkontroluj, zda existuje rezervace pro daného rezervátora a psa
			bool existujeRezervace = ExistujeRezervace(majitelIdOsoba, idPes, con, transaction);

			if (existujeRezervace)
			{
				// 2. Pokud existuje, změň typ osoby na "P" v tabulce Osoby
				using (var cmd = new OracleCommand("UPDATE_TYP_OSOBY", con))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Transaction = transaction;

					// Parametry pro proceduru
					cmd.Parameters.Add(new OracleParameter("p_novy_typ", OracleDbType.Char)).Value = 'P';
					cmd.Parameters.Add(new OracleParameter("p_osoba_id", OracleDbType.Int32)).Value = majitelIdOsoba;

					// Spustíme proceduru
					cmd.ExecuteNonQuery();
				}
			}
			else
			{
				// Změň typ osoby na "M" v tabulce Osoby
				using (var cmd = new OracleCommand("UPDATE_TYP_OSOBY", con))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Transaction = transaction;

					// Parametry pro proceduru
					cmd.Parameters.Add(new OracleParameter("p_novy_typ", OracleDbType.Char)).Value = 'M';
					cmd.Parameters.Add(new OracleParameter("p_osoba_id", OracleDbType.Int32)).Value = majitelIdOsoba;

					// Spustíme proceduru
					cmd.ExecuteNonQuery();
				}
			}

		}

		internal void PridejMajitele(int majitelIdOsoba, OracleConnection con, OracleTransaction transaction)
		{
			using (var cmd = new OracleCommand("pridat_majitele", con))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Transaction = transaction;

				// Parametr pro proceduru
				cmd.Parameters.Add(new OracleParameter("p_id_osoba", OracleDbType.Int32)).Value = majitelIdOsoba;

				// Spustíme proceduru
				cmd.ExecuteNonQuery();
			}
		}

		//metoda pro vyřízení adopce
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
