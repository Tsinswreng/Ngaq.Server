using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ngaq.Biz.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PoRole",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    DbCreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    DbUpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<byte[]>(type: "BLOB", nullable: true),
                    LastUpdatedBy = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "BLOB", nullable: false),
                    UniqueName = table.Column<string>(type: "TEXT", nullable: false),
                    NickName = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Avatar = table.Column<string>(type: "TEXT", nullable: true),
                    RoleId = table.Column<byte[]>(type: "BLOB", nullable: true),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    DbCreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    DbUpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<byte[]>(type: "BLOB", nullable: true),
                    LastUpdatedBy = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Password",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Algo = table.Column<long>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    Salt = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    DbCreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    DbUpdatedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<byte[]>(type: "BLOB", nullable: true),
                    LastUpdatedBy = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Password", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Password_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Password_UserId",
                table: "Password",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PoRole_Key",
                table: "PoRole",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Password");

            migrationBuilder.DropTable(
                name: "PoRole");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
