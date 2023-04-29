using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class endpointdefinition_mapTo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "map_to",
                table: "endpoint_definitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_endpoint_definitions_map_to",
                table: "endpoint_definitions",
                column: "map_to");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_endpoint_definitions_map_to",
                table: "endpoint_definitions");

            migrationBuilder.DropColumn(
                name: "map_to",
                table: "endpoint_definitions");
        }
    }
}
