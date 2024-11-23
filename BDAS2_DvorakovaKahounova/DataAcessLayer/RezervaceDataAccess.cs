using Oracle.ManagedDataAccess.Client;

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

    }
}
