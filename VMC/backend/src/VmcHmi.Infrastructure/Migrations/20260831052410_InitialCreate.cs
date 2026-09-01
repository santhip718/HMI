using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VmcHmi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hmi");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "hmi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "machine_sessions",
                schema: "hmi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_stage = table.Column<string>(type: "text", nullable: false),
                    operation_status = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_machine_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_machine_sessions_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "hmi",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "checklist_items",
                schema: "hmi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stage = table.Column<string>(type: "text", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checklist_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_checklist_items_machine_sessions_session_id",
                        column: x => x.session_id,
                        principalSchema: "hmi",
                        principalTable: "machine_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "operation_runs",
                schema: "hmi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    stopped_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_runs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_operation_runs_machine_sessions_session_id",
                        column: x => x.session_id,
                        principalSchema: "hmi",
                        principalTable: "machine_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tools",
                schema: "hmi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    RequiredByItemId = table.Column<string>(type: "text", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tools_machine_sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "hmi",
                        principalTable: "machine_sessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_checklist_items_session_id",
                schema: "hmi",
                table: "checklist_items",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_machine_sessions_user_id",
                schema: "hmi",
                table: "machine_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_operation_runs_session_id",
                schema: "hmi",
                table: "operation_runs",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_tools_SessionId",
                schema: "hmi",
                table: "tools",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                schema: "hmi",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "checklist_items",
                schema: "hmi");

            migrationBuilder.DropTable(
                name: "operation_runs",
                schema: "hmi");

            migrationBuilder.DropTable(
                name: "tools",
                schema: "hmi");

            migrationBuilder.DropTable(
                name: "machine_sessions",
                schema: "hmi");

            migrationBuilder.DropTable(
                name: "users",
                schema: "hmi");
        }
    }
}
