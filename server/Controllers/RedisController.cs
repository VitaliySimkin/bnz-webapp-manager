using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceStack.Redis;
using WebAppManager.Model;

namespace WebAppManager.Controllers {

	[Route("redis")]
	[Produces("application/json")]
	[ApiController]
	public class RedisController : BaseController {

		public RedisController(WebAppManager webAppManager) : base(webAppManager) { }

		#region Methods: protected

		/// <summary> Створити клієнт Redis </summary>
		/// <param name="connectionString">рядок підключення</param>
		/// <returns>клієнт для роботи з базою redis</returns>
		protected IRedisClient CreateRedisClient(string connectionString) {
			var connectionStringBuilder = new DbConnectionStringBuilder() {
				ConnectionString = connectionString
			};
			string host = GetConnectionStringParameter(connectionStringBuilder, "host", "localhost");
			int port = GetConnectionStringParameter(connectionStringBuilder, "port", 6379);
			string service = string.Concat(host, ":", port);
			int db = GetConnectionStringParameter(connectionStringBuilder, "db", 0);
			return new BasicRedisClientManager(db, service).GetClient();
		}

		/// <summary> Отримати клієнт для підключення до бази redis застосунку </summary>
		/// <param name="id">id застосунку</param>
		/// <returns>клієнт</returns>
		protected IRedisClient GetWebAppRedisClient(string id) {
			WebApp webApp = WebAppManager.GetById(id);
			return CreateRedisClient(webApp.RedisConnectionString);
		}

		/// <summary> Отримати параметр з рядка підключення до Redis </summary>
		/// <typeparam name="T">тип параметру</typeparam>
		/// <param name="connectionString">рядок підключення</param>
		/// <param name="key">ключ параметра</param>
		/// <param name="defaultValue">значення по замовчуванню (якщо не вдалось знайти в рядку)</param>
		/// <returns>значення параметра</returns>
		protected T GetConnectionStringParameter<T>(DbConnectionStringBuilder connectionString,
					string key, T defaultValue) {
			if (!connectionString.ContainsKey(key)) {
				return defaultValue;
			}
			Type t = typeof(T);
			return (T)Convert.ChangeType(connectionString[key],
				t.Name == "Nullable`1" ? Nullable.GetUnderlyingType(t) : t);
		}

		#endregion

		#region Methods: public

		/// <summary> Отримати всі дані з БД </summary>
		/// <param name="webAppId">id застосунку</param>
		/// <returns>перелік всіх значень в БД</returns>
		[HttpGet("{webAppId}", Name = nameof(GetAll))]
		public KeyValuePair<string, string>[] GetAll(string webAppId) {
			using IRedisClient client = GetWebAppRedisClient(webAppId);
			return client.GetAll<string>(client.GetAllKeys()).ToArray();
		}

		/// <summary> Отримати значення за ключем </summary>
		/// <param name="webAppId">id застосунку</param>
		/// <param name="key">ключ</param>
		/// <returns>значення</returns>
		[HttpGet("{webAppId}/{key}", Name = nameof(GetByKey))]
		public string GetByKey(string webAppId, string key) {
			using IRedisClient client = GetWebAppRedisClient(webAppId);
			return client.Get<string>(key);
		}

		/// <summary> Встановити значення </summary>
		/// <param name="webAppId">id застосунку</param>
		/// <param name="key">ключ</param>
		/// <param name="value">значення</param>
		[HttpPost("{webAppId}/{key}", Name = nameof(Set))]
		public bool Set(string webAppId, string key, [FromBody]string value) {
			using IRedisClient client = GetWebAppRedisClient(webAppId);
			return client.Set(key, value);
		}

		/// <summary> Очистити базу redis </summary>
		/// <param name="webAppId">id застосунку</param>
		[HttpPost("{webAppId}/flush/db", Name = nameof(FlushDb))]
		public void FlushDb(string webAppId) {
			using IRedisClient client = GetWebAppRedisClient(webAppId);
			client.FlushDb();
		}

		#endregion

	}

}