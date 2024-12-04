using BDAS2_DvorakovaKahounova.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
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
                    cmd.Parameters.Add(new OracleParameter("p_id_pohlavi", idPohlavi.HasValue ? (object)idPohlavi.Value : DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("p_id_fotografie", idFotografie.HasValue ? (object)idFotografie.Value : DBNull.Value));

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

        // Pokračuj s metodami pro všechny tabulky podle stejného vzoru...



    }
}
