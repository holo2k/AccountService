using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.Migrations
{
    /// <inheritdoc />
    public partial class EditAccrueInterestProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                    DROP PROCEDURE IF EXISTS accrue_interest(UUID);
                ");
            migrationBuilder.Sql(@"
                    CREATE OR REPLACE FUNCTION accrue_interest(account_id UUID)
                    RETURNS INTEGER
                    LANGUAGE plpgsql
                    AS $$
                    DECLARE
                        updated_count INTEGER;
                    BEGIN
                        UPDATE ""Accounts""
                        SET ""Balance"" = ""Balance"" + (""Balance"" * (""PercentageRate"" / 100))
                        WHERE ""Id"" = account_id
                          AND ""PercentageRate"" IS NOT NULL;

                        GET DIAGNOSTICS updated_count = ROW_COUNT;
                        RETURN updated_count;
                    END;
                    $$;
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS accrue_interest(UUID);");
        }
    }
}
