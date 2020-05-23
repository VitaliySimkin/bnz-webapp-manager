using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WebAppManager.Model;

namespace WebAppManager.Controllers {

	[Route("webapp")]
	[ApiController]
	[Produces("application/json")]
	public class WebAppController : BaseController {

		public WebAppController(WebAppManager webAppManager) : base(webAppManager) { }

		/// <see cref="WebAppManager.GetWebApps(bool)" />
		[HttpGet(Name = nameof(Get))]
		public List<WebApp> Get(bool updateInCache = false) {
			return WebAppManager.GetWebApps(updateInCache);
		}

	}
}