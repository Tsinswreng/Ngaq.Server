namespace Ngaq.Biz.Tools;

using System;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
public class ToolArgon{

	protected static ToolArgon? _inst = null;
	public static ToolArgon Inst => _inst??= new ToolArgon();

	public async Task<string> HashPasswordAsy(
		string password
		,CancellationToken ct
		,int iterations = 4
		,int memoryCost = 65536
		,int parallelism = 2
		,int digestSize = 32
	){
		// 生成随机盐（推荐16字节）
		byte[] salt = new byte[16];
		using (var rng = RandomNumberGenerator.Create()){
			rng.GetBytes(salt);
		}

		var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
		{
			Salt = salt,
			Iterations = iterations,
			MemorySize = memoryCost,
			DegreeOfParallelism = parallelism
		};

		byte[] hash = await argon2.GetBytesAsync(digestSize);
		// 将盐、参数和哈希值组合存储（格式：$argon2id$v=19$m=...,t=...,p=...$salt$hash）
		return $"$argon2id$v=19$m={memoryCost},t={iterations},p={parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
	}

	public async Task<bool> VerifyPasswordAsy(
		string password
		,string storedHash
		,CancellationToken ct
	){
		// 解析存储的哈希值
		var parts = storedHash.Split('$');
		if (parts.Length != 6 || parts[1] != "argon2id"){
			//TODO 參數傳反也會報此錯
			throw new ArgumentException("Invalid hash format");
		}

		var paramsPart = parts[3].Split(',');
		int memoryCost = int.Parse(paramsPart[0].Split('=')[1]);
		int iterations = int.Parse(paramsPart[1].Split('=')[1]);
		int parallelism = int.Parse(paramsPart[2].Split('=')[1]);

		byte[] salt = Convert.FromBase64String(parts[4]);
		byte[] expectedHash = Convert.FromBase64String(parts[5]);

		// 重新计算哈希
		var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
		{
			Salt = salt,
			Iterations = iterations,
			MemorySize = memoryCost,
			DegreeOfParallelism = parallelism
		};

		byte[] actualHash = await argon2.GetBytesAsync(expectedHash.Length);

		// 安全比较哈希值（防时序攻击）
		return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
	}

}
