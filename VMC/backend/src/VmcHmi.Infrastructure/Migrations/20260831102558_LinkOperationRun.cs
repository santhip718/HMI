using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VmcHmi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkOperationRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_operation_runs_session_id",
                schema: "hmi",
                table: "operation_runs");

            migrationBuilder.CreateIndex(
                name: "IX_operation_runs_session_id",
                schema: "hmi",
                table: "operation_runs",
                column: "session_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_operation_runs_session_id",
                schema: "hmi",
                table: "operation_runs");

            migrationBuilder.CreateIndex(
                name: "IX_operation_runs_session_id",
                schema: "hmi",
                table: "operation_runs",
                column: "session_id");
        }
    }
}
