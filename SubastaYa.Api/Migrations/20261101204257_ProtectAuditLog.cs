using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubastaYa.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProtectAuditLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR ALTER TRIGGER dbo.TR_AUDITORIA_LOG_INMUTABLE
                ON dbo.AUDITORIA_LOG
                AFTER UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    THROW 50001,
                        'Los registros de auditoria no pueden modificarse ni eliminarse.',
                        1;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS dbo.TR_AUDITORIA_LOG_INMUTABLE;
                """);
        }
    }
}
