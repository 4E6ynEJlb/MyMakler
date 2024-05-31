using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repos.Migrations
{
    /// <inheritdoc />
    public partial class CreateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    _user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_login = table.Column<string>(type: "varchar(16)", nullable: false),
                    user_password = table.Column<string>(type: "varchar(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x._user_id);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    _user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    _user_name = table.Column<string>(type: "varchar(16)", nullable: false),
                    is_admin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x._user_id);
                    table.ForeignKey(
                        name: "FK_user_profiles_users__user_id",
                        column: x => x._user_id,
                        principalTable: "users",
                        principalColumn: "_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ads",
                columns: table => new
                {
                    ad_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    phone_number = table.Column<int>(type: "int", nullable: false),
                    _user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ad_text = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    pic_link = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, defaultValue: "Empty"),
                    rating = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    creation_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deletion_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ads", x => x.ad_id);
                    table.ForeignKey(
                        name: "FK_ads_user_profiles__user_id",
                        column: x => x._user_id,
                        principalTable: "user_profiles",
                        principalColumn: "_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ads__user_id",
                table: "ads",
                column: "_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ads_ad_id",
                table: "ads",
                column: "ad_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_profiles__user_id",
                table: "user_profiles",
                column: "_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_user_login",
                table: "users",
                column: "user_login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_user_password",
                table: "users",
                column: "user_password",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ads");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
