using BDAS2_DvorakovaKahounova.Models;
using Oracle.ManagedDataAccess.Client;

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

	}
}
