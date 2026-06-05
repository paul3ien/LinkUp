using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Migrations
{
    /// <inheritdoc />
    public partial class AddUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Assign a unique username to every existing user that has none
            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""Username"" = concat(
                    (ARRAY['Dragon','Phoenix','Wolf','Tiger','Eagle','Falcon','Shadow','Storm',
                           'Blaze','Frost','Viper','Hawk','Raven','Cobra','Lynx','Panda'])[
                        (floor(random() * 16) + 1)::int
                    ],
                    lpad((floor(random() * 9000) + 1000)::int::text, 4, '0')
                )
                WHERE ""Username"" = '';
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");
        }
    }
}
