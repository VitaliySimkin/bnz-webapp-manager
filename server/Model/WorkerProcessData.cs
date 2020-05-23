using System.ComponentModel.DataAnnotations;

namespace WebAppManager.Model {

	/// <summary> Дані про процеси IIS </summary>
	public class WorkerProcessData {

		/// <summary> Id </summary>
		[Required]
		public int ProcessId { get; set; }

		/// <summary> Стан </summary>
		[Required]
		public string State { get; set; }

		/// <summary> Використана оперативна пам'ять, KB </summary>
		public double RAM { get; set; }

		/// <summary> % процесорного часу </summary>
		public double CPU { get; set; }

	}

}
