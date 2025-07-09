using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImoutoRebirth.Lilin.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAllTagColorsForReadability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                -- Update all tag colors for better readability on dark themes
                
                -- Meta: #5C00FF → #4ECDC4 (dark purple → teal)
                UPDATE public."TagTypes" 
                SET "Color" = 5163460, "ModifiedOn" = NOW() 
                WHERE "Name" = 'Meta';

                -- Faults: #000000 → #999999 (black → gray)
                UPDATE public."TagTypes" 
                SET "Color" = 10066329, "ModifiedOn" = NOW() 
                WHERE "Name" = 'Faults';

                -- Circle: #000000 → #999999 (black → gray)  
                UPDATE public."TagTypes" 
                SET "Color" = 10066329, "ModifiedOn" = NOW() 
                WHERE "Name" = 'Circle';

                -- Copyright: #AA002A → #c797ff (dark red → light purple)
                UPDATE public."TagTypes" 
                SET "Color" = 13086463, "ModifiedOn" = NOW() 
                WHERE "Name" = 'Copyright';

                -- Artist: #AA0000 → #FF4444 (dark red → bright red)
                UPDATE public."TagTypes" 
                SET "Color" = 16729156, "ModifiedOn" = NOW() 
                WHERE "Name" = 'Artist';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                -- Revert all tag colors to original values
                
                -- Revert Meta: #4ECDC4 → #5C00FF (teal → dark purple)
                UPDATE public."TagTypes" 
                SET "Color" = 6029567, "ModifiedOn" = NOW() 
                WHERE "Name" = 'Meta';

                -- Revert Faults: #999999 → #000000 (gray → black)
                UPDATE public."TagTypes" 
                SET "Color" = 0, "ModifiedOn" = NOW() 
                WHERE "Name" = 'Faults';

                -- Revert Circle: #999999 → #000000 (gray → black)
                UPDATE public."TagTypes" 
                SET "Color" = 0, "ModifiedOn" = NOW() 
                WHERE "Name" = 'Circle';

                -- Revert Copyright: #c797ff → #AA002A (light purple → dark red)
                UPDATE public."TagTypes" 
                SET "Color" = 11141290, "ModifiedOn" = NOW() 
                WHERE "Name" = 'Copyright';

                -- Revert Artist: #FF4444 → #AA0000 (bright red → dark red)
                UPDATE public."TagTypes" 
                SET "Color" = 11141120, "ModifiedOn" = NOW() 
                WHERE "Name" = 'Artist';
                """);
        }
    }
}
