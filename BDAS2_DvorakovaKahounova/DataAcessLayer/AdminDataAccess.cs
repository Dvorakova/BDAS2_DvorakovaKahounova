using BDAS2_DvorakovaKahounova.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace BDAS2_DvorakovaKahounova.DataAcessLayer
{
    public class AdminDataAccess
    {
        private string _connectionString;

        public AdminDataAccess(string configuration)
        {
            _connectionString = configuration;
        }

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
                            celkemDavkaKg = oracleDecimal.Value; // Používáme Value k získání decimal hodnoty
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

                    // Definice výstupního parametru
                    OracleParameter outputParam = new OracleParameter("p_pocet_psu", OracleDbType.Decimal);
                    outputParam.Direction = ParameterDirection.Output;

                    command.Parameters.Add(outputParam);

                    // Spustíme proceduru
                    command.ExecuteNonQuery();

                    // Použijeme převod na OracleDecimal a následně na Int32
                    if (outputParam.Value != DBNull.Value)
                    {
                        OracleDecimal oracleDecimalValue = (OracleDecimal)outputParam.Value;

                        // Převedeme OracleDecimal na Int32
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

                        // Definice výstupního parametru
                        OracleParameter outputParam = new OracleParameter("p_pocet_psu", OracleDbType.Decimal);
                        outputParam.Direction = ParameterDirection.Output;

                        command.Parameters.Add(outputParam);

                        // Spustíme proceduru
                        command.ExecuteNonQuery();

                        // Použijeme převod na OracleDecimal a následně na Int32
                        if (outputParam.Value != DBNull.Value)
                        {
                            OracleDecimal oracleDecimalValue = (OracleDecimal)outputParam.Value;

                            // Převedeme OracleDecimal na Int32
                            pocetPsu = oracleDecimalValue.ToInt32();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Zpracování chyby
                    Console.WriteLine("Chyba při volání procedury: " + ex.Message);
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
                            // Získáme hodnotu jako double místo decimal
                            prumernyPobyt = Convert.ToDecimal(oracleDecimal.ToDouble());
                        }
                    }
                }
            }

            return prumernyPobyt;
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


    }
}
