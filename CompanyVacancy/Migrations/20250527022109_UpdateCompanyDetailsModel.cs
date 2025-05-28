using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompanyVacancy.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCompanyDetailsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Company_Name",
                table: "CompanyDetails",
                newName: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "CompanyDetails",
                newName: "Company_Name");
        }
    }
}
