using Microsoft.AspNetCore.Mvc;


namespace WebAppManager.Controllers {

	public class BaseController : Controller {

		protected WebAppManager WebAppManager { get; }

		public BaseController(WebAppManager webAppManager) {
			WebAppManager = webAppManager;
		}

	}

}