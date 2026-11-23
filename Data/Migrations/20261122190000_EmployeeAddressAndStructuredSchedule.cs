using EmpleadaHogar.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmpleadaHogar.Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20261122190000_EmployeeAddressAndStructuredSchedule")]
public partial class EmployeeAddressAndStructuredSchedule : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "City", table: "Employees", type: "character varying(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "PostalCode", table: "Employees", type: "character varying(5)", maxLength: 5, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Province", table: "Employees", type: "character varying(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "WorkDays", table: "Employees", type: "character varying(20)", maxLength: 20, nullable: true);
        migrationBuilder.AddColumn<TimeOnly>(name: "WorkStartTime", table: "Employees", type: "time without time zone", nullable: true);
        migrationBuilder.AddColumn<TimeOnly>(name: "WorkEndTime", table: "Employees", type: "time without time zone", nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "City", table: "Employees");
        migrationBuilder.DropColumn(name: "PostalCode", table: "Employees");
        migrationBuilder.DropColumn(name: "Province", table: "Employees");
        migrationBuilder.DropColumn(name: "WorkDays", table: "Employees");
        migrationBuilder.DropColumn(name: "WorkStartTime", table: "Employees");
        migrationBuilder.DropColumn(name: "WorkEndTime", table: "Employees");
    }
}
