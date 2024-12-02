namespace BDAS2_DvorakovaKahounova.DataAcessLayer
{
    public class AdminDataAccess
    {
        private string _connectionString;

        public AdminDataAccess(string configuration)
        {
            _connectionString = configuration;
        }
    }
}
