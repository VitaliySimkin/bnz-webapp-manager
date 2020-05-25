using Microsoft.Web.Administration;
using System;

namespace Installer {
	class Program {
		static void Main(string[] args) {
			using ServerManager serverManager = new ServerManager();
			var appPool = serverManager.ApplicationPools["server"];
			if (appPool == null) CreateServerAppPool(serverManager);
		}

		static void CreateServerAppPool(ServerManager serverManager) {
			throw new NotImplementedException();
		}
	}
}
