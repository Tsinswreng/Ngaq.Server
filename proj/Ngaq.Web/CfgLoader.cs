namespace Ngaq.Web;

using Ngaq.Biz.Infra.Cfg;
using Tsinswreng.CsCfg;


public static class CfgLoader{
	public static str GetCfgFilePathFromCliArgs(string[] args){
		var CfgFilePath = "";
		if(args.Length > 0){
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
