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
                                NAROZENI = reader.GetDateTime(1),
                                POHLAVI = reader.GetInt32(2),
                                PLEMENO = reader.GetString(3),
                                BARVA = reader.GetString(4),
                                VLASTNOSTI = reader.GetString(5),
                                CISLO_CIPU = reader.GetString(6),
                                ID_FOTOGRAFIE = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7) // Ošetření null hodnot
                            };
                        }
                    }
                }
            }

            return null;
        } 


    }
}
