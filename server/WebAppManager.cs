using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using WebAppManager.Model;

namespace WebAppManager {

	public class WebAppManager {

		#region Properties: protected

		protected IConfiguration Configuration { get; }

		protected MemoryCache MemoryCache { get; }

		/// <summary> Ключ для зберігання даних в кеші </summary>
		protected const string CacheKey = "WebApps";

		/// <summary> Об'єкт для  </summary>
		protected object LockObj = new object();

		#endregion

		#region Constructors: public

		public WebAppManager(IConfiguration configuration) {
			Configuration = configuration;
			MemoryCache = new MemoryCache(new MemoryCacheOptions() {
				SizeLimit = 512
			});
		}

		#endregion

		#region Methods: protected

		/// <summary> Отримати час зберігання даних про застосунки в кеші </summary>
		protected TimeSpan GetCacheStorageTime() {
			var config = Configuration.GetSection("Config");
			int seconds = config.GetValue("WebAppsCacheStorageTime", 300);
			return TimeSpan.FromSeconds(seconds);
		}

		/// <summary> Фабрика для створення запису в кеші з даними про застосунки </summary>
		/// <param name="cacheEntry">запис в кеші</param>
		/// <returns>дані про застосунки</returns>
		protected List<WebApp> CacheWebAppsFactory(ICacheEntry cacheEntry) {
			cacheEntry.AbsoluteExpirationRelativeToNow = GetCacheStorageTime();
			cacheEntry.Size = 1;
			return GetWebAppsFromIIS();
		}

		/// <summary> Отримати дані про застосунки з IIS </summary>
		/// <returns>перелік застосунків Creatio/Bpmonline розгорнутих в IIS</returns>
		protected List<WebApp> GetWebAppsFromIIS() {
			using ServerManager serverManager = new ServerManager();
			return serverManager.Sites
				.SelectMany(GetSiteWebApps)
				.Where(IsCreatioWebApp)
				.ToList();
		}

		/// <summary> Перевірити що застосунок є застосунком Creatio (зовнішній застосунок) </summary>
		/// <param name="webApp">застосунок</param>
		/// <returns><see langword="true"/>застосунок Creatio</returns>
		protected bool IsCreatioWebApp(WebApp webApp) {
			return !string.IsNullOrEmpty(webApp.DBConnectionString);
		}

		/// <summary> Отримати перелік застосунків розгорнутих на сайті </summary>
		/// <param name="site">сайт</param>
		/// <returns>застосунки на сайті</returns>
		protected IEnumerable<WebApp> GetSiteWebApps(Site site) {
			return site.Applications.Select(application => ConvertToWebApp(application, site));
		}

		/// <summary> Отримати тип БД </summary>
		/// <param name="connectionString">рядок підключення до БД</param>
		/// <returns>тип БД</returns>
		public virtual SQLDBType GetDBConnectionType(string connectionString) {
			if (string.IsNullOrEmpty(connectionString)) {
				return SQLDBType.Unknown;
			}
			if(connectionString.Contains("(SERVICE_NAME")) {
				return SQLDBType.Oracle;
			} else if(connectionString.ToUpper().Contains("INITIAL CATALOG")) {
				return SQLDBType.MSSQL;
			} else {
				return SQLDBType.PostgreSQL;
			}
		}

		/// <summary> Конвертувати дані про застосунок </summary>
		/// <param name="application">застосунок IIS</param>
		/// <param name="site">сайт IIS</param>
		/// <returns>дані про застосунок для подальшої роботи з ними</returns>
		protected WebApp ConvertToWebApp(Application application, Site site) {
			WebApp webApp = new WebApp {
				PhysicalPath = application.VirtualDirectories["/"].PhysicalPath,
				ApplicationPoolName = application.ApplicationPoolName,
				Path = application.Path,
				Site = site.Name
			};
			LoadConnectionStringsInfo(ref webApp);
			webApp.SQLDBType = GetDBConnectionType(webApp.DBConnectionString).ToString();
			webApp.Id = GetWebAppHash(webApp);
			return webApp;
		}

		/// <summary> Отримати hash даних про застосунок. Використовується для створення ідентифікатора </summary>
		/// <param name="webApp">застосунок</param>
		/// <returns>унікальний ідентифікатор сайту</returns>
		protected string GetWebAppHash(WebApp webApp) {
			string str = webApp.Site + webApp.Path;
			unchecked {
				int hash1 = 5381;
				int hash2 = hash1;
				for (int i = 0; i < str.Length && str[i] != '\0'; i += 2) {
					hash1 = ((hash1 << 5) + hash1) ^ str[i];
					if (i == str.Length - 1 || str[i + 1] == '\0')
						break;
					hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
				}

				return (hash1 + (hash2 * 1566083941)).ToString("X");
			}
		}

		/// <summary> Завантажити дані про рядки підключення до БД та Redis </summary>
		/// <param name="webApp">дані про застосунок</param>
		protected void LoadConnectionStringsInfo(ref WebApp webApp) {
			string connectionStringFilePath = Path.Combine(webApp.PhysicalPath, "ConnectionStrings.config");
			if (!File.Exists(connectionStringFilePath)) {
				return;
			}
			try {
				XmlDocument document = new XmlDocument();
				document.Load(connectionStringFilePath);
				webApp.DBConnectionString = document.DocumentElement
					.SelectSingleNode("add[@name='db']").Attributes["connectionString"].Value;
				webApp.RedisConnectionString = document.DocumentElement
					.SelectSingleNode("add[@name='redis']").Attributes["connectionString"].Value;
			} catch { }
		}

		#endregion

		#region Methods: public

		/// <summary> Отримати дані про застосунок по Id </summary>
		/// <param name="id">id</param>
		public WebApp GetById(string id) {
			var webApp = GetWebApps().Find(webApp => webApp.Id == id);
			if (webApp == null) {
				throw new KeyNotFoundException($"WebApp with Id = {id} not found");
			}
			return webApp;
		}

		/// <summary> Отримати дані про застосунки.
		/// Використовується кеш для того щоб не читати дані ConnectionString кожен раз
		/// </summary>
		/// <param name="updateInCache">оновити дані в кеші. Якщо false - значення буде оновлене в кеші </param>
		public List<WebApp> GetWebApps(bool updateInCache = false) {
			if (!updateInCache && MemoryCache.TryGetValue(CacheKey, out List<WebApp> webApps)) {
				return webApps;
			}
			lock (LockObj) {
				if (updateInCache) {
					MemoryCache.Remove(CacheKey);
				}
				return MemoryCache.GetOrCreate(CacheKey, CacheWebAppsFactory);
			}
		}

		#endregion

	}

}
