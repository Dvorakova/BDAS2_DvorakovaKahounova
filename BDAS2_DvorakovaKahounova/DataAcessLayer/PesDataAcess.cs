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

        ////stara metoda - jen testovaci tabulka (uplne puvodni)
        //public List<Pes> GetAllPsi()
        //{
        //    List<Pes> psi = new List<Pes>();

        //    using (var con = new OracleConnection(_connectionString))
        //    {
        //        con.Open();
        //        using (var cmd = new OracleCommand("SELECT * FROM Psi", con))
        //        {
        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    Pes pes = new Pes
        //                    {
        //                        ID_PSA = reader.GetInt32(0),
        //                        JMENO = reader.GetString(1),
        //                        CISLO_CIPU = reader.GetString(2),
        //                        NAROZENI = reader.GetDateTime(3) 
        //                    };
        //                    psi.Add(pes);
        //                }
        //            }
        //        }
        //    }

        //    return psi;
        //}

        ////pridano ND - předělávka Psi k adopci pro nepřihlášeného uživatele
        //public List<Pes> GetAllPsi()
        //{
        //    List<Pes> psi = new List<Pes>();

        //    using (var con = new OracleConnection(_connectionString))
        //    {
        //        con.Open();
        //        // Dotaz na pohled
        //        using (var cmd = new OracleCommand("SELECT jmeno, datum_narozeni, barva, plemeno, vlastnosti FROM View_PsiVUtulkuBezMajitele", con))
        //        {
        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    Pes pes = new Pes
        //                    {
        //                        JMENO = reader.GetString(0),
        //                        NAROZENI = reader.GetDateTime(1),
        //                        BARVA = reader.GetString(2),
        //                        PLEMENO = reader.GetString(3),
        //                        VLASTNOSTI = reader.IsDBNull(4) ? null : reader.GetString(4)
        //                    };
        //                    psi.Add(pes);
        //                }
        //            }
        //        }
        //    }

        //    return psi;
        //}

        //pridano ND pro upravu i rpo prihlasene uživatele
        public List<Pes> GetAllPsi()
        {
            List<Pes> psi = new List<Pes>();

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                // Dotaz na pohled s novými sloupci
                using (var cmd = new OracleCommand("SELECT id_psa, jmeno, datum_narozeni, barva, plemeno, vlastnosti, karantena_do, rezervovano FROM View_PsiVUtulkuBezMajitele", con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Pes pes = new Pes
                            {
                                ID_PSA = reader.GetInt32(0),
                                JMENO = reader.GetString(1),
                                NAROZENI = reader.GetDateTime(2),
                                BARVA = reader.GetString(3),
                                PLEMENO = reader.GetString(4),
                                VLASTNOSTI = reader.IsDBNull(5) ? null : reader.GetString(5),
                                KARANTENA_DO = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6), // Přidání kontroly pro karanténa
                                REZERVOVANO = reader.IsDBNull(7) ? null : reader.GetString(7) // Přidání rezervace
                            };
                            psi.Add(pes);
                        }
                    }
                }
            }

            return psi;
        }



        //pridano KK
        //public List<Pes> GetPsiProOsobu(int osobaId)
        //{
        //    List<Pes> psi = new List<Pes>();

        //    using (var con = new OracleConnection(_connectionString))
        //    {
        //        con.Open();
        //        using (var cmd = new OracleCommand(
        //            "SELECT ID_PSA, JMENO, CISLO_CIPU, NAROZENI, ID_OSOBA FROM Psi WHERE ID_OSOBA = :osobaId", con))
        //        {
        //            cmd.Parameters.Add(new OracleParameter(":osobaId", osobaId));

        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    Pes pes = new Pes
        //                    {
        //                        ID_PSA = reader.GetInt32(0),
        //                        JMENO = reader.GetString(1),
        //                        CISLO_CIPU = reader.GetString(2),
        //                        NAROZENI = reader.GetDateTime(3),
        //                        ID_OSOBA = reader.GetInt32(4)
        //                    };
        //                    psi.Add(pes);
        //                }
        //            }
        //        }
        //    }

        //    return psi;
        //}

    }
}
