﻿using BDAS2_DvorakovaKahounova.Models;
using Oracle.ManagedDataAccess.Client;
using System.Data;

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
        //metoda pro stránku Psi k adopci
        public List<Pes> GetAllPsi()
        {
            List<Pes> psi = new List<Pes>();

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                // Dotaz na pohled s novými sloupci
                using (var cmd = new OracleCommand("SELECT id_psa, jmeno, datum_narozeni, pohlavi, barva, plemeno, vlastnosti, karantena_do, rezervovano FROM View_PsiVUtulkuBezMajitele", con))
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
                                POHLAVI = reader.GetString(3),
                                BARVA = reader.GetString(4),
                                PLEMENO = reader.GetString(5),
                                VLASTNOSTI = reader.IsDBNull(6) ? null : reader.GetString(6),
                                KARANTENA_DO = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7), // Přidání kontroly pro karanténa
                                REZERVOVANO = reader.IsDBNull(8) ? null : reader.GetString(8) // Přidání rezervace
                            };
                            psi.Add(pes);
                        }
                    }
                }
            }

            return psi;
        }



        //pridano KK
        public List<Pes> GetPsiProOsobu(int osobaId)
        {
            List<Pes> psi = new List<Pes>();

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand("GetPsiProOsobu", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter("osobaId", osobaId));

                    cmd.Parameters.Add(new OracleParameter("cur", OracleDbType.RefCursor, ParameterDirection.Output));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Pes pes = new Pes
                            {
                                ID_PSA = reader.GetInt32(0),
                                JMENO = reader.GetString(1),
                                NAROZENI = reader.GetDateTime(2),
                                POHLAVI = reader.GetString(3),
                                PLEMENO = reader.GetString(4),
                                BARVA = reader.GetString(5),
                                VLASTNOSTI = reader.GetString(6),
                                CISLO_CIPU = reader.GetString(7),
                                ID_FOTOGRAFIE = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8) // Ošetření null hodnot
                            };
                            psi.Add(pes);
                        }
                    }
                }
            }

            return psi;
        }

        //pridano ND pro uložení fotografie
        public int SaveFotografie(Fotografie fotografie)
        {
            int newId = 0;

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand(
                    @"INSERT INTO Fotografie (nazev_souboru, typ_souboru, pripona_souboru, datum_nahrani, nahrano_id_osoba, obsah_souboru) 
              VALUES (:nazev, :typ, :pripona, :datum, :osobaId, :obsah)
              RETURNING id_fotografie INTO :newId", con))
                {
                    cmd.Parameters.Add(new OracleParameter("nazev", fotografie.nazev_souboru));
                    cmd.Parameters.Add(new OracleParameter("typ", fotografie.typ_souboru));
                    cmd.Parameters.Add(new OracleParameter("pripona", fotografie.pripona_souboru));
                    cmd.Parameters.Add(new OracleParameter("datum", fotografie.datum_nahrani));
                    cmd.Parameters.Add(new OracleParameter("osobaId", fotografie.nahrano_id_osoba));
                    cmd.Parameters.Add(new OracleParameter("obsah", fotografie.obsah_souboru));
                    cmd.Parameters.Add(new OracleParameter("newId", OracleDbType.Int32, ParameterDirection.Output));

                    cmd.ExecuteNonQuery();
                    newId = Convert.ToInt32(cmd.Parameters["newId"].Value.ToString());
                }
            }

            return newId;
        }

        public void UpdatePesFotografie(int pesId, int fotografieId)
        {
            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand(
                    "UPDATE Psi SET ID_FOTOGRAFIE = :fotografieId WHERE ID_PSA = :pesId", con))
                {
                    cmd.Parameters.Add(new OracleParameter("fotografieId", fotografieId));
                    cmd.Parameters.Add(new OracleParameter("pesId", pesId));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Fotografie GetFotografieById(int id)
        {
            Fotografie fotografie = null;

            using (var con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new OracleCommand(
                    "SELECT id_fotografie, nazev_souboru, typ_souboru, obsah_souboru FROM Fotografie WHERE id_fotografie = :id", con))
                {
                    cmd.Parameters.Add(new OracleParameter("id", id));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fotografie = new Fotografie
                            {
                                id_fotografie = reader.GetInt32(0),
                                nazev_souboru = reader.GetString(1),
                                typ_souboru = reader.GetString(2),
                                obsah_souboru = reader["obsah_souboru"] as byte[]
                            };
                        }
                    }
                }
            }

            return fotografie;
        }


    }
}
