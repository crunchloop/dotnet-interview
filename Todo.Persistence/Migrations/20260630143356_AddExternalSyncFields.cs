using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Todo.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalSyncFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoListItems_TodoLists_TodoListId",
                table: "TodoListItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TodoLists",
                table: "TodoLists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TodoListItems",
                table: "TodoListItems");

            migrationBuilder.RenameTable(
                name: "TodoLists",
                newName: "TodoList");

            migrationBuilder.RenameTable(
                name: "TodoListItems",
                newName: "TodoListItem");

            migrationBuilder.RenameIndex(
                name: "IX_TodoListItems_TodoListId",
                table: "TodoListItem",
                newName: "IX_TodoListItem_TodoListId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TodoList",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "TodoList",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TodoList",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TodoListItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "TodoListItem",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TodoListItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TodoList",
                table: "TodoList",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TodoListItem",
                table: "TodoListItem",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TodoList_ExternalId",
                table: "TodoList",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoListItem_ExternalId",
                table: "TodoListItem",
                column: "ExternalId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoListItem_TodoList_TodoListId",
                table: "TodoListItem",
                column: "TodoListId",
                principalTable: "TodoList",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoListItem_TodoList_TodoListId",
                table: "TodoListItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TodoListItem",
                table: "TodoListItem");

            migrationBuilder.DropIndex(
                name: "IX_TodoListItem_ExternalId",
                table: "TodoListItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TodoList",
                table: "TodoList");

            migrationBuilder.DropIndex(
                name: "IX_TodoList_ExternalId",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TodoListItem");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "TodoListItem");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TodoListItem");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TodoList");

            migrationBuilder.RenameTable(
                name: "TodoListItem",
                newName: "TodoListItems");

            migrationBuilder.RenameTable(
                name: "TodoList",
                newName: "TodoLists");

            migrationBuilder.RenameIndex(
                name: "IX_TodoListItem_TodoListId",
                table: "TodoListItems",
                newName: "IX_TodoListItems_TodoListId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TodoListItems",
                table: "TodoListItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TodoLists",
                table: "TodoLists",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoListItems_TodoLists_TodoListId",
                table: "TodoListItems",
                column: "TodoListId",
                principalTable: "TodoLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
