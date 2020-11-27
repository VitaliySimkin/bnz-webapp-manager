using System.ComponentModel.DataAnnotations;

namespace WebAppManager.Model {


	/// <summary> Дані про застосунок розгорнутий в IIS </summary>
	public class WebApp {

		/// <summary> Ідентифікатор сайту (генерується автоматично) </summary>
		[Required]
		public string Id { get; set; }

		/// <summary> Сайт на якому розташований застосунок </summary>
		[Required]
		public string Site { get; set; }

		/// <summary> URL застосунку </summary>
		[Required]
		public string Path { get; set; }

		/// <summary> Шлях до розташування застосунку на диску </summary>
		[Required]
		public string PhysicalPath { get; set; }

		/// <summary> Назва пулу застосунків </summary>
		[Required]
		public string ApplicationPoolName { get; set; }

		public string SQLDBType { get; set; }

		/// <summary> Рядок підключення до БД </summary>
		public string DBConnectionString { get; set; }

		/// <summary> Рядок підключення до Redis </summary>
		public string RedisConnectionString { get; set; }

	}

}
