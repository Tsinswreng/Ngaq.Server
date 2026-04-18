namespace Ngaq.Server.Test;

public static class TestCfg{
	public static string GetServerCfgPath(string[] args){
		if(args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])){
			return args[0];
		}
		return "Ngaq.Server.test.jsonc";
	}
}
