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

                using (var cmd = new OracleCommand("SELECT id_psa, jmeno, cislo_cipu, datum_narozeni, id_pohlavi, barva, plemeno, vlastnosti, karantena_do, duvod_pobytu, krmna_davka, vaha, majitel, rezervace, fotografie FROM ViewVypisVsechPsu", con))
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
                                NAROZENI = reader.GetDateTime(3),
                                //ID_POHLAVI = reader.GetInt32(4),
                                BARVA = reader.GetString(5),
                                PLEMENO = reader.GetString(6),
                                VLASTNOSTI = reader.IsDBNull(7) ? null : reader.GetString(7),
                                KARANTENA_DO = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                                DUVOD_POBYTU = reader.IsDBNull(9) ? null : reader.GetString(9),
                                KRMNA_DAVKA = reader.IsDBNull(10) ? null : reader.GetString(10),
                                VAHA = reader.GetDecimal(11),
                                MAJITEL = reader.IsDBNull(12) ? null : reader.GetString(13),
                                REZERVOVANO = reader.IsDBNull(14) ? null : reader.GetString(14),
                                ID_FOTOGRAFIE = reader.IsDBNull(15) ? (int?)null : reader.GetInt32(15)
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
