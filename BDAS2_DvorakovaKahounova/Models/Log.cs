namespace BDAS2_DvorakovaKahounova.Models
{
	public class Log
	{
		public int IdLog { get; set; }
		public string NazevTabulky { get; set; } //název tabulky, která byla změněna
		public string Akce { get; set; } // typ akce, která byla rpovedena
		public string PopisAkce { get; set; } // popis, co bylo změněno
		public DateTime Cas { get; set; } // čas změny
	}
}
