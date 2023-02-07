using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    createdatutc = table.Column<DateTime>(name: "created_at_utc", type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_configs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "endpoint_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pattern = table.Column<string>(type: "text", nullable: false),
                    endpoint = table.Column<string>(type: "text", nullable: false),
                    method = table.Column<string>(type: "text", nullable: false),
                    serviceconfigid = table.Column<Guid>(name: "service_config_id", type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_endpoint_definitions", x => x.id);
                    table.ForeignKey(
                        name: "FK_endpoint_definitions_service_configs_service_config_id",
                        column: x => x.serviceconfigid,
                        principalTable: "service_configs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meta",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    endpointdefinitionid = table.Column<Guid>(name: "endpoint_definition_id", type: "uuid", nullable: true),
                    serviceconfigid = table.Column<Guid>(name: "service_config_id", type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meta", x => x.id);
                    table.ForeignKey(
                        name: "FK_meta_endpoint_definitions_endpoint_definition_id",
                        column: x => x.endpointdefinitionid,
                        principalTable: "endpoint_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_meta_service_configs_service_config_id",
                        column: x => x.serviceconfigid,
                        principalTable: "service_configs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_endpoint_definitions_service_config_id",
                table: "endpoint_definitions",
                column: "service_config_id");

            migrationBuilder.CreateIndex(
                name: "IX_meta_endpoint_definition_id",
                table: "meta",
                column: "endpoint_definition_id");

            migrationBuilder.CreateIndex(
                name: "IX_meta_service_config_id",
                table: "meta",
                column: "service_config_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meta");

            migrationBuilder.DropTable(
                name: "endpoint_definitions");

            migrationBuilder.DropTable(
                name: "service_configs");
        }
    }
}
