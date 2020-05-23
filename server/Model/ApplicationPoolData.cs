
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAppManager.Model {

	/// <summary> Дані про пул застосунків </summary>
	public class ApplicationPoolData {

		/// <summary> Назва </summary>
		[Required]
		public string Name { get; set; }

		/// <summary> Стан </summary>
		[Required]
		public string State { get; set; }

		/// <summary> Запущені процеси </summary>
		[Required]
		public List<WorkerProcessData> WorkerProcesses { get; set; }
	}
}
