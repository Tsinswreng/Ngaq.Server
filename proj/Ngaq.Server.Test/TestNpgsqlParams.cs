namespace Ngaq.Server.Test;
using Ngaq.Biz.Db.TswG;

public class TestNpgsqlParams{
	public async Task Test(str[] args){
		var app = NgaqWeb.InitApp(args);
		//app.Services.GetRequiredService<ServerDb>();
		var Conn = ServerDb.Inst.DataSource.CreateConnection();
		Conn.Open();
		var cmd = Conn.CreateCommand();
		cmd.CommandText =
		"""
		SELECT "Id" FROM "__TsinswrengSchemaHistory"
		WHERE "Name" = @Name
		""";
		cmd.Parameters.Add("@Name", NpgsqlTypes.NpgsqlDbType.Text);
		cmd.Prepare();
		cmd.Parameters.AddWithValue("@Name", "Init");
		var reader = await cmd.ExecuteReaderAsync();//這行報錯 System.InvalidCastException: Parameter @Name must be set
		while(await reader.ReadAsync()){
			System.Console.WriteLine(reader.GetValue(0));
		}
	}
}
