using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.Migrations
{
    /// <inheritdoc />
    public partial class AddAccrueInterestProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                    CREATE OR REPLACE PROCEDURE accrue_interest(account_id UUID)
                    LANGUAGE plpgsql
                    AS $$
                    BEGIN
                        UPDATE ""Accounts""
                        SET ""Balance"" = ""Balance"" + (""Balance"" * (""PercentageRate"" / 100))
                        WHERE ""Id"" = account_id AND ""PercentageRate"" IS NOT NULL;
                    END;
                    $$;
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
