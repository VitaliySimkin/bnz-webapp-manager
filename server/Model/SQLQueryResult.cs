using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebAppManager.Model {

	/// <summary> Результат виконання SQL запиту </summary>
	public class SQLQueryResult {

		/// <summary> Успішність виконання </summary>
		[Required]
		[DefaultValue(true)]
		public bool Success { get; set; } = true;

		/// <summary> Час виконання запиту (в мс) </summary>
		public int ExecuteTime { get; set; }

		/// <summary> Текст помилки </summary>
		public string ErrorMessage { get; set; }

		/// <summary> Стек помилки </summary>
		public string ErrorStack { get; set; }

		/// <summary> Запит </summary>
		public string Sql { get; set; }

		/// <summary> Колонки </summary>
		public List<string> Columns { get; set; }

		/// <summary> Рядки з даними </summary>
		public List<List<string>> Rows { get; set; }

		/// <summary> Кількість зачеплених записів </summary>
		public int RecordsAffected { get; set; }

		/// <summary> Тип бази даних </summary>
		public string DBType { get; set; }

	}

}
