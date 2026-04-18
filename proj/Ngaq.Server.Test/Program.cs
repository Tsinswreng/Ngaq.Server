using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core;
using Ngaq.Core.Infra.Url;
using Ngaq.Local.Di;
using Ngaq.Server;
using Ngaq.Server.Http;
using Ngaq.Server.Infra.Cfg;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Server.Test;

internal class Program{
	public static IServiceCollection SvcColct = new ServiceCollection();
	public static IServiceProvider SvcProvdr = null!;

	public static async Task Main(string[] args){
		BaseDirMgr.Inst._BaseDir = Directory.GetCurrentDirectory();

		var serverCfgPath = TestCfg.GetServerCfgPath(args);
		ServerCfg.Inst.LoadFromArgs([serverCfgPath]);

		SvcColct
			.AddLogging()
			.SetupCore()
			.SetupBiz()
			.SetupCommonBackend()
		;

		var mgr = ServerTestMgr.Inst;
		SvcProvdr = mgr.InitSvc(SvcColct, sc=>sc.BuildServiceProvider());

		await ServerTestBootstrap.EnsureServerDbReady(SvcProvdr, default);

		ITestExecutor executor = new TreeTestExecutor();
		await executor.RunEtPrint(mgr.TestNode);
	}
}
