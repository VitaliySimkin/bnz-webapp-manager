using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WebAppManager.Model;

namespace WebAppManager.Controllers {

	/// <summary> API для роботи з пулами застосунків </summary>
	[Route("applicationpool")]
	[Produces("application/json")]
	[ApiController]
	public class ApplicationPoolController : BaseController {

		/// <summary> Create instance of <see cref="ApplicationPoolController"/> </summary>
		/// <param name="webAppManager">менеджер</param>
		public ApplicationPoolController(WebAppManager webAppManager) : base(webAppManager) { }

		#region Methods: protected

		/// <summary> Отримати дані про пул застосунків </summary>
		/// <param name="applicationPool">пул застосунків</param>
		protected ApplicationPoolData GetApplicationPoolData(ApplicationPool applicationPool) {
			var workerProcesses = applicationPool.WorkerProcesses.Select(GetWorkerProcessData).ToList();
			return new ApplicationPoolData {
				Name = applicationPool.Name,
				State = applicationPool.State.ToString(),
				WorkerProcesses = workerProcesses
			};
		}

		/// <summary> Отримати дані про процес </summary>
		/// <param name="workerProcess">процес</param>
		protected WorkerProcessData GetWorkerProcessData(WorkerProcess workerProcess) {
			var (ram, cpu) = GetProcessRamCpuUsage(workerProcess.ProcessId);
			return new WorkerProcessData {
				ProcessId = workerProcess.ProcessId,
				State = workerProcess.State.ToString(),
				RAM = ram,
				CPU = cpu
			};
		}

		/// <summary> Отримати дані про використання ОЗУ та ЦПУ </summary>
		/// <param name="processId">id процеса</param>
		protected (double ram, double cpu) GetProcessRamCpuUsage(int processId) {
			Process p = Process.GetProcessById(processId);
			double ram, cpu;
			using (var perf = new PerformanceCounter("Process", "Working Set", p.ProcessName)) ram = perf.NextValue();
			using (var perf = new PerformanceCounter("Process", "% Processor Time", p.ProcessName)) cpu = perf.NextValue();
			return (ram / 1024, cpu);
		}

		#endregion

		#region Methods: public

		/// <summary> Отримати дані про пул застосунків </summary>
		[HttpGet(Name = nameof(GetApplicationPools))]
		public List<ApplicationPoolData> GetApplicationPools() {
			using ServerManager serverManager = new ServerManager();
			return serverManager.ApplicationPools.Select(GetApplicationPoolData).ToList();
		}

		/// <summary> Запустити пул застосунків </summary>
		/// <param name="name">назва</param>
		[HttpPost("{name}/start", Name = nameof(Start))]
		public IActionResult Start(string name) {
			using ServerManager serverManager = new ServerManager();
			serverManager.ApplicationPools[name].Start();
			return Ok();
		}

		/// <summary> Зупинити пул застосунків </summary>
		/// <param name="name">назва</param>
		[HttpPost("{name}/stop", Name = nameof(Stop))]
		public IActionResult Stop(string name) {
			using ServerManager serverManager = new ServerManager();
			serverManager.ApplicationPools[name].Stop();
			return Ok();
		}

		/// <summary> Перезапустити пул застосунків </summary>
		/// <param name="name">назва</param>
		[HttpPost("{name}/recycle", Name = nameof(Recycle))]
		public IActionResult Recycle(string name) {
			using ServerManager serverManager = new ServerManager();
			serverManager.ApplicationPools[name].Recycle();
			return Ok();
		}

		#endregion

	}

}
