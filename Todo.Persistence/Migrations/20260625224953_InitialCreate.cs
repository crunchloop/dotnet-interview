using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Todo.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TodoLists",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoLists", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "TodoListItems",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    TodoListId = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoListItems_TodoLists_TodoListId",
                        column: x => x.TodoListId,
                        principalTable: "TodoLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_TodoListItems_TodoListId",
                table: "TodoListItems",
                column: "TodoListId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TodoListItems");

            migrationBuilder.DropTable(name: "TodoLists");
        }
    }
}
