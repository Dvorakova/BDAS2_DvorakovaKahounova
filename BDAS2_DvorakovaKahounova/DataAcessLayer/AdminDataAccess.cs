﻿using BDAS2_DvorakovaKahounova.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;

namespace BDAS2_DvorakovaKahounova.DataAcessLayer
{
	public class AdminDataAccess
	{
		private string _connectionString;

		public AdminDataAccess(string configuration)
		{
			_connectionString = configuration;
		}

		//metoda pro zobrazení logů (historie úprav)
		public List<Log> GetLogs()
		{
			var logs = new List<Log>();
			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				using (var command = new OracleCommand("SELECT ID_LOG, NAZEV_TABULKY, AKCE, POPIS_AKCE, CAS FROM view_logovani", connection))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							logs.Add(new Log
							{
								IdLog = Convert.ToInt32(reader["ID_LOG"]),
								NazevTabulky = reader["NAZEV_TABULKY"].ToString(),
								Akce = reader["AKCE"].ToString(),
								PopisAkce = reader["POPIS_AKCE"].ToString(),
								Cas = Convert.ToDateTime(reader["CAS"])
							});
						}
					}
				}
			}
			return logs;
		}

		//Metody pro statistiky:

		public decimal SpoctiKrmneDavky()
		{
			decimal celkemDavkaKg = 0;

			using (var connection = new OracleConnection(_connectionString))
			{
				using (var command = new OracleCommand("SPOCTI_KRMNE_DAVKY", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					// Výstupní parametr
					var outputParam = new OracleParameter("p_celkem_davka_kg", OracleDbType.Decimal)
					{
						Direction = ParameterDirection.Output
					};
					command.Parameters.Add(outputParam);

					connection.Open();
					command.ExecuteNonQuery();

					// Kontrola, zda je hodnota NULL a převod na decimal
					if (outputParam.Value != DBNull.Value)
					{
						OracleDecimal oracleDecimal = (OracleDecimal)outputParam.Value;
						if (!oracleDecimal.IsNull)
						{
							celkemDavkaKg = oracleDecimal.Value;
						}
					}
				}
			}

			return celkemDavkaKg;
		}

		public int SpoctiPocetPsuVUtulku()
		{
			int pocetPsu = 0;

			using (OracleConnection connection = new OracleConnection(_connectionString))
			{
				connection.Open();

				using (OracleCommand command = new OracleCommand("SPOCTI_POCET_PSU_V_UTULKU", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					OracleParameter outputParam = new OracleParameter("p_pocet_psu", OracleDbType.Decimal);
					outputParam.Direction = ParameterDirection.Output;

					command.Parameters.Add(outputParam);

					command.ExecuteNonQuery();

					if (outputParam.Value != DBNull.Value)
					{
						OracleDecimal oracleDecimalValue = (OracleDecimal)outputParam.Value;

						pocetPsu = oracleDecimalValue.ToInt32();
					}
				}
			}

			return pocetPsu;
		}

		public int SpoctiPocetPsuVKarantene()
		{
			int pocetPsu = 0;

			using (OracleConnection connection = new OracleConnection(_connectionString))
			{
				try
				{
					connection.Open();

					using (OracleCommand command = new OracleCommand("SPOCTI_POCET_PSU_V_KARANTENE", connection))
					{
						command.CommandType = CommandType.StoredProcedure;

						OracleParameter outputParam = new OracleParameter("p_pocet_psu", OracleDbType.Decimal);
						outputParam.Direction = ParameterDirection.Output;

						command.Parameters.Add(outputParam);

						command.ExecuteNonQuery();

						if (outputParam.Value != DBNull.Value)
						{
							OracleDecimal oracleDecimalValue = (OracleDecimal)outputParam.Value;

							pocetPsu = oracleDecimalValue.ToInt32();
						}
					}
				}
				catch (Exception ex)
				{
					// Zpracování chyby
					pocetPsu = 0; // Pokud nastane chyba, nastavíme výstup na 0
				}
			}

			return pocetPsu;
		}

		public decimal SpoctiPrumernouDobuPobytu()
		{
			decimal prumernyPobyt = 0;

			using (var connection = new OracleConnection(_connectionString))
			{
				using (var command = new OracleCommand("SPOCTI_PRUMERNOU_DOBU_POBYTU", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					// Výstupní parametr
					var outputParam = new OracleParameter("p_prumerny_pobyt", OracleDbType.Decimal)
					{
						Direction = ParameterDirection.Output
					};
					command.Parameters.Add(outputParam);

					connection.Open();
					command.ExecuteNonQuery();

					if (outputParam.Value != DBNull.Value)
					{
						OracleDecimal oracleDecimal = (OracleDecimal)outputParam.Value;
						if (!oracleDecimal.IsNull)
						{
							prumernyPobyt = Convert.ToDecimal(oracleDecimal.ToDouble());
						}
					}
				}
			}

			return prumernyPobyt;
		}

		// Metoda pro výpis systémového katalogu:
		public List<KatalogItem> GetKatalogViewAll()
		{
			var katalogItems = new List<KatalogItem>();

			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var cmd = new OracleCommand("SELECT NAZEV, TYP, STATUS FROM KATALOG_VIEW_ALL", con))
				{
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							katalogItems.Add(new KatalogItem
							{
								Nazev = reader.GetString(0),
								Typ = reader.GetString(1),
								Stav = reader.GetString(2)
							});
						}
					}
				}
			}

			return katalogItems;
		}



		//Metody pro emulaci:
		// Metoda pro výpis všech uživatelů
		public List<Osoba> GetAllUsers()
		{
			var users = new List<Osoba>();

			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var cmd = new OracleCommand("SELECT JMENO, PRIJMENI, EMAIL, ID_OSOBA, TYP_OSOBY FROM VIEW_ALL_USERS", con))
				{
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							users.Add(new Osoba
							{
								JMENO = reader.GetString(0),
								PRIJMENI = reader.GetString(1),
								EMAIL = reader.GetString(2),
								ID_OSOBA = reader.GetInt32(3),
								TYP_OSOBY = reader.GetString(4)
							});
						}
					}
				}
			}
			return users;
		}

		public Osoba GetUserById(int userId)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var cmd = new OracleCommand("SELECT JMENO, PRIJMENI, EMAIL, ID_OSOBA, TYP_OSOBY FROM OSOBY WHERE ID_OSOBA = :userId", con))
				{
					cmd.Parameters.Add(new OracleParameter("userId", userId));

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							return new Osoba
							{
								JMENO = reader.GetString(0),
								PRIJMENI = reader.GetString(1),
								EMAIL = reader.GetString(2),
								ID_OSOBA = reader.GetInt32(3),
								TYP_OSOBY = reader.GetString(4)
							};
						}
					}
				}
			}
			return null;
		}

		// metody pro načtení všech tabulek z databáze
		public List<TableInfo> VypisTabulkyASloupce()
		{
			var tablesAndColumns = new List<TableInfo>();

			using (var con = new OracleConnection(_connectionString))
			{

				con.Open();
				var command = new OracleCommand("SELECT * FROM KATALOG_VIEW_TABLES_AND_COLUMNS", con);
				var reader = command.ExecuteReader();

				while (reader.Read())
				{
					var tableName = reader["TABLE_NAME"].ToString();
					var columnName = reader["COLUMN_NAME"].ToString();
					var dataType = reader["DATA_TYPE"].ToString();

					tablesAndColumns.Add(new TableInfo
					{
						TableName = tableName,
						ColumnName = columnName,
						DataType = dataType
					});
				}
			}
			return tablesAndColumns;
		}

		internal List<Dictionary<string, string>> TableContent(string tableName)
		{
			var rows = new List<Dictionary<string, string>>();

			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				var command = new OracleCommand($"SELECT * FROM {tableName}", con);
				var reader = command.ExecuteReader();

				while (reader.Read())
				{
					var row = new Dictionary<string, string>();
					for (int i = 0; i < reader.FieldCount; i++)
					{
						row[reader.GetName(i)] = reader[i]?.ToString();
					}
					rows.Add(row);
				}
			}
			return rows;
		}

		//Vyhledávání - univerzální metoda pro volání procedur pro vyhledávání
		public List<Dictionary<string, string>> SearchTableProcedure(string tableName, string searchTerm)
		{
			var results = new List<Dictionary<string, string>>();
			bool isDate = DateTime.TryParse(searchTerm, out DateTime dateValue);

			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var cmd = new OracleCommand())
				{
					cmd.Connection = con;
					cmd.CommandType = CommandType.StoredProcedure;

					// Dynamické sestavení názvu procedury
					string prefix =/* isNumber ? "N_" :*/ isDate ? "D_" : "T_";
					cmd.CommandText = $"vyhledavani_administratorem.{prefix}{tableName}";

					// Přidání parametrů
					//if (isNumber)
					//{
					//    cmd.Parameters.Add("searchTerm", OracleDbType.Decimal).Value = numericValue;
					//}
					//else
					if (isDate)
					{
						cmd.Parameters.Add("searchTerm", OracleDbType.Date).Value = dateValue;
					}
					else
					{
						cmd.Parameters.Add("searchTerm", OracleDbType.Varchar2).Value = searchTerm;
					}
					cmd.Parameters.Add("results", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

					// Zpracování výsledků
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var row = new Dictionary<string, string>();
							for (int i = 0; i < reader.FieldCount; i++)
							{
								row[reader.GetName(i)] = reader[i]?.ToString();
							}
							results.Add(row);
						}
					}
				}
			}

			return results;
		}

		// metoda pro zavolání funkce pro výpis zaměstnanců pracujících pod daným zaměstnancem (hierarchický dotaz)
		public List<Osoba> GetHierarchy(int vstupniId)
		{
			var osoby = new List<Osoba>();

			try
			{
				using (var connection = new OracleConnection(_connectionString))
				{
					connection.Open();

					// PL/SQL blok pro volání funkce s REF CURSOR
					string query = "BEGIN :cursor := ZISKEJ_HIERARCHII(:vstupniId); END;";

					using (var command = new OracleCommand(query, connection))
					{
						// Přidání parametru pro výstupní SYS_REFCURSOR
						var cursorParam = new OracleParameter("cursor", OracleDbType.RefCursor, ParameterDirection.Output);
						command.Parameters.Add(cursorParam);

						// Parametr pro vstupní ID
						var vstupniIdParam = new OracleParameter("vstupniId", OracleDbType.Int32);
						vstupniIdParam.Value = vstupniId;
						command.Parameters.Add(vstupniIdParam);

						// Spuštění příkazu
						using (var reader = command.ExecuteReader())
						{
							// Zpracování dat z REF CURSOR
							while (reader.Read())
							{
								var osoba = new Osoba
								{
									ID_OSOBA = reader.GetInt32(reader.GetOrdinal("ID_OSOBA")),
									JMENO = reader.GetString(reader.GetOrdinal("JMENO")),
									PRIJMENI = reader.GetString(reader.GetOrdinal("PRIJMENI")),
									TELEFON = reader.IsDBNull(reader.GetOrdinal("TELEFON")) ? null : reader.GetString(reader.GetOrdinal("TELEFON")),
									EMAIL = reader.IsDBNull(reader.GetOrdinal("EMAIL")) ? null : reader.GetString(reader.GetOrdinal("EMAIL"))
								};

								osoby.Add(osoba);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw;
			}

			return osoby;
		}

//-----------------------------------------------------------------------------------------------
		//metody pro načítání do comboboxů
		public List<Pes> GetPsi()
		{
			var psi = new List<Pes>();

			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT ID_PSA, JmenoPsa, CisloCipu FROM CB_PSI";
				using (var command = new OracleCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						psi.Add(new Pes
						{
							ID_PSA = reader.GetInt32(0),
							JMENO = !reader.IsDBNull(1) ? reader.GetString(1) : "Neznámý",
							CISLO_CIPU = !reader.IsDBNull(2) ? reader.GetString(2) : "Bez čipu"
						});
					}
				}
			}

			return psi;
		}

		public List<Osoba> GetZamestnanci()
		{
			var zamestnanci = new List<Osoba>();

			try
			{
				using (var connection = new OracleConnection(_connectionString))
				{
					connection.Open();

					// SQL dotaz pro získání seznamu zaměstnanců
					string query = "SELECT ID_OSOBA, JMENO, PRIJMENI FROM VIEW_VYPIS_CHOVATELE";

					using (var command = new OracleCommand(query, connection))
					{
						using (var reader = command.ExecuteReader())
						{
							// Zpracování výsledků
							while (reader.Read())
							{
								var osoba = new Osoba
								{
									ID_OSOBA = reader.GetInt32(reader.GetOrdinal("ID_OSOBA")),
									JMENO = reader.GetString(reader.GetOrdinal("JMENO")),
									PRIJMENI = reader.GetString(reader.GetOrdinal("PRIJMENI"))
								};

								zamestnanci.Add(osoba);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw;
			}

			return zamestnanci;
		}

		public List<Osoba> GetMajitele()
		{
			var majitele = new List<Osoba>();

			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT ID_OSOBA, JMENO, PRIJMENI FROM CB_MAJITELE";
				using (var command = new OracleCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						majitele.Add(new Osoba
						{
							ID_OSOBA = reader.GetInt32(reader.GetOrdinal("ID_OSOBA")),
							JMENO = reader.GetString(reader.GetOrdinal("JMENO")),
							PRIJMENI = reader.GetString(reader.GetOrdinal("PRIJMENI"))
						});
					}
				}
			}

			return majitele;
		}

		public List<Osoba> GetRezervatori()
		{
			var seznam = new List<Osoba>();

			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT ID_OSOBA, JMENO, PRIJMENI FROM CB_REZERVATORI";
				using (var command = new OracleCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						seznam.Add(new Osoba
						{
							ID_OSOBA = reader.GetInt32(reader.GetOrdinal("ID_OSOBA")),
							JMENO = reader.GetString(reader.GetOrdinal("JMENO")),
							PRIJMENI = reader.GetString(reader.GetOrdinal("PRIJMENI"))
						});
					}
				}
			}

			return seznam;
		}

		public List<Osoba> GetOsoby()
		{
			var seznam = new List<Osoba>();

			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT ID_OSOBA, JMENO, PRIJMENI FROM CB_OSOBY";
				using (var command = new OracleCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						seznam.Add(new Osoba
						{
							ID_OSOBA = reader.GetInt32(reader.GetOrdinal("ID_OSOBA")),
							JMENO = reader.GetString(reader.GetOrdinal("JMENO")),
							PRIJMENI = reader.GetString(reader.GetOrdinal("PRIJMENI"))
						});
					}
				}
			}

			return seznam;
		}

		public List<Barva> GetBarvy()
		{
			var seznam = new List<Barva>();

			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT ID_BARVA, NAZEV FROM CB_BARVY";
				using (var command = new OracleCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						seznam.Add(new Barva
						{
							Id = reader.GetInt32(reader.GetOrdinal("ID_BARVA")),
							Nazev = reader.GetString(reader.GetOrdinal("NAZEV"))
						});
					}
				}
			}

			return seznam;
		}

		public List<Plemeno> GetPlemena()
		{
			var seznam = new List<Plemeno>();

			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT ID_PLEMENO, NAZEV FROM CB_PLEMENA";
				using (var command = new OracleCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						seznam.Add(new Plemeno
						{
							Id = reader.GetInt32(reader.GetOrdinal("ID_PLEMENO")),
							Nazev = reader.GetString(reader.GetOrdinal("NAZEV"))
						});
					}
				}
			}

			return seznam;
		}

		public List<DuvodPobytu> GetDuvody()
		{
			var seznam = new List<DuvodPobytu>();

			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT ID_DUVOD, DUVOD FROM CB_DUVODY";
				using (var command = new OracleCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						seznam.Add(new DuvodPobytu
						{
							Id = reader.GetInt32(reader.GetOrdinal("ID_DUVOD")),
							Nazev = reader.GetString(reader.GetOrdinal("DUVOD"))
						});
					}
				}
			}

			return seznam;
		}

		public List<Predpis> GetPredpisy()
		{
			var seznam = new List<Predpis>();

			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT ID_PREDPIS, povinny_pocet_dnu_karanteny FROM CB_PREDPIS";
				using (var command = new OracleCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						seznam.Add(new Predpis
						{
							Id = reader.GetInt32(reader.GetOrdinal("ID_PREDPIS")),
							PocetDni = reader.GetInt32(reader.GetOrdinal("povinny_pocet_dnu_karanteny"))
						});
					}
				}
			}

			return seznam;
		}

		public List<Pohlavi> GetPohlavi()
		{
			var seznam = new List<Pohlavi>();

			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT ID_POHLAVI, NAZEV FROM CB_POHLAVI";
				using (var command = new OracleCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						seznam.Add(new Pohlavi
						{
							Id = reader.GetInt32(reader.GetOrdinal("ID_POHLAVI")),
							Nazev = reader.GetString(reader.GetOrdinal("NAZEV"))
						});
					}
				}
			}

			return seznam;
		}

		public List<Vlastnost> GetVlastnosti()
		{
			var seznam = new List<Vlastnost>();

			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT ID_VLASTNOST, NAZEV FROM CB_VLASTNOSTI";
				using (var command = new OracleCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						seznam.Add(new Vlastnost
						{
							Id = reader.GetInt32(reader.GetOrdinal("ID_VLASTNOST")),
							Nazev = reader.GetString(reader.GetOrdinal("NAZEV"))
						});
					}
				}
			}

			return seznam;
		}

		public List<Fotografie> GetFotografie()
		{
			var seznam = new List<Fotografie>();

			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT ID_FOTOGRAFIE, NAZEV_SOUBORU FROM CB_FOTOGRAFIE";
				using (var command = new OracleCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						seznam.Add(new Fotografie
						{
							id_fotografie = reader.GetInt32(reader.GetOrdinal("ID_FOTOGRAFIE")),
							nazev_souboru = reader.GetString(reader.GetOrdinal("NAZEV_SOUBORU"))
						});
					}
				}
			}

			return seznam;
		}

		public List<Pobyt> GetPobyty()
		{
			var seznam = new List<Pobyt>();

			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT ID_POBYT, JMENO, CISLO_CIPU FROM CB_POBYTY";
				using (var command = new OracleCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{

						var pobyt = new Pobyt
						{
							Id = reader.GetInt32(reader.GetOrdinal("ID_POBYT")),
							JmenoPsa = !reader.IsDBNull(1) ? reader.GetString(1) : "Neznámý",
							cisloCipu = !reader.IsDBNull(2) ? reader.GetString(2) : "Bez čipu"
						};
						seznam.Add(pobyt);
					}
				}
			}

			return seznam;
		}

		public List<Zaznam> GetZaznamy()
		{
			var seznam = new List<Zaznam>();

			using (var connection = new OracleConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT ID_ZAZNAM_POBYT, JMENO, CISLO_CIPU FROM CB_ZAZNAMY";
				using (var command = new OracleCommand(query, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var zaznam = new Zaznam
						{
							Id = reader.GetInt32(reader.GetOrdinal("ID_ZAZNAM_POBYT")),
							JmenoPsa = !reader.IsDBNull(1) ? reader.GetString(1) : "Neznámý",
							cisloCipu = !reader.IsDBNull(2) ? reader.GetString(2) : "Bez čipu"

						};
						seznam.Add(zaznam);

					}
				}
			}

			return seznam;
		}
//--------------------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------------------
		//metoda pro přidání nového záznamu do (jakékoli) tabulky v databázi
		//rozcestník pro správnou proceduru pro přidání záznamu
		public void InsertRecord(string tableName, Dictionary<string, string> values)
		{
			// Switch podle názvu tabulky
			switch (tableName.ToLower())
			{
				case "barvy":
					Z_barvy(values);
					break;
				case "adopce":
					Z_adopce(values);
					break;
				case "duvody_pobytu":
					Z_duvody_pobytu(values);
					break;
				case "fotografie":
					Z_fotografie(values);
					break;
				case "majitele":
					Z_majitele(values);
					break;
				case "ockovani":
					Z_ockovani(values);
					break;
				case "odcerveni":
					Z_odcerveni(values);
					break;
				case "osoby":
					Z_osoby(values);
					break;
				case "plemena":
					Z_plemena(values);
					break;
				case "pobyty":
					Z_pobyty(values);
					break;
				case "predpisy_karanteny":
					Z_predpisy_karanteny(values);
					break;
				case "psi":
					Z_psi(values);
					break;
				case "pohlavi":
					Z_pohlavi(values);
					break;
				case "psi_vlastnosti":
					Z_psi_vlastnosti(values);
					break;
				case "rezervace":
					Z_rezervace(values);
					break;
				case "rezervatori":
					Z_rezervatori(values);
					break;
				case "vlastnosti":
					Z_vlastnosti(values);
					break;
				case "zaznamy_o_prubehu_pobytu":
					Z_zaznamy_o_prubehu_pobytu(values);
					break;
				default:
					throw new ArgumentException($"Neznámá tabulka: {tableName}");
			}
		}
//Metody pro přidání záznamu (označeny písmenem Z_):
		private void Z_zaznamy_o_prubehu_pobytu(Dictionary<string, string> values)
		{
			int? idPobyt = values.ContainsKey("ID_POBYT") && int.TryParse(values["ID_POBYT"], out int id) ? id : (int?)null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_zaznamy_o_prubehu_pobytu", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add(new OracleParameter("p_id_pobyt", idPobyt.HasValue ? (object)idPobyt.Value : DBNull.Value));

					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Z_vlastnosti(Dictionary<string, string> values)
		{
			string nazev = values.ContainsKey("NAZEV") ? values["NAZEV"] : string.Empty;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_vlastnosti", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add(new OracleParameter("p_nazev", nazev ?? string.Empty)); 

					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Z_rezervatori(Dictionary<string, string> values)
		{
			int? idOsoba = values.ContainsKey("ID_OSOBA") && int.TryParse(values["ID_OSOBA"], out int id) ? id : (int?)null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_rezervatori", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add(new OracleParameter("p_id_osoba", idOsoba.HasValue ? (object)idOsoba.Value : DBNull.Value));

					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Z_rezervace(Dictionary<string, string> values)
		{
			int? idPsa = values.ContainsKey("ID_PSA") ? (int?)Convert.ToInt32(values["ID_PSA"]) : null;
			DateTime? datum = values.ContainsKey("DATUM") ? (DateTime?)Convert.ToDateTime(values["DATUM"]) : null;
			int? rezervatorIdOsoba = values.ContainsKey("REZERVATOR_ID_OSOBA") ? (int?)Convert.ToInt32(values["REZERVATOR_ID_OSOBA"]) : null;
			string rezervaceKod = values.ContainsKey("REZERVACE_KOD") ? values["REZERVACE_KOD"] : null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_rezervace", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa.HasValue ? (object)idPsa.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_datum", datum.HasValue ? (object)datum.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_id_adopce", DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_rezervator_id_osoba", rezervatorIdOsoba.HasValue ? (object)rezervatorIdOsoba.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_rezervace_kod", rezervaceKod ?? "")); // Pokud není hodnot, použije se DBNull.Value

					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Z_psi_vlastnosti(Dictionary<string, string> values)
		{
			int? idVlastnost = values.ContainsKey("ID_VLASTNOST") ? (int?)Convert.ToInt32(values["ID_VLASTNOST"]) : null;
			int? idPsa = values.ContainsKey("ID_PSA") ? (int?)Convert.ToInt32(values["ID_PSA"]) : null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_psi_vlastnosti", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add(new OracleParameter("p_id_vlastnost", idVlastnost.HasValue ? (object)idVlastnost.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa.HasValue ? (object)idPsa.Value : DBNull.Value));

					cmd.ExecuteNonQuery();
				}
			}
		}



		private void Z_pohlavi(Dictionary<string, string> values)
		{
			string nazev = values.ContainsKey("NAZEV") ? values["NAZEV"] : null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_pohlavi", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add(new OracleParameter("p_nazev", nazev ?? ""));

					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Z_psi(Dictionary<string, string> values)
		{
			string jmeno = values.ContainsKey("JMENO") ? values["JMENO"] : null;
			string cisloCipu = values.ContainsKey("CISLO_CIPU") ? values["CISLO_CIPU"] : null;
			DateTime? narozeni = values.ContainsKey("NAROZENI") && DateTime.TryParse(values["NAROZENI"], out DateTime narozeniVal) ? narozeniVal : (DateTime?)null;
			int? idPlemeno = values.ContainsKey("ID_PLEMENO") && int.TryParse(values["ID_PLEMENO"], out int idPlemenoVal) ? idPlemenoVal : (int?)null;
			int? idBarva = values.ContainsKey("ID_BARVA") && int.TryParse(values["ID_BARVA"], out int idBarvaVal) ? idBarvaVal : (int?)null;
			int? majitelIdOsoba = values.ContainsKey("MAJITEL_ID_OSOBA") && int.TryParse(values["MAJITEL_ID_OSOBA"], out int majitelIdOsobaVal) ? majitelIdOsobaVal : (int?)null;
			int? idPohlavi = values.ContainsKey("ID_POHLAVI") && int.TryParse(values["ID_POHLAVI"], out int idPohlaviVal) ? idPohlaviVal : (int?)null;
			int? idFotografie = values.ContainsKey("ID_FOTOGRAFIE") && int.TryParse(values["ID_FOTOGRAFIE"], out int idFotografieVal) ? idFotografieVal : (int?)null;
			decimal? vaha = values.ContainsKey("VAHA") && decimal.TryParse(values["VAHA"], out decimal vahaVal) ? vahaVal : (decimal?)null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_psi", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add(new OracleParameter("p_jmeno", jmeno ?? ""));
					cmd.Parameters.Add(new OracleParameter("p_cislo_cipu", cisloCipu ?? ""));
					cmd.Parameters.Add(new OracleParameter("p_narozeni", narozeni.HasValue ? (object)narozeni.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_id_plemeno", idPlemeno.HasValue ? (object)idPlemeno.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_id_barva", idBarva.HasValue ? (object)idBarva.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_majitel_id_osoba", majitelIdOsoba.HasValue ? (object)majitelIdOsoba.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_id_fotografie", idFotografie.HasValue ? (object)idFotografie.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_id_pohlavi", idPohlavi.HasValue ? (object)idPohlavi.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_vaha", vaha.HasValue ? (object)vaha.Value : DBNull.Value));

					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Z_predpisy_karanteny(Dictionary<string, string> values)
		{
			int? povinnyPocetDnuKaranteny = values.ContainsKey("POVINNY_POCET_DNU_KARANTENY") && int.TryParse(values["POVINNY_POCET_DNU_KARANTENY"], out int povinnyPocetVal) ? povinnyPocetVal : (int?)null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_predpisy_karanteny", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add(new OracleParameter("p_povinny_pocet_dnu_karanteny", povinnyPocetDnuKaranteny.HasValue ? (object)povinnyPocetDnuKaranteny.Value : DBNull.Value));

					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Z_pobyty(Dictionary<string, string> values)
		{
			int? idPsa = values.ContainsKey("ID_PSA") && int.TryParse(values["ID_PSA"], out int idPsaVal) ? idPsaVal : (int?)null;
			DateTime? zacatekPobytu = values.ContainsKey("ZACATEK_POBYTU") && DateTime.TryParse(values["ZACATEK_POBYTU"], out DateTime zacatekVal) ? zacatekVal : (DateTime?)null;
			int? idDuvod = values.ContainsKey("ID_DUVOD") && int.TryParse(values["ID_DUVOD"], out int idDuvodVal) ? idDuvodVal : (int?)null;
			int? idPredpis = values.ContainsKey("ID_PREDPIS") && int.TryParse(values["ID_PREDPIS"], out int idPredpisVal) ? idPredpisVal : (int?)null;
			decimal? krmnaDavka = values.ContainsKey("KRMNA_DAVKA") && decimal.TryParse(values["KRMNA_DAVKA"], out decimal krmnaDavkaVal) ? krmnaDavkaVal : (decimal?)null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_pobyty", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa.HasValue ? (object)idPsa.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_zacatek_pobytu", zacatekPobytu.HasValue ? (object)zacatekPobytu.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_id_duvod", idDuvod.HasValue ? (object)idDuvod.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_id_predpis", idPredpis.HasValue ? (object)idPredpis.Value : DBNull.Value));
					cmd.Parameters.Add(new OracleParameter("p_krmna_davka", krmnaDavka.HasValue ? (object)krmnaDavka.Value : DBNull.Value));

					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Z_plemena(Dictionary<string, string> values)
		{
			string nazev = values.ContainsKey("NAZEV") ? values["NAZEV"] : null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_plemena", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add(new OracleParameter("p_nazev", string.IsNullOrEmpty(nazev) ? DBNull.Value : (object)nazev));

					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Z_osoby(Dictionary<string, string> values)
		{
			string jmeno = values.ContainsKey("JMENO") ? values["JMENO"] : null;
			string prijmeni = values.ContainsKey("PRIJMENI") ? values["PRIJMENI"] : null;
			string telefon = values.ContainsKey("TELEFON") ? values["TELEFON"] : null;
			string typOsoby = values.ContainsKey("TYP_OSOBY") ? values["TYP_OSOBY"] : null;
			string email = values.ContainsKey("EMAIL") ? values["EMAIL"] : null;
			string heslo = values.ContainsKey("HESLO") ? values["HESLO"] : null;
			string salt = values.ContainsKey("SALT") ? values["SALT"] : null;
			string idNadrizenehoStr = values.ContainsKey("ID_NADRIZENEHO") ? values["ID_NADRIZENEHO"] : null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_osoby", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add(new OracleParameter("p_jmeno", string.IsNullOrEmpty(jmeno) ? DBNull.Value : (object)jmeno));
					cmd.Parameters.Add(new OracleParameter("p_prijmeni", string.IsNullOrEmpty(prijmeni) ? DBNull.Value : (object)prijmeni));
					cmd.Parameters.Add(new OracleParameter("p_telefon", string.IsNullOrEmpty(telefon) ? DBNull.Value : (object)telefon));
					cmd.Parameters.Add(new OracleParameter("p_typ_osoby", string.IsNullOrEmpty(typOsoby) ? DBNull.Value : (object)typOsoby));
					cmd.Parameters.Add(new OracleParameter("p_email", string.IsNullOrEmpty(email) ? DBNull.Value : (object)email));
					cmd.Parameters.Add(new OracleParameter("p_heslo", string.IsNullOrEmpty(heslo) ? DBNull.Value : (object)heslo));
					cmd.Parameters.Add(new OracleParameter("p_salt", string.IsNullOrEmpty(salt) ? DBNull.Value : (object)salt));
					if (!string.IsNullOrEmpty(idNadrizenehoStr) && int.TryParse(idNadrizenehoStr, out int idNadrizeneho))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_nadrizeneho", idNadrizeneho));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_nadrizeneho", DBNull.Value));
					}
					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Z_odcerveni(Dictionary<string, string> values)
		{
			string datumOdcerveni = values.ContainsKey("DATUM") ? values["DATUM"] : null;
			string idZaznamPobyt = values.ContainsKey("ID_ZAZNAM_POBYT") ? values["ID_ZAZNAM_POBYT"] : null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_odcerveni", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					OracleParameter datumParam = new OracleParameter("p_datum", OracleDbType.Date);
					if (DateTime.TryParse(datumOdcerveni, out DateTime parsedDatum))
					{
						datumParam.Value = parsedDatum; 
					}
					else
					{
						datumParam.Value = DBNull.Value;  
					}
					cmd.Parameters.Add(datumParam);

					if (string.IsNullOrEmpty(idZaznamPobyt))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", DBNull.Value));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", OracleDbType.Int32) { Value = int.Parse(idZaznamPobyt) });
					}

					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Z_ockovani(Dictionary<string, string> values)
		{
			string datumOckovani = values.ContainsKey("DATUM") ? values["DATUM"] : null;
			string idZaznamPobyt = values.ContainsKey("ID_ZAZNAM_POBYT") ? values["ID_ZAZNAM_POBYT"] : null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_ockovani", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					OracleParameter datumParam = new OracleParameter("p_datum", OracleDbType.Date);
					if (DateTime.TryParse(datumOckovani, out DateTime parsedDatum))
					{
						datumParam.Value = parsedDatum;  
					}
					else
					{
						datumParam.Value = DBNull.Value; 
					}
					cmd.Parameters.Add(datumParam);

					if (string.IsNullOrEmpty(idZaznamPobyt))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", DBNull.Value));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", OracleDbType.Int32) { Value = int.Parse(idZaznamPobyt) });
					}

					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Z_majitele(Dictionary<string, string> values)
		{
			string idOsoba = values.ContainsKey("ID_OSOBA") ? values["ID_OSOBA"] : null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_majitele", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					OracleParameter param = new OracleParameter("p_id_osoba", OracleDbType.Int32);
					param.Value = string.IsNullOrEmpty(idOsoba) ? DBNull.Value : (object)int.Parse(idOsoba);

					cmd.Parameters.Add(param);

					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Z_fotografie(Dictionary<string, string> values)
		{
			string nazevSouboru = values.ContainsKey("NAZEV_SOUBORU") ? values["NAZEV_SOUBORU"] : null;
			string typSouboru = values.ContainsKey("TYP_SOUBORU") ? values["TYP_SOUBORU"] : null;
			string priponaSouboru = values.ContainsKey("PRIPONA_SOUBORU") ? values["PRIPONA_SOUBORU"] : null;
			string datumNahrani = values.ContainsKey("DATUM_NAHRANI") ? values["DATUM_NAHRANI"] : null;
			string nahranoIdOsoba = values.ContainsKey("NAHRANO_ID_OSOBA") ? values["NAHRANO_ID_OSOBA"] : null;
			string obsahSouboru = values.ContainsKey("OBSAH_SOUBORU") ? values["OBSAH_SOUBORU"] : null;

			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				using (var cmd = new OracleCommand("pridani_administratorem.Z_fotografie", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add(new OracleParameter("p_nazev_souboru", nazevSouboru ?? ""));
					cmd.Parameters.Add(new OracleParameter("p_typ_souboru", typSouboru ?? ""));
					cmd.Parameters.Add(new OracleParameter("p_pripona_souboru", priponaSouboru ?? ""));
					cmd.Parameters.Add(new OracleParameter("p_nahrano_id_osoba", string.IsNullOrEmpty(nahranoIdOsoba) ? DBNull.Value : (object)int.Parse(nahranoIdOsoba)));

					OracleParameter datumParam = new OracleParameter("p_datum_nahrani", OracleDbType.Date);
					if (DateTime.TryParse(datumNahrani, out DateTime parsedDatum))
					{
						datumParam.Value = parsedDatum;
					}
					else
					{
						datumParam.Value = DBNull.Value;
					}
					cmd.Parameters.Add(datumParam);

					cmd.Parameters.Add(new OracleParameter("p_obsah_souboru", DBNull.Value));

					cmd.ExecuteNonQuery();
				}
			}
		}

		public void SaveFotografie(Fotografie fotografie)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				try
				{
					using (var cmd = new OracleCommand("save_fotografie", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						cmd.Parameters.Add(new OracleParameter("p_nazev", fotografie.nazev_souboru));
						cmd.Parameters.Add(new OracleParameter("p_typ", fotografie.typ_souboru));
						cmd.Parameters.Add(new OracleParameter("p_pripona", fotografie.pripona_souboru));
						cmd.Parameters.Add(new OracleParameter("p_datum", fotografie.datum_nahrani));
						cmd.Parameters.Add(new OracleParameter("p_osoba_id", fotografie.nahrano_id_osoba));
						cmd.Parameters.Add(new OracleParameter("p_obsah", OracleDbType.Blob)
						{
							Value = fotografie.obsah_souboru
						});

						var outputParam = new OracleParameter("p_new_id", OracleDbType.Int32, ParameterDirection.Output);
						cmd.Parameters.Add(outputParam);

						cmd.ExecuteNonQuery();

						var newId = Convert.ToInt32(outputParam.Value.ToString());

					}

				}
				catch (Exception ex)
				{
					throw new Exception("Došlo k chybě při ukládání fotografie.", ex);
				}

			}
		}

		public void UpdateFotografie(Fotografie fotografie)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				try
				{
					using (var cmd = new OracleCommand("update_fotografie", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						
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

						cmd.ExecuteNonQuery();
					}

				}
				catch (Exception ex)
				{
					throw new Exception("Došlo k chybě při ukládání fotografie.", ex);
				}

			}
		}


			private void Z_duvody_pobytu(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("pridani_administratorem.Z_duvody_pobytu", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						cmd.Parameters.Add(new OracleParameter("p_duvod", values["DUVOD"] ?? ""));

						cmd.ExecuteNonQuery();
					}
				}
			}


			private void Z_adopce(Dictionary<string, string> values)
			{
				DateTime? datum = null;
				if (!string.IsNullOrEmpty(values["DATUM"]) && DateTime.TryParse(values["DATUM"], out DateTime parsedDatum))
				{
					datum = parsedDatum;
				}

				int? idPsa = null;
				if (!string.IsNullOrEmpty(values["ID_PSA"]) && int.TryParse(values["ID_PSA"], out int parsedIdPsa))
				{
					idPsa = parsedIdPsa;
				}

				int? majitelIdOsoba = null;
				if (!string.IsNullOrEmpty(values["MAJITEL_ID_OSOBA"]) && int.TryParse(values["MAJITEL_ID_OSOBA"], out int parsedMajitelIdOsoba))
				{
					majitelIdOsoba = parsedMajitelIdOsoba;
				}

				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("pridani_administratorem.Z_adopce", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						cmd.Parameters.Add(new OracleParameter("p_datum", datum.HasValue ? (object)datum.Value : DBNull.Value));
						cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa.HasValue ? (object)idPsa.Value : DBNull.Value));
						cmd.Parameters.Add(new OracleParameter("p_majitel_id_osoba", majitelIdOsoba.HasValue ? (object)majitelIdOsoba.Value : DBNull.Value));

						cmd.ExecuteNonQuery();
					}
				}
			}



			private void Z_barvy(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();
					using (var cmd = new OracleCommand("pridani_administratorem.Z_barvy", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add(new OracleParameter("p_nazev", values["NAZEV"]));
						cmd.ExecuteNonQuery();
					}
				}
			}

//-------------------------------------------------------------------------------------------------------------------------------------------------
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Metoda pro odstanění záznamu z tabulky - rozcestník pro volání metody se správnými parametry
			public void DeleteRecord(string tableName, Dictionary<string, string> values)
			{
				int primaryKeyID;

				if (tableName.Equals("osoby", StringComparison.OrdinalIgnoreCase))
				{
					// Čtvrtý sloupec pro tabulku 'osoby'
					var keyColumn = values.Keys.ElementAtOrDefault(3); // Získání názvu 4. sloupce (index 3)
					if (keyColumn != null && int.TryParse(values[keyColumn], out primaryKeyID))
					{
					}
					else
					{
						return;
					}
				}
				else
				{
					// První sloupec pro ostatní tabulky
					var keyColumn = values.Keys.FirstOrDefault(); // Získání názvu 1. sloupce
					if (keyColumn != null && int.TryParse(values[keyColumn], out primaryKeyID))
					{
					}
					else
					{
						return;
					}
				}
				//předání správných názvů procedur metodě rpo odstranění záznamu
				switch (tableName.ToLower())
				{
					case "barvy":
						CallDeleteProcedure("mazani_administratorem.X_BARVY", primaryKeyID);
						break;
					case "adopce":
						CallDeleteProcedure("mazani_administratorem.X_ADOPCE", primaryKeyID);
						break;
					case "duvody_pobytu":
						CallDeleteProcedure("mazani_administratorem.X_DUVOD_POBYTU", primaryKeyID);
						break;
					case "fotografie":
						CallDeleteProcedure("mazani_administratorem.X_FOTOGRAFIE", primaryKeyID);
						break;
					case "majitele":
						CallDeleteProcedure("mazani_administratorem.X_MAJITELE", primaryKeyID);
						break;
					case "ockovani":
						CallDeleteProcedure("mazani_administratorem.X_OCKOVANI", primaryKeyID);
						break;
					case "odcerveni":
						CallDeleteProcedure("mazani_administratorem.X_ODCERVENI", primaryKeyID);
						break;
					case "osoby":
						CallDeleteProcedure("mazani_administratorem.X_OSOBY", primaryKeyID);
						break;
					case "plemena":
						CallDeleteProcedure("mazani_administratorem.X_PLEMENA", primaryKeyID);
						break;
					case "pobyty":
						CallDeleteProcedure("mazani_administratorem.X_POBYTY", primaryKeyID);
						break;
					case "predpisy_karanteny":
						CallDeleteProcedure("mazani_administratorem.X_PREDPISY_KARANTENY", primaryKeyID);
						break;
					case "psi":
						CallDeleteProcedure("mazani_administratorem.X_PSI", primaryKeyID);
						break;
					case "pohlavi":
						CallDeleteProcedure("mazani_administratorem.X_POHLAVI", primaryKeyID);
						break;
					case "psi_vlastnosti":
					// přidání druhého parametru pro proceduru přidání do tabulky psi_vlastnosti
						int secondaryKeyID = 0;
						var keyColumn = values.Keys.ElementAtOrDefault(1);
						if (keyColumn != null && int.TryParse(values[keyColumn], out secondaryKeyID))
						{
						}
						else
						{
							return;
						}
						CallDeleteDogsProcedure(primaryKeyID, secondaryKeyID);
						break;
					case "rezervace":
						CallDeleteProcedure("mazani_administratorem.X_REZERVACE", primaryKeyID);
						break;
					case "rezervatori":
						CallDeleteProcedure("mazani_administratorem.X_REZERVATORI", primaryKeyID);
						break;
					case "vlastnosti":
						CallDeleteProcedure("mazani_administratorem.X_VLASTNOSTI", primaryKeyID);
						break;
					case "zaznamy_o_prubehu_pobytu":
						CallDeleteProcedure("mazani_administratorem.X_ZAZNAMY_O_PRUBEHU_POBYTU", primaryKeyID);
						break;
					default:
						throw new ArgumentException($"Neznámá tabulka: {tableName}");
				}

			}

		//volání procedury pro smazání záznamu z tabulky psi_vlastnosti
			private void CallDeleteDogsProcedure(int primaryKeyID, object secondaryKeyID)
			{
				using (var con = new OracleConnection(_connectionString))
				{
					con.Open();
					using (var command = new OracleCommand("mazani_administratorem.X_PSI_VLASTNOSTI", con))
					{
						command.CommandType = CommandType.StoredProcedure;

						command.Parameters.Add(new OracleParameter("p_primaryKeyID", primaryKeyID));
						command.Parameters.Add(new OracleParameter("p_secondaryKeyID", secondaryKeyID));

						command.ExecuteNonQuery();
					}
				}
			}

		// metoda pro volání (správné) procedury pro smazání záznamu
		private void CallDeleteProcedure(string procedureName, int primaryKeyID)
			{
				using (var con = new OracleConnection(_connectionString))
				{
					con.Open();
					using (var command = new OracleCommand(procedureName, con))
					{
						command.CommandType = CommandType.StoredProcedure;

						command.Parameters.Add(new OracleParameter("p_primaryKeyID", primaryKeyID));

						command.ExecuteNonQuery();
					}
				}

			}
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//--------------------------------------------------------------------------------------------------------------------------------------------------

			// Metoda (rozcestník) pro volání metod por editaci jednotlivýxh záznamů (označeny písmenem Y):
			internal void UpdateRecord(string tableName, Dictionary<string, string> values, Dictionary<string, string> oldValues)
			{
				switch (tableName.ToLower())
				{
					case "barvy":
						Y_barvy(values);
						break;
					case "adopce":
						Y_adopce(values);
						break;
					case "duvody_pobytu":
						Y_duvody_pobytu(values);
						break;
					case "fotografie":
						Y_fotografie(values);
						break;
					case "majitele":
						Y_majitele(values, oldValues);
						break;
					case "ockovani":
						Y_ockovani(values);
						break;
					case "odcerveni":
						Y_odcerveni(values);
						break;
					case "osoby":
						Y_osoby(values);
						break;
					case "plemena":
						Y_plemena(values);
						break;
					case "pobyty":
						Y_pobyty(values);
						break;
					case "predpisy_karanteny":
						Y_predpisy_karanteny(values);
						break;
					case "psi":
						Y_psi(values);
						break;
					case "pohlavi":
						Y_pohlavi(values);
						break;
					case "psi_vlastnosti":
						Y_psi_vlastnosti(values, oldValues);
						break;
					case "rezervace":
						Y_rezervace(values);
						break;
					case "rezervatori":
						Y_rezervatori(values, oldValues);
						break;
					case "vlastnosti":
						Y_vlastnosti(values);
						break;
					case "zaznamy_o_prubehu_pobytu":
						Y_zaznamy_o_prubehu_pobytu(values);
						break;
					default:
						throw new ArgumentException($"Neznámá tabulka: {tableName}");
				}

			}

//metody pro úpravu jednoho záznamu v tabulce - volají procedury:
			private void Y_zaznamy_o_prubehu_pobytu(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_zaznamy_o_prubehu_pobytu", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_ZAZNAM_POBYT") && int.TryParse(values["ID_ZAZNAM_POBYT"], out int idZaznamPobyt))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", idZaznamPobyt));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro ID_ZAZNAM_POBYT.");
						}

						if (values.ContainsKey("ID_POBYT") && int.TryParse(values["ID_POBYT"], out int idPobyt))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_pobyt", idPobyt));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro ID_POBYT.");
						}

						cmd.ExecuteNonQuery();
					}
				}
			}


			private void Y_vlastnosti(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_vlastnosti", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_VLASTNOST") && int.TryParse(values["ID_VLASTNOST"], out int idVlastnost))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_vlastnost", idVlastnost));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro ID_VLASTNOST.");
						}

						cmd.Parameters.Add(new OracleParameter("p_nazev", values.ContainsKey("NAZEV") ? values["NAZEV"] : string.Empty));

						cmd.ExecuteNonQuery();
					}
				}
			}


			private void Y_rezervatori(Dictionary<string, string> values, Dictionary<string, string> oldValues)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_rezervatori", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (oldValues.ContainsKey("ID_OSOBA") && int.TryParse(oldValues["ID_OSOBA"], out int oldIdOsoba))
						{
							cmd.Parameters.Add(new OracleParameter("p_old_id_osoba", oldIdOsoba));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro OLD_ID_OSOBA.");
						}

						if (values.ContainsKey("ID_OSOBA") && int.TryParse(values["ID_OSOBA"], out int newIdOsoba))
						{
							cmd.Parameters.Add(new OracleParameter("p_new_id_osoba", newIdOsoba));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro NEW_ID_OSOBA.");
						}

						cmd.ExecuteNonQuery();
					}
				}
			}


			private void Y_rezervace(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_rezervace", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_REZERVACE") && int.TryParse(values["ID_REZERVACE"], out int idRezervace))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_rezervace", idRezervace));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_rezervace", DBNull.Value)); 
						}

						if (values.ContainsKey("ID_PSA") && int.TryParse(values["ID_PSA"], out int idPsa))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro ID_PSA.");
						}

						if (values.ContainsKey("DATUM") && DateTime.TryParse(values["DATUM"], out DateTime datumRezervace))
						{
							cmd.Parameters.Add(new OracleParameter("p_datum", datumRezervace));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro DATUM.");
						}

						if (values.ContainsKey("ID_ADOPCE") && int.TryParse(values["ID_ADOPCE"], out int idAdopce))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_adopce", idAdopce));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_adopce", DBNull.Value));
						}

						if (values.ContainsKey("REZERVATOR_ID_OSOBA") && int.TryParse(values["REZERVATOR_ID_OSOBA"], out int rezervatorIdOsoba))
						{
							cmd.Parameters.Add(new OracleParameter("p_rezervator_id_osoba", rezervatorIdOsoba));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro REZERVATOR_ID_OSOBA.");
						}

						if (values.ContainsKey("REZERVACE_KOD"))
						{
							cmd.Parameters.Add(new OracleParameter("p_rezervace_kod", values["REZERVACE_KOD"]));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_rezervace_kod", DBNull.Value)); 
						}

						cmd.ExecuteNonQuery();
					}
				}
			}


			private void Y_psi_vlastnosti(Dictionary<string, string> values, Dictionary<string, string> oldValues)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_psi_vlastnosti", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (oldValues.ContainsKey("ID_VLASTNOST") && int.TryParse(oldValues["ID_VLASTNOST"], out int oldIdVlastnost))
						{
							cmd.Parameters.Add(new OracleParameter("p_old_id_vlastnost", oldIdVlastnost));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro OLD_ID_VLASTNOST.");
						}

						if (oldValues.ContainsKey("ID_PSA") && int.TryParse(oldValues["ID_PSA"], out int oldIdPsa))
						{
							cmd.Parameters.Add(new OracleParameter("p_old_id_psa", oldIdPsa));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro OLD_ID_PSA.");
						}

						if (values.ContainsKey("ID_VLASTNOST") && int.TryParse(values["ID_VLASTNOST"], out int newIdVlastnost))
						{
							cmd.Parameters.Add(new OracleParameter("p_new_id_vlastnost", newIdVlastnost));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro NEW_ID_VLASTNOST.");
						}

						if (values.ContainsKey("ID_PSA") && int.TryParse(values["ID_PSA"], out int newIdPsa))
						{
							cmd.Parameters.Add(new OracleParameter("p_new_id_psa", newIdPsa));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro NEW_ID_PSA.");
						}

						cmd.ExecuteNonQuery();
					}
				}
			}


			private void Y_pohlavi(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_pohlavi", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_POHLAVI") && int.TryParse(values["ID_POHLAVI"], out int idPohlavi))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_pohlavi", idPohlavi));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_pohlavi", DBNull.Value));
						}

						cmd.Parameters.Add(new OracleParameter("p_nazev", values.ContainsKey("NAZEV") ? values["NAZEV"] : DBNull.Value));

						cmd.ExecuteNonQuery();
					}
				}
			}


			private void Y_psi(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_psi", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_PSA") && int.TryParse(values["ID_PSA"], out int idPsa))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_psa", DBNull.Value)); 
						}

						if (values.ContainsKey("JMENO"))
						{
							cmd.Parameters.Add(new OracleParameter("p_jmeno", values["JMENO"]));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_jmeno", DBNull.Value));
						}

						if (values.ContainsKey("CISLO_CIPU"))
						{
							cmd.Parameters.Add(new OracleParameter("p_cislo_cipu", values["CISLO_CIPU"]));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_cislo_cipu", DBNull.Value));
						}

						if (values.ContainsKey("NAROZENI") && DateTime.TryParse(values["NAROZENI"], out DateTime narozeni))
						{
							cmd.Parameters.Add(new OracleParameter("p_narozeni", narozeni));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_narozeni", DBNull.Value)); 
						}

						if (values.ContainsKey("ID_PLEMENO") && int.TryParse(values["ID_PLEMENO"], out int idPlemeno))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_plemeno", idPlemeno));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_plemeno", DBNull.Value)); 
						}

						if (values.ContainsKey("ID_BARVA") && int.TryParse(values["ID_BARVA"], out int idBarva))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_barva", idBarva));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_barva", DBNull.Value));
						}

						if (values.ContainsKey("MAJITEL_ID_OSOBA") && int.TryParse(values["MAJITEL_ID_OSOBA"], out int majitelIdOsoba))
						{
							cmd.Parameters.Add(new OracleParameter("p_majitel_id_osoba", majitelIdOsoba));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_majitel_id_osoba", DBNull.Value));
						}

						if (values.ContainsKey("ID_FOTOGRAFIE") && int.TryParse(values["ID_FOTOGRAFIE"], out int idFotografie))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_fotografie", idFotografie));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_fotografie", DBNull.Value));
						}

						if (values.ContainsKey("ID_POHLAVI") && int.TryParse(values["ID_POHLAVI"], out int idPohlavi))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_pohlavi", idPohlavi));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_pohlavi", DBNull.Value));
						}

						if (values.ContainsKey("VAHA") && double.TryParse(values["VAHA"], out double vaha))
						{
							cmd.Parameters.Add(new OracleParameter("p_vaha", vaha));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_vaha", DBNull.Value));
						}

						cmd.ExecuteNonQuery();
					}
				}
			}


			private void Y_predpisy_karanteny(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_predpisy_karanteny", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_PREDPIS") && int.TryParse(values["ID_PREDPIS"], out int idPredpis))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_predpis", idPredpis));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_predpis", DBNull.Value));
						}

						if (values.ContainsKey("POVINNY_POCET_DNU_KARANTENY") && int.TryParse(values["POVINNY_POCET_DNU_KARANTENY"], out int povinnyPocetDnu))
						{
							cmd.Parameters.Add(new OracleParameter("p_povinny_pocet_dnu_karanteny", povinnyPocetDnu));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_povinny_pocet_dnu_karanteny", DBNull.Value));
						}

						cmd.ExecuteNonQuery();
					}
				}
			}



			private void Y_pobyty(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_pobyty", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_POBYT") && int.TryParse(values["ID_POBYT"], out int idPobyt))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_pobyt", idPobyt));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_pobyt", DBNull.Value));
						}

						if (values.ContainsKey("ID_PSA") && int.TryParse(values["ID_PSA"], out int idPsa))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_psa", DBNull.Value));
						}

						if (values.ContainsKey("ZACATEK_POBYTU") && DateTime.TryParse(values["ZACATEK_POBYTU"], out DateTime zacatekPobytu))
						{
							cmd.Parameters.Add(new OracleParameter("p_zacatek_pobytu", zacatekPobytu));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_zacatek_pobytu", DBNull.Value));
						}

						if (values.ContainsKey("KONEC_KARANTENY") && DateTime.TryParse(values["KONEC_KARANTENY"], out DateTime konecKaranteny))
						{
							cmd.Parameters.Add(new OracleParameter("p_konec_karanteny", konecKaranteny));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_konec_karanteny", DBNull.Value));
						}

						if (values.ContainsKey("KONEC_POBYTU") && DateTime.TryParse(values["KONEC_POBYTU"], out DateTime konecPobytu))
						{
							cmd.Parameters.Add(new OracleParameter("p_konec_pobytu", konecPobytu));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_konec_pobytu", DBNull.Value));
						}

						if (values.ContainsKey("UMRTI") && DateTime.TryParse(values["UMRTI"], out DateTime umrti))
						{
							cmd.Parameters.Add(new OracleParameter("p_umrti", umrti));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_umrti", DBNull.Value));
						}

						if (values.ContainsKey("ID_DUVOD") && int.TryParse(values["ID_DUVOD"], out int idDuvod))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_duvod", idDuvod));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_duvod", DBNull.Value));
						}

						if (values.ContainsKey("ID_PREDPIS") && int.TryParse(values["ID_PREDPIS"], out int idPredpis))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_predpis", idPredpis));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_predpis", DBNull.Value));
						}

						if (values.ContainsKey("KRMNA_DAVKA") && int.TryParse(values["KRMNA_DAVKA"], out int krmnaDavka))
						{
							cmd.Parameters.Add(new OracleParameter("p_krmna_davka", krmnaDavka));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_krmna_davka", DBNull.Value));
						}

						cmd.ExecuteNonQuery();
					}
				}
			}



			private void Y_plemena(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_plemena", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_PLEMENO") && int.TryParse(values["ID_PLEMENO"], out int idPlemeno))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_plemeno", idPlemeno));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_plemeno", DBNull.Value));
						}

						if (values.ContainsKey("NAZEV"))
						{
							cmd.Parameters.Add(new OracleParameter("p_nazev", values["NAZEV"]));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_nazev", DBNull.Value));
						}

						cmd.ExecuteNonQuery();
					}
				}
			}



			private void Y_osoby(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_osoby", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("JMENO"))
						{
							cmd.Parameters.Add(new OracleParameter("p_jmeno", values["JMENO"]));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_jmeno", DBNull.Value));
						}

						if (values.ContainsKey("PRIJMENI"))
						{
							cmd.Parameters.Add(new OracleParameter("p_prijmeni", values["PRIJMENI"]));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_prijmeni", DBNull.Value));
						}

						if (values.ContainsKey("TELEFON"))
						{
							cmd.Parameters.Add(new OracleParameter("p_telefon", values["TELEFON"]));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_telefon", DBNull.Value));
						}

						if (values.ContainsKey("ID_OSOBA") && int.TryParse(values["ID_OSOBA"], out int idOsoba))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_osoba", idOsoba));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_osoba", DBNull.Value));
						}

						if (values.ContainsKey("TYP_OSOBY") && !string.IsNullOrEmpty(values["TYP_OSOBY"]))
						{
							string typOsoby = values["TYP_OSOBY"];

							if (typOsoby.Length == 1)
							{
								cmd.Parameters.Add(new OracleParameter("p_typ_osoby", typOsoby));
							}
							else
							{
								throw new ArgumentException("Typ osoby musí být jedno písmeno.");
							}
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_typ_osoby", DBNull.Value));
						}

						if (values.ContainsKey("EMAIL"))
						{
							cmd.Parameters.Add(new OracleParameter("p_email", values["EMAIL"]));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_email", DBNull.Value));
						}

						if (values.ContainsKey("HESLO"))
						{
							cmd.Parameters.Add(new OracleParameter("p_heslo", values["HESLO"]));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_heslo", DBNull.Value));
						}

						if (values.ContainsKey("SALT"))
						{
							cmd.Parameters.Add(new OracleParameter("p_salt", values["SALT"]));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_salt", DBNull.Value));
						}

						if (values.ContainsKey("ID_NADRIZENEHO") && int.TryParse(values["ID_NADRIZENEHO"], out int idNadrizeneho))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_nadrizeneho", idNadrizeneho));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_nadrizeneho", DBNull.Value));
						}
						cmd.ExecuteNonQuery();
					}
				}
			}



			private void Y_odcerveni(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_odcerveni", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_ODCERVENI") && int.TryParse(values["ID_ODCERVENI"], out int idOdcerveni))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_odcerveni", idOdcerveni));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_odcerveni", DBNull.Value));
						}

						if (values.ContainsKey("DATUM") && DateTime.TryParse(values["DATUM"], out DateTime datum))
						{
							cmd.Parameters.Add(new OracleParameter("p_datum", datum));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_datum", DBNull.Value));
						}

						if (values.ContainsKey("ID_ZAZNAM_POBYT") && int.TryParse(values["ID_ZAZNAM_POBYT"], out int idZaznamPobyt))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", idZaznamPobyt));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", DBNull.Value));
						}

						cmd.ExecuteNonQuery();
					}
				}
			}



			private void Y_ockovani(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_ockovani", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_OCKOVANI") && int.TryParse(values["ID_OCKOVANI"], out int idOckovani))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_ockovani", idOckovani));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_ockovani", DBNull.Value));
						}

						if (values.ContainsKey("DATUM") && DateTime.TryParse(values["DATUM"], out DateTime datum))
						{
							cmd.Parameters.Add(new OracleParameter("p_datum", datum));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_datum", DBNull.Value));
						}

						if (values.ContainsKey("ID_ZAZNAM_POBYT") && int.TryParse(values["ID_ZAZNAM_POBYT"], out int idZaznamPobyt))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", idZaznamPobyt));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", DBNull.Value));
						}

						cmd.ExecuteNonQuery();
					}
				}
			}

			private void Y_majitele(Dictionary<string, string> values, Dictionary<string, string> oldValues)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_majitele", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (oldValues.ContainsKey("ID_OSOBA") && int.TryParse(oldValues["ID_OSOBA"], out int oldIdOsoba))
						{
							cmd.Parameters.Add(new OracleParameter("p_old_id_osoba", oldIdOsoba));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro OLD_ID_OSOBA.");
						}

						if (values.ContainsKey("ID_OSOBA") && int.TryParse(values["ID_OSOBA"], out int newIdOsoba))
						{
							cmd.Parameters.Add(new OracleParameter("p_new_id_osoba", newIdOsoba));
						}
						else
						{
							throw new ArgumentException("Chybí nebo neplatná hodnota pro NEW_ID_OSOBA.");
						}

						cmd.ExecuteNonQuery();
					}
				}
			}

			private void Y_fotografie(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_fotografie", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_FOTOGRAFIE") && int.TryParse(values["ID_FOTOGRAFIE"], out int idFotografie))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_fotografie", idFotografie));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_fotografie", DBNull.Value));
						}

						cmd.Parameters.Add(new OracleParameter("p_nazev_souboru", values.ContainsKey("NAZEV_SOUBORU") ? values["NAZEV_SOUBORU"] : string.Empty));

						cmd.Parameters.Add(new OracleParameter("p_typ_souboru", values.ContainsKey("TYP_SOUBORU") ? values["TYP_SOUBORU"] : string.Empty));

						cmd.Parameters.Add(new OracleParameter("p_pripona_souboru", values.ContainsKey("PRIPONA_SOUBORU") ? values["PRIPONA_SOUBORU"] : string.Empty));

						if (values.ContainsKey("DATUM_NAHRANI") && DateTime.TryParse(values["DATUM_NAHRANI"], out DateTime datumNahrani))
						{
							cmd.Parameters.Add(new OracleParameter("p_datum_nahrani", datumNahrani));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_datum_nahrani", DBNull.Value));
						}

						if (values.ContainsKey("DATUM_ZMENY") && DateTime.TryParse(values["DATUM_ZMENY"], out DateTime datumZmeny))
						{
							cmd.Parameters.Add(new OracleParameter("p_datum_zmeny", datumZmeny));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_datum_zmeny", DBNull.Value));
						}

						if (values.ContainsKey("NAHRANO_ID_OSOBA") && int.TryParse(values["NAHRANO_ID_OSOBA"], out int nahranoIdOsoba))
						{
							cmd.Parameters.Add(new OracleParameter("p_nahrano_id_osoba", nahranoIdOsoba));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_nahrano_id_osoba", DBNull.Value));
						}

						if (values.ContainsKey("ZMENENO_ID_OSOBA") && int.TryParse(values["ZMENENO_ID_OSOBA"], out int zmenenoIdOsoba))
						{
							cmd.Parameters.Add(new OracleParameter("p_zmeneno_id_osoba", zmenenoIdOsoba));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_zmeneno_id_osoba", DBNull.Value));
						}

						cmd.Parameters.Add(new OracleParameter("p_obsah_souboru", values.ContainsKey("OBSAH_SOUBORU") ? values["OBSAH_SOUBORU"] : string.Empty));

						cmd.ExecuteNonQuery();
					}
				}
			}



			private void Y_duvody_pobytu(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_duvody_pobytu", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_DUVOD") && int.TryParse(values["ID_DUVOD"], out int idDuvod))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_duvod", idDuvod));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_duvod", DBNull.Value));
						}

						cmd.Parameters.Add(new OracleParameter("p_duvod", values.ContainsKey("DUVOD") ? values["DUVOD"] : string.Empty));

						cmd.ExecuteNonQuery();
					}
				}
			}




			private void Y_adopce(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_adopce", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_ADOPCE") && int.TryParse(values["ID_ADOPCE"], out int idAdopce))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_adopce", idAdopce));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_adopce", DBNull.Value));
						}

						if (values.ContainsKey("DATUM") && DateTime.TryParse(values["DATUM"], out DateTime datum))
						{
							cmd.Parameters.Add(new OracleParameter("p_datum", datum));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_datum", DBNull.Value));
						}

						if (values.ContainsKey("ID_PSA") && int.TryParse(values["ID_PSA"], out int idPsa))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_psa", DBNull.Value));
						}

						if (values.ContainsKey("VRACENI_PSA") && bool.TryParse(values["VRACENI_PSA"], out bool vraceniPsa))
						{
							cmd.Parameters.Add(new OracleParameter("p_vraceni_psa", vraceniPsa));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_vraceni_psa", DBNull.Value));
						}

						if (values.ContainsKey("MAJITEL_ID_OSOBA") && int.TryParse(values["MAJITEL_ID_OSOBA"], out int majitelIdOsoba))
						{
							cmd.Parameters.Add(new OracleParameter("p_majitel_id_osoba", majitelIdOsoba));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_majitel_id_osoba", DBNull.Value));
						}

						cmd.ExecuteNonQuery();
					}
				}
			}



			private void Y_barvy(Dictionary<string, string> values)
			{
				using (var conn = new OracleConnection(_connectionString))
				{
					conn.Open();

					using (var cmd = new OracleCommand("uprava_administratorem.Y_barvy", conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (values.ContainsKey("ID_BARVA") && int.TryParse(values["ID_BARVA"], out int idBarva))
						{
							cmd.Parameters.Add(new OracleParameter("p_id_barva", idBarva));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_id_barva", DBNull.Value));
						}

						cmd.Parameters.Add(new OracleParameter("p_nazev", values.ContainsKey("NAZEV") ? values["NAZEV"] : DBNull.Value));

						cmd.ExecuteNonQuery();
					}
				}
			}



		}
	}
