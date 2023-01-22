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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    baseurl = table.Column<string>(name: "base_url", type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_configs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "endpoint_definitions",
                columns: table => new
                {
                    Pattern = table.Column<string>(type: "text", nullable: false),
                    endpoint = table.Column<string>(type: "text", nullable: false),
                    ServiceConfigId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_endpoint_definitions", x => x.Pattern);
                    table.ForeignKey(
                        name: "FK_endpoint_definitions_service_configs_ServiceConfigId",
                        column: x => x.ServiceConfigId,
                        principalTable: "service_configs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "meta",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    EndpointDefinitionPattern = table.Column<string>(type: "text", nullable: true),
                    ServiceConfigId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_meta_endpoint_definitions_EndpointDefinitionPattern",
                        column: x => x.EndpointDefinitionPattern,
                        principalTable: "endpoint_definitions",
                        principalColumn: "Pattern");
                    table.ForeignKey(
                        name: "FK_meta_service_configs_ServiceConfigId",
                        column: x => x.ServiceConfigId,
                        principalTable: "service_configs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_endpoint_definitions_ServiceConfigId",
                table: "endpoint_definitions",
                column: "ServiceConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_meta_EndpointDefinitionPattern",
                table: "meta",
                column: "EndpointDefinitionPattern");

            migrationBuilder.CreateIndex(
                name: "IX_meta_ServiceConfigId",
                table: "meta",
                column: "ServiceConfigId");
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
