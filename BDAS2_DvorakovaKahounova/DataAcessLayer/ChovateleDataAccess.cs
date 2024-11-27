﻿using BDAS2_DvorakovaKahounova.Models;
using Oracle.ManagedDataAccess.Client;
using System.Data;

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

                using (var cmd = new OracleCommand("SELECT id_psa, jmenoPsa, cislo_cipu, datumNarozeni, Pohlavi, Barva, Plemeno, Vlastnosti, KarantenaDo, DuvodPobytu, KrmnaDavka, Vaha, Majitel, Rezervace, Fotografie, IdMajitel FROM ViewVypisVsechPsu", con))
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
								NAROZENI = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(3),
								POHLAVI = reader.GetInt32(4),
                                BARVA = reader.GetString(5),
                                PLEMENO = reader.GetString(6),
								VLASTNOSTI = reader.IsDBNull(7) ? null : reader.GetString(7),
								KARANTENA_DO = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                                DUVOD_POBYTU = reader.IsDBNull(9) ? null : reader.GetString(9),
                                KRMNA_DAVKA =  reader.GetDecimal(10),
                                VAHA = reader.GetDecimal(11),
								MAJITEL = reader.IsDBNull(12) ? null : reader.GetString(12),
								REZERVOVANO = reader.IsDBNull(13) ? null : reader.GetString(13),
								ID_FOTOGRAFIE = reader.IsDBNull(14) ? (int?)null : reader.GetInt32(14),
                                ID_MAJITEL = reader.IsDBNull(15) ? (int?)null : reader.GetInt32(15)
							};
                            psi.Add(pes);
                        }
                    }
                }
            }

            return psi;
        }

		//fotografie
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

		public void UpdateFotografie(Fotografie fotografie)
		{
			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var cmd = new OracleCommand(
					"UPDATE Fotografie SET nazev_souboru = :nazev, typ_souboru = :typ, pripona_souboru = :pripona, obsah_souboru = :obsah, datum_zmeny = :datum, zmeneno_id_osoba = :zmeneno_id_osoba WHERE id_fotografie = :id", con))
				{
					cmd.Parameters.Add(new OracleParameter("nazev", fotografie.nazev_souboru));
					cmd.Parameters.Add(new OracleParameter("typ", fotografie.typ_souboru));
					cmd.Parameters.Add(new OracleParameter("pripona", fotografie.pripona_souboru));
					cmd.Parameters.Add(new OracleParameter("obsah", fotografie.obsah_souboru));
					cmd.Parameters.Add(new OracleParameter("datum", DateTime.Now)); // datum_zmeny
					cmd.Parameters.Add(new OracleParameter("zmeneno_id_osoba",fotografie.nahrano_id_osoba)); // zmeneno_id_osoba
					cmd.Parameters.Add(new OracleParameter("id", fotografie.id_fotografie));

					cmd.ExecuteNonQuery();
				}
			}
		}


		public Fotografie GetFotografieByPesId(int pesId)
		{
			Fotografie fotografie = null;

			using (var con = new OracleConnection(_connectionString))
			{
				con.Open();
				using (var cmd = new OracleCommand(
					"SELECT id_fotografie, nazev_souboru, typ_souboru, obsah_souboru FROM Fotografie WHERE id_fotografie = (SELECT id_fotografie FROM Psi WHERE id_psa = :pesId)", con))
				{
					cmd.Parameters.Add(new OracleParameter("pesId", pesId));

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
