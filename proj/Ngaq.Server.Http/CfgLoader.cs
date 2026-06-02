namespace Ngaq.Server.Http;

using Ngaq.Server.Infra.Cfg;
using Tsinswreng.CsCfg;
using Tsinswreng.CsCore;

public static class CfgLoader{
	public const str CmdMigrate = "migrate";

	public static bool IsMigrateCmd(string[] args){
		return args.Length > 0
			&& str.Equals(args[0], CmdMigrate, StringComparison.OrdinalIgnoreCase);
	}
	
	[Doc(@$"從{nameof(args)}[0] 讀 配置文件路徑;
	未設置 則從默認路徑讀取
	")]
	public static str GetCfgFilePathFromCliArgs(string[] args){
		var CfgFilePath = "";
		if(IsMigrateCmd(args)){
			CfgFilePath = args.Length > 1
				? args[1]
				: "";
		}else if(args.Length > 0){
			CfgFilePath = args[0];
		}else{
	#if DEBUG
			CfgFilePath = "Ngaq.Server.dev.jsonc";
	#else
			CfgFilePath = "Ngaq.Server.jsonc";
	#endif
		}
		return CfgFilePath;
	}

	[Doc(@$"{nameof(GetCfgFilePathFromCliArgs)}")]
	public static ICfgAccessor LoadFromArgs(
		this ServerCfg Cfg
		,str[] args
	){
		try{
			var CfgPath = GetCfgFilePathFromCliArgs(args);
			var CfgText = File.ReadAllText(CfgPath);
			Cfg.FromJson(CfgText);
			//AppCfg.Inst = AppCfgParser.Inst.FromYaml(GetCfgFilePath(args));
		}
		catch (System.Exception e){
			System.Console.Error.WriteLine("Failed to load config file: "+e);
			System.Console.WriteLine("----");
			System.Console.WriteLine();
		}
		return Cfg;
	}
}
