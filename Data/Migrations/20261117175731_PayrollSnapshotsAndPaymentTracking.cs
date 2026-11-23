using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmpleadaHogar.Data.Migrations
{
    /// <inheritdoc />
    public partial class PayrollSnapshotsAndPaymentTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractTypeSnapshot",
                table: "PayrollRecords",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeEmailSnapshot",
                table: "PayrollRecords",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeIdentificationSnapshot",
                table: "PayrollRecords",
                type: "character varying(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeNameSnapshot",
                table: "PayrollRecords",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeSocialSecuritySnapshot",
                table: "PayrollRecords",
                type: "character varying(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployerAddressSnapshot",
                table: "PayrollRecords",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployerCccSnapshot",
                table: "PayrollRecords",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployerIdentificationSnapshot",
                table: "PayrollRecords",
                type: "character varying(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployerNameSnapshot",
                table: "PayrollRecords",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidUtc",
                table: "PayrollRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeeklyHoursSnapshot",
                table: "PayrollRecords",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractTypeSnapshot",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "EmployeeEmailSnapshot",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "EmployeeIdentificationSnapshot",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "EmployeeNameSnapshot",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "EmployeeSocialSecuritySnapshot",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "EmployerAddressSnapshot",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "EmployerCccSnapshot",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "EmployerIdentificationSnapshot",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "EmployerNameSnapshot",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "PaidUtc",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "WeeklyHoursSnapshot",
                table: "PayrollRecords");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
