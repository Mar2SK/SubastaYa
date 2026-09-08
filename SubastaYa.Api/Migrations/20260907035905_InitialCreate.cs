using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubastaYa.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CATEGORIA",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    url_icono = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATEGORIA", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AUDITORIA_LOG",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    entidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    entidad_id = table.Column<int>(type: "int", nullable: false),
                    accion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: true),
                    detalle_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUDITORIA_LOG", x => x.id);
                    table.ForeignKey(
                        name: "FK_AUDITORIA_LOG_USUARIO_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "USUARIO",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BILLETERA",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    saldo_total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_retenido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_disponible = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BILLETERA", x => x.id);
                    table.ForeignKey(
                        name: "FK_BILLETERA_USUARIO_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "USUARIO",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SUBASTA",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vendedor_id = table.Column<int>(type: "int", nullable: false),
                    categoria_id = table.Column<int>(type: "int", nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    url_imagen = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    precio_base = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    incremento_minimo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SUBASTA", x => x.id);
                    table.ForeignKey(
                        name: "FK_SUBASTA_CATEGORIA_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "CATEGORIA",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SUBASTA_USUARIO_vendedor_id",
                        column: x => x.vendedor_id,
                        principalTable: "USUARIO",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PUJA",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    subasta_id = table.Column<int>(type: "int", nullable: false),
                    comprador_id = table.Column<int>(type: "int", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    fecha_puja = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PUJA", x => x.id);
                    table.ForeignKey(
                        name: "FK_PUJA_SUBASTA_subasta_id",
                        column: x => x.subasta_id,
                        principalTable: "SUBASTA",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PUJA_USUARIO_comprador_id",
                        column: x => x.comprador_id,
                        principalTable: "USUARIO",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRANSACCION_LEDGER",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    billetera_id = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    subasta_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSACCION_LEDGER", x => x.id);
                    table.ForeignKey(
                        name: "FK_TRANSACCION_LEDGER_BILLETERA_billetera_id",
                        column: x => x.billetera_id,
                        principalTable: "BILLETERA",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRANSACCION_LEDGER_SUBASTA_subasta_id",
                        column: x => x.subasta_id,
                        principalTable: "SUBASTA",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AUDITORIA_LOG_usuario_id",
                table: "AUDITORIA_LOG",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_BILLETERA_usuario_id",
                table: "BILLETERA",
                column: "usuario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PUJA_comprador_id",
                table: "PUJA",
                column: "comprador_id");

            migrationBuilder.CreateIndex(
                name: "IX_PUJA_subasta_id",
                table: "PUJA",
                column: "subasta_id");

            migrationBuilder.CreateIndex(
                name: "IX_SUBASTA_categoria_id",
                table: "SUBASTA",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_SUBASTA_vendedor_id",
                table: "SUBASTA",
                column: "vendedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_TRANSACCION_LEDGER_billetera_id",
                table: "TRANSACCION_LEDGER",
                column: "billetera_id");

            migrationBuilder.CreateIndex(
                name: "IX_TRANSACCION_LEDGER_subasta_id",
                table: "TRANSACCION_LEDGER",
                column: "subasta_id");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_email",
                table: "USUARIO",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AUDITORIA_LOG");

            migrationBuilder.DropTable(
                name: "PUJA");

            migrationBuilder.DropTable(
                name: "TRANSACCION_LEDGER");

            migrationBuilder.DropTable(
                name: "BILLETERA");

            migrationBuilder.DropTable(
                name: "SUBASTA");

            migrationBuilder.DropTable(
                name: "CATEGORIA");

            migrationBuilder.DropTable(
                name: "USUARIO");
        }
    }
}
