using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operative.Services.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Board",
                columns: table => new
                {
                    PK = table.Column<Guid>(type: "uuid", nullable: false),
                    ID = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Timestamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Board", x => x.PK);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    PK = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CustomerID = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    ID = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.PK);
                });

            migrationBuilder.CreateTable(
                name: "Component",
                columns: table => new
                {
                    PK = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    BoardPK = table.Column<Guid>(type: "uuid", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Component", x => x.PK);
                    table.ForeignKey(
                        name: "FK_Component_Board_BoardPK",
                        column: x => x.BoardPK,
                        principalTable: "Board",
                        principalColumn: "PK");
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    PK = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    BoardPK = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    OrderPK = table.Column<Guid>(type: "uuid", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.PK);
                    table.ForeignKey(
                        name: "FK_OrderItem_Board_BoardPK",
                        column: x => x.BoardPK,
                        principalTable: "Board",
                        principalColumn: "PK",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItem_Orders_OrderPK",
                        column: x => x.OrderPK,
                        principalTable: "Orders",
                        principalColumn: "PK");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Component_BoardPK",
                table: "Component",
                column: "BoardPK");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_BoardPK",
                table: "OrderItem",
                column: "BoardPK");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderPK",
                table: "OrderItem",
                column: "OrderPK");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ID",
                table: "Orders",
                column: "ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Name_Description",
                table: "Orders",
                columns: new[] { "Name", "Description" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Component");

            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "Board");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
