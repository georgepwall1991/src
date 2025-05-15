#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace CQRSSolution.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "OutboxMessages",
            table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                Type = table.Column<string>("nvarchar(max)", nullable: false),
                Payload = table.Column<string>("nvarchar(max)", nullable: false),
                OccurredOnUtc = table.Column<DateTime>("datetime2", nullable: false),
                ProcessedOnUtc = table.Column<DateTime>("datetime2", nullable: true),
                Error = table.Column<string>("nvarchar(max)", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_OutboxMessages", x => x.Id); });

        migrationBuilder.CreateTable(
            "Orders",
            table => new
            {
                OrderId = table.Column<Guid>("uniqueidentifier", nullable: false),
                CustomerName = table.Column<string>("nvarchar(200)", maxLength: 200, nullable: false),
                OrderDate = table.Column<DateTime>("datetime2", nullable: false),
                TotalAmount = table.Column<decimal>("decimal(18,2)", nullable: false),
                Status = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Orders", x => x.OrderId); });

        migrationBuilder.CreateTable(
            "OrderItems",
            table => new
            {
                OrderItemId = table.Column<Guid>("uniqueidentifier", nullable: false),
                OrderId = table.Column<Guid>("uniqueidentifier", nullable: false),
                ProductName = table.Column<string>("nvarchar(200)", maxLength: 200, nullable: false),
                Quantity = table.Column<int>("int", nullable: false),
                UnitPrice = table.Column<decimal>("decimal(18,2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderItems", x => x.OrderItemId);
                table.ForeignKey(
                    "FK_OrderItems_Orders_OrderId",
                    x => x.OrderId,
                    "Orders",
                    "OrderId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            "IX_OrderItems_OrderId",
            "OrderItems",
            "OrderId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "OrderItems");

        migrationBuilder.DropTable(
            "OutboxMessages");

        migrationBuilder.DropTable(
            "Orders");
    }
}