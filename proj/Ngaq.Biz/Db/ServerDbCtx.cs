/*
pwd=Ngaq.Biz
dotnet ef migrations add Init --context ServerDbCtx
dotnet ef database update --context ServerDbCtx
 */

// using Ngaq.Core.Model.Auth;
// using Ngaq.Core.Model.IF;
//using Ngaq.Core.Model.Po.Auth;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ngaq.Biz.Infra.Cfg;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Model.Po;
using Ngaq.Core.Model.Po.Role;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Models.Po;
using Ngaq.Core.Models.Sys.Po.Password;
using Ngaq.Core.Models.Sys.Po.Role;
using Ngaq.Core.Tools;
using Tsinswreng.CsCfg;
using Tsinswreng.CsTools;
using IdTool = Tsinswreng.CsTools.ToolUInt128;
//using Ngaq.Core.Model.PoRole;


namespace Ngaq.Biz.Db;

public  partial class ServerDbCtx
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
		// var dbPath = Path.Combine(
		// 	Directory.GetCurrentDirectory(),
		// 	"..", "Ngaq_Server.sqlite"
		// );
		var dbPath = ItemsServerCfg.SqliteDbPath.GetFrom(ServerCfg.Inst);
		opt.UseSqlite($"Data Source={dbPath}");
	}

	protected (
		Func<T, u8[]>
		,Func<u8[], T>
	)_ConvU128EtU8Arr<T>()
		where T: IIdUInt128, new()
	{
		var CodeToDb = (T id)=>{
			return IdTool.ToByteArr(id.Value);
		};
		var DbToCode = (u8[] id)=>{
			var UInt128 = IdTool.ByteArrToUInt128(id);
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
			code => IdTool.ToByteArr(code.Value)
			,db => new TId{Value = IdTool.ByteArrToUInt128(db)}
		);
		return NIL;
	}

	protected nil _CfgIdU128<TPo, TId>(
		EntityTypeBuilder<TPo> e
	)
		where TPo:class, I_Id<TId>
		where TId: IIdUInt128, new()
	{
		e.HasKey(e=>e.Id);
		_ConvIdU128<TPo, TId>(e);
		return NIL;
	}

	protected nil CfgPoBase<TEntity>(EntityTypeBuilder<TEntity> e)
		where TEntity: class, IPoBase
	{
		e.Property(e=>e.DbCreatedAt).HasConversion<TempusConverter>();
		e.Property(e=>e.CreatedAt).HasConversion<TempusConverter>();
		e.Property(e=>e.DbUpdatedAt).HasConversion<TempusConverter>();
		e.Property(e=>e.UpdatedAt).HasConversion<TempusConverter>();

		e.Property(e=>e.CreatedBy).HasConversion<IdUserConverter>();
		e.Property(e=>e.LastUpdatedBy).HasConversion<IdUserConverter>();

		// e.Property(e=>e.Status).HasConversion<i32>(
		// 	PoStatus => PoStatus.Value
		// 	,Raw => PoStatus.Parse(Raw)
		// );

		return NIL;
	}

	protected override void OnModelCreating(ModelBuilder mb) {
		base.OnModelCreating(mb);
		mb.Entity<PoUser>(e=>{
			CfgPoBase(e);
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

		});

		mb.Entity<PoPassword>(e=>{
			CfgPoBase(e);
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
			CfgPoBase(e);
			// e.HasKey(e=>e.Id);
			// e.Property(e=>e.Id).HasConversion(
			// 	id=>id.Value
			// 	,val => new Id_Role(val)
			// );
			_CfgIdU128<PoRole, IdRole>(e);
			e.HasIndex(e=>e.Code).IsUnique();
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

//Tempus類型映射(不效)
		// foreach (var entityType in mb.Model.GetEntityTypes()){
		// 	foreach (var property in entityType.GetProperties()){
		// 		if (property.ClrType == typeof(Tempus)
		// 			|| property.ClrType == typeof(Tempus?)
		// 		){
		// 			property.SetValueConverter(
		// 				new ValueConverter<Tempus, i64>(
		// 					v => v,   // Tempus 转 long
		// 					v => v // long 转 Tempus
		// 				)
		// 			);
		// 		}
		// 	}
		// }

	}
}


public partial class TempusConverter : ValueConverter<Tempus, i64>{
	public TempusConverter(): base(
			v => v
			,v => v
		)
	{}
}

public partial class IdUserConverter : ValueConverter<IdUser, u8[]>{
	public IdUserConverter(): base(
			v => v.Value.ToByteArr()
			,v => IdUser.FromByteArr(v)
		)
	{}
}








#if false
namespace Ngaq.Local.Db;

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Ngaq.Core.Infra;
using Ngaq.Core.Model.Po;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Model.Po.Word;
using Ngaq.Core.Models.Po;
using Tsinswreng.CsUlid;
using ToolId = Tsinswreng.CsUlid.IdTool;


/*
// dotnet ef migrations add Init --project ./Ngaq.Local
// dotnet ef database update --project ./Ngaq.Local

// #dotnet ef dbcontext optimize --output-dir GeneratedInterceptors # --namespace YourProject.GeneratedInterceptors
// dotnet ef dbcontext optimize --output-dir GeneratedInterceptors --precompile-queries --nativeaot

 */

public  partial class LocalDbCtx : DbContext{

	public DbSet<PoWord> PoWord{get;set;}
	public DbSet<PoWordProp> PoKv{get;set;}
	public DbSet<PoWordLearn> PoLearn{get;set;}
	public str DbPath{get;set;} = AppCfg.Inst.SqlitePath;

	protected override void OnConfiguring(DbContextOptionsBuilder opt) {
		base.OnConfiguring(opt);
		// var dbPath = Path.Combine(
		// 	Directory.GetCurrentDirectory(),
		// 	"..", "Ngaq.sqlite"
		// );
		//var dbPath = DbPath;
		var dbPath = "E:/_code/CsNgaq/Ngaq.Sqlite";
		opt.UseSqlite($"Data Source={dbPath}");
	}

	protected nil _CfgPoBase<
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.PublicConstructors |
			DynamicallyAccessedMemberTypes.NonPublicConstructors |
			DynamicallyAccessedMemberTypes.PublicFields |
			DynamicallyAccessedMemberTypes.NonPublicFields |
			DynamicallyAccessedMemberTypes.PublicProperties |
			DynamicallyAccessedMemberTypes.NonPublicProperties |
			DynamicallyAccessedMemberTypes.Interfaces
		)]
		T
	>(ModelBuilder mb) where T:class, IPoBase{
		mb.Entity<T>(e=>{

			e.Property(p=>p.CreatedAt).HasConversion(
				tempus=>tempus.Value,
				val => new Tempus(val)
			);
			e.Property(p=>p.DbCreatedAt).HasConversion(
				tempus=>tempus.Value,
				val => new Tempus(val)
			);

			e.Property(p=>p.UpdatedAt).HasConversion<long?>(
				tempus=>tempus==null?null:tempus.Value.Value,
				val => val==null?null:new Tempus(val.Value)
			);

			e.Property(p=>p.DbUpdatedAt).HasConversion<long?>(
				tempus=>tempus==null?null:tempus.Value.Value,
				val => val==null?null:new Tempus(val.Value)
			);

			e.Property(p=>p.Status).HasConversion(
				status=>status.Value,
				val => new PoStatus(val)
			);


			e.Property(p=>p.CreatedBy).HasConversion(
				id=>id==null?null:id.Value.Value.ToByteArr()
				,val => val==null?null:new IdUser(ToolId.ByteArrToUInt128(val))
			);
			e.HasIndex(p=>p.CreatedBy);

			e.Property(p=>p.LastUpdatedBy).HasConversion(
				id=>id==null?null:id.Value.Value.ToByteArr()
				,val => val==null?null:new IdUser(ToolId.ByteArrToUInt128(val))
			);
			e.HasIndex(p=>p.LastUpdatedBy);

		});
		return NIL;
	}

	protected nil _CfgI_WordId<
		[DynamicallyAccessedMembers(
			DynamicallyAccessedMemberTypes.PublicConstructors |
			DynamicallyAccessedMemberTypes.NonPublicConstructors |
			DynamicallyAccessedMemberTypes.PublicFields |
			DynamicallyAccessedMemberTypes.NonPublicFields |
			DynamicallyAccessedMemberTypes.PublicProperties |
			DynamicallyAccessedMemberTypes.NonPublicProperties |
			DynamicallyAccessedMemberTypes.Interfaces
		)]
		T
	>(ModelBuilder mb) where T:class, I_WordId{
		mb.Entity<T>(e=>{
			e.HasIndex(p=> p.WordId);
			e.Property(p => p.WordId).HasConversion(
				id=> id.Value.ToByteArr()
				,val => IdWord.FromByteArr(val)
			).HasColumnType("BLOB");
		});
		return NIL;
	}

	protected override void OnModelCreating(ModelBuilder mb) {
		base.OnModelCreating(mb);
		_CfgPoBase<PoWord>(mb);
		mb.Entity<PoWord>(e=>{
			e.ToTable("Word");
			e.HasKey(p=>p.Id);
			e.Property(p=>p.Id).HasConversion(
				id=>id.Value.ToByteArr()
				,val => new IdWord(ToolId.ByteArrToUInt128(val))
			).HasColumnType("BLOB");
			e.Property(p=>p.Owner).HasConversion(
				id=>id.Value.ToByteArr()
				,val => new IdUser(ToolId.ByteArrToUInt128(val))
			).HasColumnType("BLOB");
			e.HasIndex(p=>p.Head);
			e.HasIndex(p => new {p.Head, p.Lang, p.Owner}).IsUnique();
			//Unique(WordFormId, Lang):
		});

		_CfgPoBase<PoWordProp>(mb);
		_CfgI_WordId<PoWordProp>(mb);
		mb.Entity<PoWordProp>(e=>{
			e.ToTable("Prop").UseTpcMappingStrategy();
			e.HasKey(p=>p.Id);
			e.Property(p=>p.Id).HasConversion(
				id=>id.Value.ToByteArr()
				,val => new IdWordProp(ToolId.ByteArrToUInt128(val))
			).HasColumnType("BLOB");
			//e.Ignore(p=>p.FKeyUInt128);

			// e.HasIndex(p=>p.FKeyUInt128);
			// e.Property(p=>p.FKeyUInt128).HasConversion(
			// 	id=>id==null?null:id.Value.ToByteArr()
			// 	,val => val==null?null:ToolId.ByteArrToUInt128(val)
			// ).HasColumnType("BLOB");
			e.HasIndex(p=>p.KStr);
			e.HasIndex(p=>p.KI64);
		});

		_CfgPoBase<PoWordLearn>(mb);
		_CfgI_WordId<PoWordLearn>(mb);
		mb.Entity<PoWordLearn>((e=>{
			e.ToTable("Learn").UseTpcMappingStrategy();
			//e.HasKey(p=>p.Id);
			e.Property(p => p.Id).HasConversion(
				id=> id.Value.ToByteArr()
				,val => IdLearn.FromByteArr(val)
			).HasColumnType("BLOB");

			e.HasIndex((p=> p.CreatedAt));
		}));
	}
}


#endif
