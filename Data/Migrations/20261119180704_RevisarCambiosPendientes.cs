using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmpleadaHogar.Data.Migrations
{
    /// <inheritdoc />
    public partial class RevisarCambiosPendientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractTypeDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsIndefinite = table.Column<bool>(type: "boolean", nullable: false),
                    AllowsPartTime = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTypeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EconomicParameters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    ValueType = table.Column<int>(type: "integer", nullable: false),
                    NumericValue = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    TextValue = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EconomicParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollConceptDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    CalculationType = table.Column<int>(type: "integer", nullable: false),
                    DefaultValue = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    IsAutomatic = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollConceptDefinitions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractTypeDefinitions_OwnerUserId_Code",
                table: "ContractTypeDefinitions",
                columns: new[] { "OwnerUserId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EconomicParameters_OwnerUserId_Code_ValidFrom",
                table: "EconomicParameters",
                columns: new[] { "OwnerUserId", "Code", "ValidFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EconomicParameters_OwnerUserId_ValidFrom_ValidTo",
                table: "EconomicParameters",
                columns: new[] { "OwnerUserId", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollConceptDefinitions_OwnerUserId_Code_ValidFrom",
                table: "PayrollConceptDefinitions",
                columns: new[] { "OwnerUserId", "Code", "ValidFrom" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractTypeDefinitions");

            migrationBuilder.DropTable(
                name: "EconomicParameters");

            migrationBuilder.DropTable(
                name: "PayrollConceptDefinitions");
        }
    }
}
