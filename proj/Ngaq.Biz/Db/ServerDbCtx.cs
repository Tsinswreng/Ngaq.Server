/*
pwd=Ngaq.Infra
dotnet ef migrations add Init --context ServerDbCtx
dotnet ef database update --context ServerDbCtx
 */

// using Ngaq.Core.Model.Auth;
// using Ngaq.Core.Model.IF;
//using Ngaq.Core.Model.Po.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Model.Po;
using Ngaq.Core.Model.Po.Role;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Model.Sys.Po.User;
using Ngaq.Core.Tools;
//using Ngaq.Core.Model.PoRole;


namespace Ngaq.Biz.Db;

public class ServerDbCtx
	:DbContext
{
	public DbSet<PoUser> User{get;set;}
	public DbSet<PoPassword> Password{get;set;}
	// public DbSet<Po_Profile> Profile{get;set;}
	// public DbSet<Po_Permission> Permission{get;set;}
	// public DbSet<Po_Role> Role{get;set;}
	// public DbSet<Po_Role_Permission> Role_Permission{get;set;}
	// public DbSet<Po_RefreshToken> RefreshToken{get;set;}


	protected void _CfgPoBase<T>(ModelBuilder mb) where T:IPoBase{

	}

	protected override void OnConfiguring(DbContextOptionsBuilder opt) {
		base.OnConfiguring(opt);
		var dbPath = Path.Combine(
			Directory.GetCurrentDirectory(),//TODO 用配置文件
			"..", "Ngaq_Server.sqlite"
		);
		opt.UseSqlite($"Data Source={dbPath}");

	}

	protected (
		Func<T, u8[]>
		,Func<u8[], T>
	)_ConvU128EtU8Arr<T>()
		where T: IIdUInt128, new()
	{
		var CodeToDb = (T id)=>{
			return ToolId.ToByteArr(id.Value);
		};
		var DbToCode = (u8[] id)=>{
			var UInt128 = ToolId.ByteArrToUInt128(id);
			return new T{Value = UInt128};
		};
		return (CodeToDb, DbToCode);
	}

	protected nil _ConvIdU128<TPo, TId>(
		EntityTypeBuilder<TPo> e
	)
		where TPo:class, I_Id<TId>
		where TId: IIdUInt128, new()
	{
		e.Property(e=>e.Id).HasConversion(
			code => ToolId.ToByteArr(code.Value)
			,db => new TId{Value = ToolId.ByteArrToUInt128(db)}
		);
		return Nil;
	}

	protected nil _CfgIdU128<TPo, TId>(
		EntityTypeBuilder<TPo> e
	)
		where TPo:class, I_Id<TId>
		where TId: IIdUInt128, new()
	{
		e.HasKey(e=>e.Id);
		_ConvIdU128<TPo, TId>(e);
		return Nil;
	}

	protected override void OnModelCreating(ModelBuilder mb) {
		base.OnModelCreating(mb);
		mb.Entity<PoUser>(e=>{
			//e.HasKey(e=>e.Id);
			// e.Property(e=>e.Id).HasConversion(
			// 	id=>id.Value
			// 	,val => new IdUser(val)
			// );
			_CfgIdU128<PoUser, IdUser>(e);
			e.HasIndex(e=>e.Email).IsUnique()
				.HasFilter("[Email] IS NOT NULL"); //?
			;
			//e.HasIndex(e=>e.PhoneNumber).IsUnique(); //PhoneNumber未填則不好辦

			//var (fn1, fn2) = _ConvU128EtU8Arr<IdRole>();
			e.Property(e=>e.RoleId)
				.HasConversion(
					id => id==null?null:ToolId.ToByteArr(id.Value.Value)
					,val => val==null?null:new IdRole(ToolId.ByteArrToUInt128(val))
				)
			;
		});

		mb.Entity<PoPassword>(e=>{
			// e.HasKey(e=>e.Id);
			// e.Property(e=>e.Id).HasConversion(
			// 	id=>id.Value
			// 	,val => new Id_Password(val)
			// );
			_CfgIdU128<PoPassword, IdPassword>(e);
			// e.Property(p=>p.Id)
			// 	.HasConversion(id=>id.Value,val=>new Id_Password(val))
			// ;
			e.HasOne<PoUser>()
				.WithMany()
				.HasForeignKey(e=>e.UserId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);
			;
		});

		// mb.Entity<Po_Profile>(e=>{
		// 	e.HasKey(e=>e.Id);
		// 	e.Property(e=>e.Id).HasConversion(
		// 		id=>id.Value
		// 		,val => new Id_Profile(val)
		// 	);
		// });


		// mb.Entity<Po_RefreshToken>(e=>{
		// 	e.HasKey(e=>e.Id);
		// 	e.Property(e=>e.Id).HasConversion(
		// 		id=>id.Value
		// 		,val => new Id_RefreshToken(val)
		// 	);

		// 	e.HasOne<PoUser>()
		// 		.WithMany()
		// 		.HasForeignKey(e=>e.UserId)
		// 		.OnDelete(DeleteBehavior.Cascade)
		// 	;
		// });

		mb.Entity<PoRole>(e=>{
			// e.HasKey(e=>e.Id);
			// e.Property(e=>e.Id).HasConversion(
			// 	id=>id.Value
			// 	,val => new Id_Role(val)
			// );
			_CfgIdU128<PoRole, IdRole>(e);
			e.HasIndex(e=>e.Key).IsUnique();
		});

		// mb.Entity<Po_Permission>(e=>{
		// 	e.HasKey(e=>e.Id);
		// 	e.Property(e=>e.Id).HasConversion(
		// 		id=>id.Value
		// 		,val => new Id_Permission(val)
		// 	);

		// 	e.HasIndex(e=>e.Name).IsUnique();
		// 	e.HasIndex(e=>e.Rsrc);
		// });

		// mb.Entity<Po_Role_Permission>(e=>{
		// 	e.HasKey(e=>e.Id);
		// 	e.Property(e=>e.Id).HasConversion(
		// 		id=>id.Value
		// 		,val => new Id_Role_Permission(val)
		// 	);

		// 	e.Property(x=>x.RoleId)
		// 		.HasConversion(
		// 			id => id.Value
		// 			,val => new Id_Role(val)
		// 		).IsRequired()
		// 	;

		// 	e.Property(x=>x.PermissionId)
		// 		.HasConversion(
		// 			id => id.Value
		// 			,val => new Id_Permission(val)
		// 		).IsRequired()
		// 	;

		// 	e.HasOne<Po_Role>()
		// 		.WithMany()
		// 		.HasForeignKey(e=>e.RoleId)
		// 		.OnDelete(DeleteBehavior.Cascade)//角色被刪旹,角色權限也要刪掉
		// 	;
		// 	e.HasOne<Po_Permission>()
		// 		.WithMany()
		// 		.HasForeignKey(e=>e.PermissionId)
		// 		.OnDelete(DeleteBehavior.Restrict)//權限刪除旹 限制操作
		// 	;

		// 	e.HasIndex(x=>x.RoleId)
		// 		.HasDatabaseName("IX_RolePermissions_RoleId")
		// 		.IncludeProperties(x=>x.PermissionId)//組合索引優化
		// 	;
		// });

	}
}
