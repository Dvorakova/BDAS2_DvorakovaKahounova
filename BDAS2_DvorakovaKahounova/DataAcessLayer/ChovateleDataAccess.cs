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
    }
}
