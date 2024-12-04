namespace BDAS2_DvorakovaKahounova.DataAcessLayer
{
	public class HomeDataAccess
	{
		private string _connectionString;

		public HomeDataAccess(string configuration)
		{
			_connectionString = configuration;
		}

	}
}
