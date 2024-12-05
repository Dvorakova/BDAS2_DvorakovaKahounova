using BDAS2_DvorakovaKahounova.Models;
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

        //Vyhledávání

        public List<Dictionary<string, string>> SearchTableProcedure(string tableName, string searchTerm)
        {
            var results = new List<Dictionary<string, string>>();
            bool isNumber = decimal.TryParse(searchTerm, out decimal numericValue);
            bool isDate = DateTime.TryParse(searchTerm, out DateTime dateValue);

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Dynamické sestavení názvu procedury
                    string prefix = isNumber ? "N_" : isDate ? "D_" : "T_";
                    cmd.CommandText = $"vyhledavani_administratorem.{prefix}{tableName}";

                    // Přidání parametrů
                    if (isNumber)
                    {
                        cmd.Parameters.Add("searchTerm", OracleDbType.Decimal).Value = numericValue;
                    }
                    else if (isDate)
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


        //funguje, ale je bez D_
        //public List<Dictionary<string, string>> SearchTableProcedure(string tableName, string searchTerm)
        //{
        //    var results = new List<Dictionary<string, string>>();
        //    bool isNumber = decimal.TryParse(searchTerm, out decimal numericValue);

        //    using (var con = new OracleConnection(_connectionString))
        //    {
        //        con.Open();
        //        using (var cmd = new OracleCommand())
        //        {
        //            cmd.Connection = con;
        //            cmd.CommandType = CommandType.StoredProcedure;

        //            // Dynamické sestavení názvu procedury
        //            string prefix = isNumber ? "N_" : "T_";
        //            cmd.CommandText = $"vyhledavani_administratorem.{prefix}{tableName}";

        //            // Přidání parametrů
        //            if (isNumber)
        //            {
        //                cmd.Parameters.Add("searchTerm", OracleDbType.Decimal).Value = numericValue;
        //            }
        //            else
        //            {
        //                cmd.Parameters.Add("searchTerm", OracleDbType.Varchar2).Value = searchTerm;
        //            }
        //            cmd.Parameters.Add("results", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

        //            // Zpracování výsledků
        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    var row = new Dictionary<string, string>();
        //                    for (int i = 0; i < reader.FieldCount; i++)
        //                    {
        //                        row[reader.GetName(i)] = reader[i]?.ToString();
        //                    }
        //                    results.Add(row);
        //                }
        //            }
        //        }
        //    }

        //    return results;
        //}


        //public List<Dictionary<string, string>> SearchTable(string tableName, string query)
        //{
        //    var results = new List<Dictionary<string, string>>();

        //    using (var con = new OracleConnection(_connectionString))
        //    {
        //        con.Open();
        //        var command = new OracleCommand(
        //            $"SELECT * FROM VIEW_PSIVUTULKUBEZMAJITELE WHERE LOWER(JMENO) LIKE '%' || :query || '%'", con);
        //        command.Parameters.Add(new OracleParameter("query", query.ToLower()));

        //        var reader = command.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            var row = new Dictionary<string, string>();
        //            for (int i = 0; i < reader.FieldCount; i++)
        //            {
        //                row[reader.GetName(i)] = reader[i]?.ToString();
        //            }
        //            results.Add(row);
        //        }
        //    }
        //    return results;
        //}


        //metoda pro přidání nového záznamu do (jakékoli) tabulky v databázi

        //rozcestník pro správnou přidání proceduru

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

        private void Z_zaznamy_o_prubehu_pobytu(Dictionary<string, string> values)
        {
            int? idPobyt = values.ContainsKey("ID_POBYT") && int.TryParse(values["ID_POBYT"], out int id) ? id : (int?)null;

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_zaznamy_o_prubehu_pobytu", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter("p_id_pobyt", idPobyt.HasValue ? (object)idPobyt.Value : DBNull.Value));

                    // Spuštění procedury
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

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_vlastnosti", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů do příkazu
                    cmd.Parameters.Add(new OracleParameter("p_nazev", nazev ?? string.Empty));  // Používáme prázdný řetězec, pokud 'nazev' je null

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Z_rezervatori(Dictionary<string, string> values)
        {
            // Získání hodnot pro parametr 'id_osoba'. Pokud není hodnota k dispozici, použijeme DBNull.Value.
            int? idOsoba = values.ContainsKey("ID_OSOBA") && int.TryParse(values["ID_OSOBA"], out int id) ? id : (int?)null;

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_rezervatori", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametru pro 'id_osoba'
                    cmd.Parameters.Add(new OracleParameter("p_id_osoba", idOsoba.HasValue ? (object)idOsoba.Value : DBNull.Value));

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Z_rezervace(Dictionary<string, string> values)
        {
            // Získání hodnot pro parametry. Pokud není hodnota k dispozici, použijeme DBNull.Value.
            int? idPsa = values.ContainsKey("ID_PSA") ? (int?)Convert.ToInt32(values["ID_PSA"]) : null;
            DateTime? datum = values.ContainsKey("DATUM") ? (DateTime?)Convert.ToDateTime(values["DATUM"]) : null;
            int? rezervatorIdOsoba = values.ContainsKey("REZERVATOR_ID_OSOBA") ? (int?)Convert.ToInt32(values["REZERVATOR_ID_OSOBA"]) : null;
            string rezervaceKod = values.ContainsKey("REZERVACE_KOD") ? values["REZERVACE_KOD"] : null;

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_rezervace", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů do procedury. Pokud je hodnota null, použije se DBNull.Value.
                    cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa.HasValue ? (object)idPsa.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_datum", datum.HasValue ? (object)datum.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_id_adopce", DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_rezervator_id_osoba", rezervatorIdOsoba.HasValue ? (object)rezervatorIdOsoba.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_rezervace_kod", rezervaceKod ?? "")); // Pokud není hodnot, použije se DBNull.Value

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Z_psi_vlastnosti(Dictionary<string, string> values)
        {
            // Získání hodnot pro parametry. Pokud nejsou hodnoty k dispozici, použijeme DBNull.Value.
            int? idVlastnost = values.ContainsKey("ID_VLASTNOST") ? (int?)Convert.ToInt32(values["ID_VLASTNOST"]) : null;
            int? idPsa = values.ContainsKey("ID_PSA") ? (int?)Convert.ToInt32(values["ID_PSA"]) : null;

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_psi_vlastnosti", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů do procedury. Pokud je hodnota null, použije se DBNull.Value
                    cmd.Parameters.Add(new OracleParameter("p_id_vlastnost", idVlastnost.HasValue ? (object)idVlastnost.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa.HasValue ? (object)idPsa.Value : DBNull.Value));

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }



        private void Z_pohlavi(Dictionary<string, string> values)
        {
            // Získání hodnot pro parametr. Pokud není hodnota k dispozici, použijeme DBNull.Value nebo prázdný řetězec.
            string nazev = values.ContainsKey("NAZEV") ? values["NAZEV"] : null;

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_pohlavi", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů do procedury. Pokud je hodnota null, použije se prázdný řetězec
                    cmd.Parameters.Add(new OracleParameter("p_nazev", nazev ?? ""));

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Z_psi(Dictionary<string, string> values)
        {
            // Získání hodnot pro parametry. Pokud není hodnota k dispozici, použijeme DBNull.Value nebo null
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

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_psi", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů do procedury. Pokud je hodnota null, použije se DBNull.Value
                    cmd.Parameters.Add(new OracleParameter("p_jmeno", jmeno ?? ""));
                    cmd.Parameters.Add(new OracleParameter("p_cislo_cipu", cisloCipu ?? ""));
                    cmd.Parameters.Add(new OracleParameter("p_narozeni", narozeni.HasValue ? (object)narozeni.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_id_plemeno", idPlemeno.HasValue ? (object)idPlemeno.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_id_barva", idBarva.HasValue ? (object)idBarva.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_majitel_id_osoba", majitelIdOsoba.HasValue ? (object)majitelIdOsoba.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_id_fotografie", idFotografie.HasValue ? (object)idFotografie.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_id_pohlavi", idPohlavi.HasValue ? (object)idPohlavi.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_vaha", vaha.HasValue ? (object)vaha.Value : DBNull.Value));

                    // Spuštění procedury
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

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_predpisy_karanteny", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter("p_povinny_pocet_dnu_karanteny", povinnyPocetDnuKaranteny.HasValue ? (object)povinnyPocetDnuKaranteny.Value : DBNull.Value));

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Z_pobyty(Dictionary<string, string> values)
        {
            // Získání hodnot pro parametry. Pokud není hodnota k dispozici, použijeme DBNull.Value nebo null
            int? idPsa = values.ContainsKey("ID_PSA") && int.TryParse(values["ID_PSA"], out int idPsaVal) ? idPsaVal : (int?)null;
            DateTime? zacatekPobytu = values.ContainsKey("ZACATEK_POBYTU") && DateTime.TryParse(values["ZACATEK_POBYTU"], out DateTime zacatekVal) ? zacatekVal : (DateTime?)null;
            int? idDuvod = values.ContainsKey("ID_DUVOD") && int.TryParse(values["ID_DUVOD"], out int idDuvodVal) ? idDuvodVal : (int?)null;
            int? idPredpis = values.ContainsKey("ID_PREDPIS") && int.TryParse(values["ID_PREDPIS"], out int idPredpisVal) ? idPredpisVal : (int?)null;
            decimal? krmnaDavka = values.ContainsKey("KRMNA_DAVKA") && decimal.TryParse(values["KRMNA_DAVKA"], out decimal krmnaDavkaVal) ? krmnaDavkaVal : (decimal?)null;

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_pobyty", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů do procedury. Pokud je hodnota null, použije se DBNull.Value
                    cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa.HasValue ? (object)idPsa.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_zacatek_pobytu", zacatekPobytu.HasValue ? (object)zacatekPobytu.Value : DBNull.Value));
                   cmd.Parameters.Add(new OracleParameter("p_id_duvod", idDuvod.HasValue ? (object)idDuvod.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_id_predpis", idPredpis.HasValue ? (object)idPredpis.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_krmna_davka", krmnaDavka.HasValue ? (object)krmnaDavka.Value : DBNull.Value));

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Z_plemena(Dictionary<string, string> values)
        {
            // Získání hodnoty pro parametr 'nazev'
            string nazev = values.ContainsKey("NAZEV") ? values["NAZEV"] : null;

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_plemena", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametru do procedury. Pokud je hodnota null, použije se DBNull.Value
                    cmd.Parameters.Add(new OracleParameter("p_nazev", string.IsNullOrEmpty(nazev) ? DBNull.Value : (object)nazev));

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Z_osoby(Dictionary<string, string> values)
        {
            // Získání hodnot pro parametry
            string jmeno = values.ContainsKey("JMENO") ? values["JMENO"] : null;
            string prijmeni = values.ContainsKey("PRIJMENI") ? values["PRIJMENI"] : null;
            string telefon = values.ContainsKey("TELEFON") ? values["TELEFON"] : null;
            string typOsoby = values.ContainsKey("TYP_OSOBY") ? values["TYP_OSOBY"] : null;
            string email = values.ContainsKey("EMAIL") ? values["EMAIL"] : null;
            string heslo = values.ContainsKey("HESLO") ? values["HESLO"] : null;
            string salt = values.ContainsKey("SALT") ? values["SALT"] : null;

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_osoby", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů do procedury. Pokud je hodnota null, přidáme DBNull.Value
                    cmd.Parameters.Add(new OracleParameter("p_jmeno", string.IsNullOrEmpty(jmeno) ? DBNull.Value : (object)jmeno));
                    cmd.Parameters.Add(new OracleParameter("p_prijmeni", string.IsNullOrEmpty(prijmeni) ? DBNull.Value : (object)prijmeni));
                    cmd.Parameters.Add(new OracleParameter("p_telefon", string.IsNullOrEmpty(telefon) ? DBNull.Value : (object)telefon));
                    cmd.Parameters.Add(new OracleParameter("p_typ_osoby", string.IsNullOrEmpty(typOsoby) ? DBNull.Value : (object)typOsoby));
                    cmd.Parameters.Add(new OracleParameter("p_email", string.IsNullOrEmpty(email) ? DBNull.Value : (object)email));
                    cmd.Parameters.Add(new OracleParameter("p_heslo", string.IsNullOrEmpty(heslo) ? DBNull.Value : (object)heslo));
                    cmd.Parameters.Add(new OracleParameter("p_salt", string.IsNullOrEmpty(salt) ? DBNull.Value : (object)salt));

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Z_odcerveni(Dictionary<string, string> values)
        {
            // Získání hodnot pro parametry. Pokud není hodnota k dispozici, použijeme DBNull.Value
            string datumOdcerveni = values.ContainsKey("DATUM") ? values["DATUM"] : null;
            string idZaznamPobyt = values.ContainsKey("ID_ZAZNAM_POBYT") ? values["ID_ZAZNAM_POBYT"] : null;

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_odcerveni", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parametr pro datum (typ DateTime)
                    OracleParameter datumParam = new OracleParameter("p_datum", OracleDbType.Date);
                    if (DateTime.TryParse(datumOdcerveni, out DateTime parsedDatum))
                    {
                        datumParam.Value = parsedDatum;  // Pokud je datum validní, použijeme jej
                    }
                    else
                    {
                        datumParam.Value = DBNull.Value;  // Pokud není validní, použijeme DBNull.Value
                    }
                    cmd.Parameters.Add(datumParam);

                    // Parametr pro id_zaznam_pobyt (int, může být null)
                    if (string.IsNullOrEmpty(idZaznamPobyt))
                    {
                        cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", DBNull.Value));
                    }
                    else
                    {
                        cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", OracleDbType.Int32) { Value = int.Parse(idZaznamPobyt) });
                    }

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Z_ockovani(Dictionary<string, string> values)
        {
            // Získání hodnot pro parametry. Pokud není hodnota k dispozici, použijeme DBNull.Value
            string datumOckovani = values.ContainsKey("DATUM") ? values["DATUM"] : null;
            string idZaznamPobyt = values.ContainsKey("ID_ZAZNAM_POBYT") ? values["ID_ZAZNAM_POBYT"] : null;

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_ockovani", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parametr pro datum (typ DateTime)
                    OracleParameter datumParam = new OracleParameter("p_datum", OracleDbType.Date);
                    if (DateTime.TryParse(datumOckovani, out DateTime parsedDatum))
                    {
                        datumParam.Value = parsedDatum;  // Pokud je datum validní, použijeme jej
                    }
                    else
                    {
                        datumParam.Value = DBNull.Value;  // Pokud není validní, použijeme DBNull.Value
                    }
                    cmd.Parameters.Add(datumParam);

                    // Parametr pro id_zaznam_pobyt (int, může být null)
                    if (string.IsNullOrEmpty(idZaznamPobyt))
                    {
                        cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", DBNull.Value));
                    }
                    else
                    {
                        cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", OracleDbType.Int32) { Value = int.Parse(idZaznamPobyt) });
                    }

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Z_majitele(Dictionary<string, string> values)
        {
            // Získání hodnoty pro parametr 'id_osoba' a kontrola, zda je null nebo prázdná
            string idOsoba = values.ContainsKey("ID_OSOBA") ? values["ID_OSOBA"] : null;

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_majitele", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametru do příkazu
                    OracleParameter param = new OracleParameter("p_id_osoba", OracleDbType.Int32);
                    // Kontrola, jestli je hodnota null nebo prázdná, a přiřazení DBNull.Value v takovém případě
                    param.Value = string.IsNullOrEmpty(idOsoba) ? DBNull.Value : (object)int.Parse(idOsoba);

                    cmd.Parameters.Add(param);

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Z_fotografie(Dictionary<string, string> values)
        {
            // Získání hodnot pro parametry. Pokud není hodnota k dispozici, použijeme null nebo DBNull.Value.
            string nazevSouboru = values.ContainsKey("NAZEV_SOUBORU") ? values["NAZEV_SOUBORU"] : null;
            string typSouboru = values.ContainsKey("TYP_SOUBORU") ? values["TYP_SOUBORU"] : null;
            string priponaSouboru = values.ContainsKey("PRIPONA_SOUBORU") ? values["PRIPONA_SOUBORU"] : null;
            string datumNahrani = values.ContainsKey("DATUM_NAHRANI") ? values["DATUM_NAHRANI"] : null;
            string nahranoIdOsoba = values.ContainsKey("NAHRANO_ID_OSOBA") ? values["NAHRANO_ID_OSOBA"] : null;
            string obsahSouboru = values.ContainsKey("OBSAH_SOUBORU") ? values["OBSAH_SOUBORU"] : null;

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_fotografie", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů pro textové hodnoty (pokud jsou null, přiřadíme prázdný řetězec)
                    cmd.Parameters.Add(new OracleParameter("p_nazev_souboru", nazevSouboru ?? ""));
                    cmd.Parameters.Add(new OracleParameter("p_typ_souboru", typSouboru ?? ""));
                    cmd.Parameters.Add(new OracleParameter("p_pripona_souboru", priponaSouboru ?? ""));
                    cmd.Parameters.Add(new OracleParameter("p_nahrano_id_osoba", string.IsNullOrEmpty(nahranoIdOsoba) ? DBNull.Value : (object)int.Parse(nahranoIdOsoba)));

                    // Zpracování hodnoty pro datum (pokud je null, přiřadíme DBNull.Value)
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

                    // Parametr pro obsah souboru, nastavíme na DBNull.Value, pokud není specifikováno
                    cmd.Parameters.Add(new OracleParameter("p_obsah_souboru",  DBNull.Value));

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Z_duvody_pobytu(Dictionary<string, string> values)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_duvody_pobytu", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 'duvod' je textová hodnota, takže ji přidáme přímo
                    cmd.Parameters.Add(new OracleParameter("p_duvod", values["DUVOD"] ?? ""));

                    // Spuštění procedury
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

                // Vytvoření OracleCommand pro zavolání uložené procedury
                using (var cmd = new OracleCommand("pridani_administratorem.Z_adopce", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Přidání parametrů pro proceduru, kontrolujeme, zda jsou hodnoty null
                    cmd.Parameters.Add(new OracleParameter("p_datum", datum.HasValue ? (object)datum.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa.HasValue ? (object)idPsa.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_majitel_id_osoba", majitelIdOsoba.HasValue ? (object)majitelIdOsoba.Value : DBNull.Value));

                    // Spuštění procedury
                    cmd.ExecuteNonQuery();
                }
            }
        }



        private void Z_barvy(Dictionary<string, string> values)
        {
            // Implementace volání uložené procedury Z_barvy
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

		// Pro odstranění záznamu z tabulek:
		public void DeleteRecord(string tableName, Dictionary<string, string> values)
		{
			int primaryKeyID;

			if (tableName.Equals("osoby", StringComparison.OrdinalIgnoreCase))
			{
				// Čtvrtý sloupec pro tabulku 'osoby'
				var keyColumn = values.Keys.ElementAtOrDefault(3); // Získání názvu 4. sloupce (index 3)
				if (keyColumn != null && int.TryParse(values[keyColumn], out primaryKeyID))
				{
					Console.WriteLine($"Primární klíč pro tabulku {tableName}: {primaryKeyID}");
				}
				else
				{
					Console.WriteLine($"Chyba: Nepodařilo se získat primární klíč pro tabulku {tableName}.");
					return;
				}
			}
			else
			{
				// První sloupec pro ostatní tabulky
				var keyColumn = values.Keys.FirstOrDefault(); // Získání názvu 1. sloupce
				if (keyColumn != null && int.TryParse(values[keyColumn], out primaryKeyID))
				{
					Console.WriteLine($"Primární klíč pro tabulku {tableName}: {primaryKeyID}");
				}
				else
				{
					Console.WriteLine($"Chyba: Nepodařilo se získat primár  ní klíč pro tabulku {tableName}.");
					return;
				}
			}

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
                    int secondaryKeyID = 0;
					var keyColumn = values.Keys.ElementAtOrDefault(1); 
					if (keyColumn != null && int.TryParse(values[keyColumn], out secondaryKeyID))
					{
						Console.WriteLine($"Primární klíč pro tabulku {tableName}: {secondaryKeyID}");
					}
					else
					{
						Console.WriteLine($"Chyba: Nepodařilo se získat primární klíč pro tabulku {tableName}.");
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

		private void CallDeleteDogsProcedure(int primaryKeyID, object secondaryKeyID)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var command = new OracleCommand("mazani_administratorem.X_PSI_VLASTNOSTI", con))
				{
					command.CommandType = CommandType.StoredProcedure;

					// Přidání parametru pro primární klíč
					command.Parameters.Add(new OracleParameter("p_primaryKeyID", primaryKeyID));
					command.Parameters.Add(new OracleParameter("p_secondaryKeyID", secondaryKeyID));

					// Volání procedury
					command.ExecuteNonQuery();
				}
			}
		}

		private void CallDeleteProcedure(string procedureName, int primaryKeyID)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var command = new OracleCommand(procedureName, con))
				{
					command.CommandType = CommandType.StoredProcedure;

					// Přidání parametru pro primární klíč
					command.Parameters.Add(new OracleParameter("p_primaryKeyID", primaryKeyID));

					// Volání procedury
					command.ExecuteNonQuery();
				}
			}

			Console.WriteLine($"Procedura {procedureName} byla úspěšně vykonána pro ID {primaryKeyID}.");
		}


		// pro editaci:
		internal void UpdateRecord(string tableName, Dictionary<string, string> values)
		{
		//	var conditions = string.Join(" AND ", values.Select(v => $"{v.Key} = :{v.Value}"));
		//	Console.WriteLine(conditions);
            
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
				//case "majitele":
				//	Y_majitele(values);
				//	break;
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
				//case "psi_vlastnosti":
				//	Y_psi_vlastnosti(values);
				//	break;
				case "rezervace":
					Y_rezervace(values);
					break;
				//case "rezervatori":
				//	Y_rezervatori(values);
				//	break;
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

		private void Y_zaznamy_o_prubehu_pobytu(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_zaznamy_o_prubehu_pobytu", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID záznamu pobytu
					if (values.ContainsKey("ID_ZAZNAM_POBYT") && int.TryParse(values["ID_ZAZNAM_POBYT"], out int idZaznamPobyt))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", idZaznamPobyt));
					}
					else
					{
						throw new ArgumentException("Chybí nebo neplatná hodnota pro ID_ZAZNAM_POBYT.");
					}

					// Předání ID pobytu
					if (values.ContainsKey("ID_POBYT") && int.TryParse(values["ID_POBYT"], out int idPobyt))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_pobyt", idPobyt));
					}
					else
					{
						throw new ArgumentException("Chybí nebo neplatná hodnota pro ID_POBYT.");
					}

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Y_vlastnosti(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_vlastnosti", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID vlastnosti
					if (values.ContainsKey("ID_VLASTNOST") && int.TryParse(values["ID_VLASTNOST"], out int idVlastnost))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_vlastnost", idVlastnost));
					}
					else
					{
						throw new ArgumentException("Chybí nebo neplatná hodnota pro ID_VLASTNOST.");
					}

					// Předání názvu vlastnosti
					cmd.Parameters.Add(new OracleParameter("p_nazev", values.ContainsKey("NAZEV") ? values["NAZEV"] : string.Empty));

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}


		//private void Y_rezervatori(Dictionary<string, string> values)
		//{
		//	throw new NotImplementedException();
		//}

		private void Y_rezervace(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_rezervace", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID rezervace
					if (values.ContainsKey("ID_REZERVACE") && int.TryParse(values["ID_REZERVACE"], out int idRezervace))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_rezervace", idRezervace));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_rezervace", DBNull.Value)); // Pokud chybí ID rezervace
					}

					// Předání ID psa
					if (values.ContainsKey("ID_PSA") && int.TryParse(values["ID_PSA"], out int idPsa))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa));
					}
					else
					{
						throw new ArgumentException("Chybí nebo neplatná hodnota pro ID_PSA.");
					}

					// Předání data rezervace
					if (values.ContainsKey("DATUM") && DateTime.TryParse(values["DATUM"], out DateTime datumRezervace))
					{
						cmd.Parameters.Add(new OracleParameter("p_datum", datumRezervace));
					}
					else
					{
						throw new ArgumentException("Chybí nebo neplatná hodnota pro DATUM.");
					}

					// Předání ID adopce (pokud je k dispozici)
					if (values.ContainsKey("ID_ADOPCE") && int.TryParse(values["ID_ADOPCE"], out int idAdopce))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_adopce", idAdopce));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_adopce", DBNull.Value)); // Pokud chybí ID adopce
					}

					// Předání ID rezervátora (osoby, která rezervaci zadala)
					if (values.ContainsKey("REZERVATOR_ID_OSOBA") && int.TryParse(values["REZERVATOR_ID_OSOBA"], out int rezervatorIdOsoba))
					{
						cmd.Parameters.Add(new OracleParameter("p_rezervator_id_osoba", rezervatorIdOsoba));
					}
					else
					{
						throw new ArgumentException("Chybí nebo neplatná hodnota pro REZERVATOR_ID_OSOBA.");
					}

					// Předání kódu rezervace (pokud je k dispozici)
					if (values.ContainsKey("REZERVACE_KOD"))
					{
						cmd.Parameters.Add(new OracleParameter("p_rezervace_kod", values["REZERVACE_KOD"]));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_rezervace_kod", DBNull.Value)); // Pokud chybí kód rezervace
					}

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}


		//private void Y_psi_vlastnosti(Dictionary<string, string> values)
		//{
		//	throw new NotImplementedException();
		//}

		private void Y_pohlavi(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_pohlavi", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID pohlaví
					if (values.ContainsKey("ID_POHLAVI") && int.TryParse(values["ID_POHLAVI"], out int idPohlavi))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_pohlavi", idPohlavi));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_pohlavi", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání názvu pohlaví
					cmd.Parameters.Add(new OracleParameter("p_nazev", values.ContainsKey("NAZEV") ? values["NAZEV"] : DBNull.Value));

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Y_psi(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_psi", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID psa
					if (values.ContainsKey("ID_PSA") && int.TryParse(values["ID_PSA"], out int idPsa))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_psa", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání jména psa
					if (values.ContainsKey("JMENO"))
					{
						cmd.Parameters.Add(new OracleParameter("p_jmeno", values["JMENO"]));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_jmeno", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání čísla čipu
					if (values.ContainsKey("CISLO_CIPU"))
					{
						cmd.Parameters.Add(new OracleParameter("p_cislo_cipu", values["CISLO_CIPU"]));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_cislo_cipu", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání data narození
					if (values.ContainsKey("NAROZENI") && DateTime.TryParse(values["NAROZENI"], out DateTime narozeni))
					{
						cmd.Parameters.Add(new OracleParameter("p_narozeni", narozeni));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_narozeni", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání ID plemene
					if (values.ContainsKey("ID_PLEMENO") && int.TryParse(values["ID_PLEMENO"], out int idPlemeno))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_plemeno", idPlemeno));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_plemeno", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání ID barvy
					if (values.ContainsKey("ID_BARVA") && int.TryParse(values["ID_BARVA"], out int idBarva))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_barva", idBarva));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_barva", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání ID majitele
					if (values.ContainsKey("MAJITEL_ID_OSOBA") && int.TryParse(values["MAJITEL_ID_OSOBA"], out int majitelIdOsoba))
					{
						cmd.Parameters.Add(new OracleParameter("p_majitel_id_osoba", majitelIdOsoba));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_majitel_id_osoba", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání ID fotografie
					if (values.ContainsKey("ID_FOTOGRAFIE") && int.TryParse(values["ID_FOTOGRAFIE"], out int idFotografie))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_fotografie", idFotografie));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_fotografie", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání ID pohlaví
					if (values.ContainsKey("ID_POHLAVI") && int.TryParse(values["ID_POHLAVI"], out int idPohlavi))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_pohlavi", idPohlavi));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_pohlavi", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání váhy
					if (values.ContainsKey("VAHA") && double.TryParse(values["VAHA"], out double vaha))
					{
						cmd.Parameters.Add(new OracleParameter("p_vaha", vaha));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_vaha", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}


		private void Y_predpisy_karanteny(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_predpisy_karanteny", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID předpisu
					if (values.ContainsKey("ID_PREDPIS") && int.TryParse(values["ID_PREDPIS"], out int idPredpis))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_predpis", idPredpis));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_predpis", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání povinného počtu dnů karantény
					if (values.ContainsKey("POVINNY_POCET_DNU_KARANTENY") && int.TryParse(values["POVINNY_POCET_DNU_KARANTENY"], out int povinnyPocetDnu))
					{
						cmd.Parameters.Add(new OracleParameter("p_povinny_pocet_dnu_karanteny", povinnyPocetDnu));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_povinny_pocet_dnu_karanteny", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}



		private void Y_pobyty(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_pobyty", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID pobytu
					if (values.ContainsKey("ID_POBYT") && int.TryParse(values["ID_POBYT"], out int idPobyt))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_pobyt", idPobyt));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_pobyt", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání ID psa
					if (values.ContainsKey("ID_PSA") && int.TryParse(values["ID_PSA"], out int idPsa))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_psa", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání začátku pobytu
					if (values.ContainsKey("ZACATEK_POBYTU") && DateTime.TryParse(values["ZACATEK_POBYTU"], out DateTime zacatekPobytu))
					{
						cmd.Parameters.Add(new OracleParameter("p_zacatek_pobytu", zacatekPobytu));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_zacatek_pobytu", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání konce karantény
					if (values.ContainsKey("KONEC_KARANTENY") && DateTime.TryParse(values["KONEC_KARANTENY"], out DateTime konecKaranteny))
					{
						cmd.Parameters.Add(new OracleParameter("p_konec_karanteny", konecKaranteny));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_konec_karanteny", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání konce pobytu
					if (values.ContainsKey("KONEC_POBYTU") && DateTime.TryParse(values["KONEC_POBYTU"], out DateTime konecPobytu))
					{
						cmd.Parameters.Add(new OracleParameter("p_konec_pobytu", konecPobytu));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_konec_pobytu", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání úmrtí (pokud existuje)
					if (values.ContainsKey("UMRTI") && DateTime.TryParse(values["UMRTI"], out DateTime umrti))
					{
						cmd.Parameters.Add(new OracleParameter("p_umrti", umrti));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_umrti", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání důvodu pobytu
					if (values.ContainsKey("ID_DUVOD") && int.TryParse(values["ID_DUVOD"], out int idDuvod))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_duvod", idDuvod));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_duvod", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání předpisu karantény
					if (values.ContainsKey("ID_PREDPIS") && int.TryParse(values["ID_PREDPIS"], out int idPredpis))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_predpis", idPredpis));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_predpis", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání krmné dávky
					if (values.ContainsKey("KRMNA_DAVKA") && int.TryParse(values["KRMNA_DAVKA"], out int krmnaDavka))
					{
						cmd.Parameters.Add(new OracleParameter("p_krmna_davka", krmnaDavka));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_krmna_davka", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}



		private void Y_plemena(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_plemena", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID plemene
					if (values.ContainsKey("ID_PLEMENO") && int.TryParse(values["ID_PLEMENO"], out int idPlemeno))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_plemeno", idPlemeno));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_plemeno", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání názvu plemene
					if (values.ContainsKey("NAZEV"))
					{
						cmd.Parameters.Add(new OracleParameter("p_nazev", values["NAZEV"]));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_nazev", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}



		private void Y_osoby(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_osoby", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání jména osoby
					if (values.ContainsKey("JMENO"))
					{
						cmd.Parameters.Add(new OracleParameter("p_jmeno", values["JMENO"]));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_jmeno", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání příjmení osoby
					if (values.ContainsKey("PRIJMENI"))
					{
						cmd.Parameters.Add(new OracleParameter("p_prijmeni", values["PRIJMENI"]));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_prijmeni", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání telefonu osoby
					if (values.ContainsKey("TELEFON"))
					{
						cmd.Parameters.Add(new OracleParameter("p_telefon", values["TELEFON"]));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_telefon", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání ID osoby
					if (values.ContainsKey("ID_OSOBA") && int.TryParse(values["ID_OSOBA"], out int idOsoba))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_osoba", idOsoba));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_osoba", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání typu osoby (pokud existuje)
					if (values.ContainsKey("TYP_OSOBY"))
					{
						// Pokud je to číslo, přetypujeme, jinak předáme text
						if (int.TryParse(values["TYP_OSOBY"], out int typOsoby))
						{
							cmd.Parameters.Add(new OracleParameter("p_typ_osoby", typOsoby));
						}
						else
						{
							cmd.Parameters.Add(new OracleParameter("p_typ_osoby", values["TYP_OSOBY"]));
						}
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_typ_osoby", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání e-mailu osoby
					if (values.ContainsKey("EMAIL"))
					{
						cmd.Parameters.Add(new OracleParameter("p_email", values["EMAIL"]));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_email", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání hesla osoby
					if (values.ContainsKey("HESLO"))
					{
						cmd.Parameters.Add(new OracleParameter("p_heslo", values["HESLO"]));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_heslo", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání soli pro heslo osoby
					if (values.ContainsKey("SALT"))
					{
						cmd.Parameters.Add(new OracleParameter("p_salt", values["SALT"]));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_salt", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}



		private void Y_odcerveni(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_odcerveni", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID odčervení
					if (values.ContainsKey("ID_ODCERVENI") && int.TryParse(values["ID_ODCERVENI"], out int idOdcerveni))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_odcerveni", idOdcerveni));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_odcerveni", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání data odčervení
					if (values.ContainsKey("DATUM") && DateTime.TryParse(values["DATUM"], out DateTime datum))
					{
						cmd.Parameters.Add(new OracleParameter("p_datum", datum));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_datum", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání ID záznamu pobytu
					if (values.ContainsKey("ID_ZAZNAM_POBYT") && int.TryParse(values["ID_ZAZNAM_POBYT"], out int idZaznamPobyt))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", idZaznamPobyt));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}



		private void Y_ockovani(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_ockovani", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID očkování
					if (values.ContainsKey("ID_OCKOVANI") && int.TryParse(values["ID_OCKOVANI"], out int idOckovani))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_ockovani", idOckovani));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_ockovani", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání data očkování
					if (values.ContainsKey("DATUM") && DateTime.TryParse(values["DATUM"], out DateTime datum))
					{
						cmd.Parameters.Add(new OracleParameter("p_datum", datum));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_datum", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání ID záznamu pobytu
					if (values.ContainsKey("ID_ZAZNAM_POBYT") && int.TryParse(values["ID_ZAZNAM_POBYT"], out int idZaznamPobyt))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", idZaznamPobyt));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_zaznam_pobyt", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}



		//private void Y_majitele(Dictionary<string, string> values)
		//{
		//	using (var conn = new OracleConnection(_connectionString))
		//	{
		//		conn.Open();

		//		// Vytvoření OracleCommand pro zavolání uložené procedury
		//		using (var cmd = new OracleCommand("uprava_administratorem.Y_majitele", conn))
		//		{
		//			cmd.CommandType = CommandType.StoredProcedure;

		//			// Předání ID osoby
		//			if (values.ContainsKey("ID_OSOBA") && int.TryParse(values["ID_OSOBA"], out int idOsoba))
		//			{
		//				cmd.Parameters.Add(new OracleParameter("p_id_osoba", idOsoba));
		//			}
		//			else
		//			{
		//				throw new ArgumentException("Chybí nebo neplatná hodnota pro ID_OSOBA.");
		//			}

		//			// Spuštění procedury
		//			cmd.ExecuteNonQuery();
		//		}
		//	}
		//}


		private void Y_fotografie(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_fotografie", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID fotografie
					if (values.ContainsKey("ID_FOTOGRAFIE") && int.TryParse(values["ID_FOTOGRAFIE"], out int idFotografie))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_fotografie", idFotografie));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_fotografie", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání názvu souboru
					cmd.Parameters.Add(new OracleParameter("p_nazev_souboru", values.ContainsKey("NAZEV_SOUBORU") ? values["NAZEV_SOUBORU"] : string.Empty));

					// Předání typu souboru
					cmd.Parameters.Add(new OracleParameter("p_typ_souboru", values.ContainsKey("TYP_SOUBORU") ? values["TYP_SOUBORU"] : string.Empty));

					// Předání přípony souboru
					cmd.Parameters.Add(new OracleParameter("p_pripona_souboru", values.ContainsKey("PRIPONA_SOUBORU") ? values["PRIPONA_SOUBORU"] : string.Empty));

					// Předání data nahrání
					if (values.ContainsKey("DATUM_NAHRANI") && DateTime.TryParse(values["DATUM_NAHRANI"], out DateTime datumNahrani))
					{
						cmd.Parameters.Add(new OracleParameter("p_datum_nahrani", datumNahrani));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_datum_nahrani", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání dalších parametrů, pokud jsou k dispozici
					if (values.ContainsKey("DATUM_ZMENY") && DateTime.TryParse(values["DATUM_ZMENY"], out DateTime datumZmeny))
					{
						cmd.Parameters.Add(new OracleParameter("p_datum_zmeny", datumZmeny));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_datum_zmeny", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání ID osoby, která nahrála
					if (values.ContainsKey("NAHRANO_ID_OSOBA") && int.TryParse(values["NAHRANO_ID_OSOBA"], out int nahranoIdOsoba))
					{
						cmd.Parameters.Add(new OracleParameter("p_nahrano_id_osoba", nahranoIdOsoba));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_nahrano_id_osoba", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání ID osoby, která změnila
					if (values.ContainsKey("ZMENENO_ID_OSOBA") && int.TryParse(values["ZMENENO_ID_OSOBA"], out int zmenenoIdOsoba))
					{
						cmd.Parameters.Add(new OracleParameter("p_zmeneno_id_osoba", zmenenoIdOsoba));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_zmeneno_id_osoba", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání obsahu souboru
					cmd.Parameters.Add(new OracleParameter("p_obsah_souboru", values.ContainsKey("OBSAH_SOUBORU") ? values["OBSAH_SOUBORU"] : string.Empty));

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}



		private void Y_duvody_pobytu(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_duvody_pobytu", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID důvodu
					if (values.ContainsKey("ID_DUVOD") && int.TryParse(values["ID_DUVOD"], out int idDuvod))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_duvod", idDuvod));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_duvod", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání důvodu
					cmd.Parameters.Add(new OracleParameter("p_duvod", values.ContainsKey("DUVOD") ? values["DUVOD"] : string.Empty));

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}




		private void Y_adopce(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_adopce", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID adopce
					if (values.ContainsKey("ID_ADOPCE") && int.TryParse(values["ID_ADOPCE"], out int idAdopce))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_adopce", idAdopce));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_adopce", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání data adopce
					if (values.ContainsKey("DATUM") && DateTime.TryParse(values["DATUM"], out DateTime datum))
					{
						cmd.Parameters.Add(new OracleParameter("p_datum", datum));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_datum", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání ID psa
					if (values.ContainsKey("ID_PSA") && int.TryParse(values["ID_PSA"], out int idPsa))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_psa", idPsa));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_psa", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání informace o vracení psa (pokud je k dispozici a je typu bool)
					if (values.ContainsKey("VRACENI_PSA") && bool.TryParse(values["VRACENI_PSA"], out bool vraceniPsa))
					{
						cmd.Parameters.Add(new OracleParameter("p_vraceni_psa", vraceniPsa));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_vraceni_psa", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání ID majitele osoby
					if (values.ContainsKey("MAJITEL_ID_OSOBA") && int.TryParse(values["MAJITEL_ID_OSOBA"], out int majitelIdOsoba))
					{
						cmd.Parameters.Add(new OracleParameter("p_majitel_id_osoba", majitelIdOsoba));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_majitel_id_osoba", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}



		private void Y_barvy(Dictionary<string, string> values)
		{
			using (var conn = new OracleConnection(_connectionString))
			{
				conn.Open();

				// Vytvoření OracleCommand pro zavolání uložené procedury
				using (var cmd = new OracleCommand("uprava_administratorem.Y_barvy", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					// Předání ID barvy
					if (values.ContainsKey("ID_BARVA") && int.TryParse(values["ID_BARVA"], out int idBarva))
					{
						cmd.Parameters.Add(new OracleParameter("p_id_barva", idBarva));
					}
					else
					{
						cmd.Parameters.Add(new OracleParameter("p_id_barva", DBNull.Value)); // Použití prázdné hodnoty
					}

					// Předání názvu barvy
					cmd.Parameters.Add(new OracleParameter("p_nazev", values.ContainsKey("NAZEV") ? values["NAZEV"] : DBNull.Value));

					// Spuštění procedury
					cmd.ExecuteNonQuery();
				}
			}
		}


	}
}
