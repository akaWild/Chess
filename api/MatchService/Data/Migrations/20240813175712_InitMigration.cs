using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Creator = table.Column<string>(type: "text", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastMoveMadeAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeLimit = table.Column<int>(type: "integer", nullable: true),
                    ExtraTimePerMove = table.Column<int>(type: "integer", nullable: true),
                    WhiteSidePlayer = table.Column<string>(type: "text", nullable: true),
                    ActingSide = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Acceptor = table.Column<string>(type: "text", nullable: true),
                    AILevel = table.Column<int>(type: "integer", nullable: true),
                    Board = table.Column<string>(type: "text", nullable: true),
                    History = table.Column<string[]>(type: "text[]", nullable: false),
                    Winner = table.Column<string>(type: "text", nullable: true),
                    WinBy = table.Column<int>(type: "integer", nullable: true),
                    DrawBy = table.Column<int>(type: "integer", nullable: true),
                    DrawRequestedSide = table.Column<int>(type: "integer", nullable: true),
                    WhiteSideTimeRemaining = table.Column<int>(type: "integer", nullable: true),
                    BlackSideTimeRemaining = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.MatchId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matches");
        }
    }
}
