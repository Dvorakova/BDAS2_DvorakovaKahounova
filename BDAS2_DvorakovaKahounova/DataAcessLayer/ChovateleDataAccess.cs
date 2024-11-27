using BDAS2_DvorakovaKahounova.Models;
using Oracle.ManagedDataAccess.Client;

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

                using (var cmd = new OracleCommand("SELECT id_psa, jmenoPsa, cislo_cipu, datumNarozeni, Pohlavi, Barva, Plemeno, Vlastnosti, KarantenaDo, DuvodPobytu, KrmnaDavka, Vaha, Majitel, Rezervace, Fotografie FROM ViewVypisVsechPsu", con))
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
								NAROZENI = reader.GetDateTime(3),
								POHLAVI = reader.GetInt32(4),
                                BARVA = reader.GetString(5),
                                PLEMENO = reader.GetString(6),
								VLASTNOSTI = reader.IsDBNull(7) ? null : reader.GetString(7),
								KARANTENA_DO = reader.GetDateTime(8),
                                DUVOD_POBYTU = reader.IsDBNull(9) ? null : reader.GetString(9),
                                KRMNA_DAVKA =  reader.GetDecimal(10),
                                VAHA = reader.GetDecimal(11),
								MAJITEL = reader.IsDBNull(12) ? null : reader.GetString(12),
								REZERVOVANO = reader.IsDBNull(13) ? null : reader.GetString(13),
								ID_FOTOGRAFIE = reader.IsDBNull(14) ? (int?)null : reader.GetInt32(14)
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
