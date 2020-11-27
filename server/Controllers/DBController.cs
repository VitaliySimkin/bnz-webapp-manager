using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using WebAppManager.Model;

namespace WebAppManager.Controllers {

	[Route("sql")]
	[Produces("application/json")]
	[ApiController]
	public class DBController : BaseController {

		public DBController(WebAppManager webAppManager) : base(webAppManager) { }

		#region Methods: static protected

		/// <summary> Виконати SQL </summary>
		/// <param name="connectionString">рядок підключення до БД</param>
		/// <param name="sql">запит</param>
		/// <returns>результат виконання</returns>
		protected SQLQueryResult ExecuteSQL(string connectionString, string sql) {
			using var connection = GetDbConnection(connectionString);
			return ExecuteSQL(connection, sql);
		}

		/// <summary> Отримати підключення до БД </summary>
		/// <param name="connectionString">рядок підключення до БД</param>
		protected IDbConnection GetDbConnection(string connectionString) {
			switch (GetDBConnectionType(connectionString)) {
				case SQLDBType.MSSQL:
					connectionString = connectionString.Replace("; Async = true", "");
					var connection = new SqlConnection(connectionString);
					//connection.InfoMessage()
					return connection;
				case SQLDBType.Oracle:
					return new OracleConnection(connectionString);
				case SQLDBType.PostgreSQL:
					return new Npgsql.NpgsqlConnection(connectionString);
				default:
					throw new Exception("Unknown db type");
			}
		}

		/// <summary> Отримати тип БД </summary>
		/// <param name="connectionString">рядок підключення до БД</param>
		/// <returns>тип БД</returns>
		public static SQLDBType GetDBConnectionType(string connectionString) {
			if (connectionString.Contains("(SERVICE_NAME")) {
				return SQLDBType.Oracle;
			} else if (connectionString.ToUpper().Contains("INITIAL CATALOG")) {
				return SQLDBType.MSSQL;
			} else {
				return SQLDBType.PostgreSQL;
			}
		}

		/// <summary> Виконати SQL </summary>
		/// <param name="dbConnection">підключення до БД</param>
		/// <param name="sql">запит</param>
		/// <returns>результат виконання</returns>
		protected static SQLQueryResult ExecuteSQL(IDbConnection dbConnection, string sql) {
			var result = new SQLQueryResult { Sql = sql };
			dbConnection.Open();

			var watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			using (IDbCommand sqlCommand = dbConnection.CreateCommand()) {
				sqlCommand.CommandText = sql;
				ReadToResult(sqlCommand.ExecuteReader(), ref result);
			}
			watch.Stop();
			result.ExecuteTime = Convert.ToInt32(watch.ElapsedMilliseconds);
			dbConnection.Close();
			return result;
		}

		/// <summary> Вичитати дані в результат виконання запиту </summary>
		/// <param name="reader">читач результатів виконання запиту</param>
		/// <param name="result">результат виконання запиту</param>
		protected static void ReadToResult(IDataReader reader, ref SQLQueryResult result) {
			DataTable dataTable = new DataTable();
			dataTable.Load(reader);

			result.Columns = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();
			result.Rows = dataTable.Rows.Cast<DataRow>().Select(ConvertDataRowToList).ToList();
			result.RecordsAffected = reader.RecordsAffected;
		}

		/// <summary> Конвертувати рядок в список рядків </summary>
		/// <param name="row">рядок</param>
		protected static List<string> ConvertDataRowToList(DataRow row) {
			return row.ItemArray.Select(item => item == DBNull.Value ? "NULL" : item.ToString()).ToList();
		}

		#endregion

		#region Methods: public

		/// <summary> Виконати запит SQL до бази застосунку </summary>
		/// <param name="webAppId">id застосунку</param>
		/// <param name="config">параметри виконання</param>
		/// <returns>результат</returns>
		[HttpPost("{webAppId}/execute", Name = nameof(Execute))]
		public SQLQueryResult Execute(string webAppId, [FromBody]SQLExecuteConfig config) {
			try {
				WebApp webApp = WebAppManager.GetById(webAppId);
				string connectionStr = webApp.DBConnectionString;
				return ExecuteSQL(connectionStr, config.Sql);
			} catch (Exception ex) {
				return new SQLQueryResult {
					Success = false,
					Sql = config.Sql,
					ErrorMessage = ex.Message,
					ErrorStack = ex.StackTrace
				};
			}
		}

		#endregion

	}

}
