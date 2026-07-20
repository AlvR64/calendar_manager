using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [Migration("20260720090000_NormalizeBusinessCategories")]
    public partial class NormalizeBusinessCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE Businesses
                SET Category = CASE Category
                    WHEN N'Barberia' THEN N'barber'
                    WHEN N'Estetica' THEN N'beauty'
                    WHEN N'Fisioterapia' THEN N'physiotherapy'
                    WHEN N'Clases' THEN N'classes'
                    WHEN N'Consultas' THEN N'consulting'
                    ELSE Category
                END
                WHERE Category IN (N'Barberia', N'Estetica', N'Fisioterapia', N'Clases', N'Consultas');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE Businesses
                SET Category = CASE Category
                    WHEN N'barber' THEN N'Barberia'
                    WHEN N'beauty' THEN N'Estetica'
                    WHEN N'physiotherapy' THEN N'Fisioterapia'
                    WHEN N'classes' THEN N'Clases'
                    WHEN N'consulting' THEN N'Consultas'
                    ELSE Category
                END
                WHERE Category IN (N'barber', N'beauty', N'physiotherapy', N'classes', N'consulting');
                """);
        }
    }
}
