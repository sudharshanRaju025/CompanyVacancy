using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompanyVacancy.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCompanyDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyDetail",
                table: "CompanyDetail");

            migrationBuilder.RenameTable(
                name: "CompanyDetail",
                newName: "CompanyDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyDetails",
                table: "CompanyDetails",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyDetails",
                table: "CompanyDetails");

            migrationBuilder.RenameTable(
                name: "CompanyDetails",
                newName: "CompanyDetail");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyDetail",
                table: "CompanyDetail",
                column: "Id");
        }
    }
}
