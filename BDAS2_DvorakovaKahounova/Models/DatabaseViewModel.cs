namespace BDAS2_DvorakovaKahounova.Models
{
	public class DatabaseViewModel
	{
        public List<TableInfo> TablesAndColumns { get; set; }
        public string SelectedTable { get; set; }
        public Dictionary<string, List<Dictionary<string, string>>> TableContents { get; set; }

        public DatabaseViewModel()
        {
            TablesAndColumns = new List<TableInfo>();
            TableContents = new Dictionary<string, List<Dictionary<string, string>>>();
        }
    }
}
