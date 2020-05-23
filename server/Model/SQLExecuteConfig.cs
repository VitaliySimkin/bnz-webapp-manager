using System.ComponentModel.DataAnnotations;

namespace WebAppManager.Model {

	/// <summary> Параметри для виконання запиту SQL </summary>
	public class SQLExecuteConfig {

		/// <summary> код запиту </summary>
		[Required]
		public string Sql { get; set; }

	}

}
