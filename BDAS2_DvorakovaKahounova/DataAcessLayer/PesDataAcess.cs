using BDAS2_DvorakovaKahounova.Models;
using Oracle.ManagedDataAccess.Client;

namespace BDAS2_DvorakovaKahounova.DataAcessLayer
{
    public class PesDataAcess
    {
        private string _connectionString;

        public PesDataAcess(string configuration)
        {
            // Načtení connection string z appsettings.json
            _connectionString = configuration;
        }

        public List<Pes> GetAllPsi()
        {
            List<Pes> psi = new List<Pes>();

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("SELECT * FROM Psi", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Pes pes = new Pes
                            {
                                ID_PSA = reader.GetInt32(0),
                                JMENO = reader.GetString(1),
                                CISLO_CIPU = reader.GetString(2),
                                NAROZENI = reader.GetDateTime(3) 
                            };
                            psi.Add(pes);
                        }
                    }
                }
            }

            return psi;
        }
    }
}
