using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QueueSystem.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialRealSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "branch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    MaxCapacity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "service",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AvgServiceTimeMinutes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "crowd_log",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BranchId = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalCustomers = table.Column<int>(type: "INTEGER", nullable: false),
                    CrowdLevel = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crowd_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crowd_log_branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BranchId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staff_branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prediction_log",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BranchId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedAvgWaitMinutes = table.Column<double>(type: "REAL", nullable: false),
                    EstimatedClearingMinutes = table.Column<double>(type: "REAL", nullable: false),
                    QueueLength = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveCounters = table.Column<int>(type: "INTEGER", nullable: false),
                    AvgServiceMinutesInput = table.Column<double>(type: "REAL", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prediction_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prediction_log_branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prediction_log_service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "queue_day_sequence",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BranchId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    DateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastNumber = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_day_sequence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_queue_day_sequence_branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_queue_day_sequence_service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BranchId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalCustomersServed = table.Column<int>(type: "INTEGER", nullable: false),
                    AvgWaitMinutes = table.Column<double>(type: "REAL", nullable: false),
                    PeakHour = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    BusiestServiceId = table.Column<int>(type: "INTEGER", nullable: true),
                    GeneratedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report", x => x.Id);
                    table.ForeignKey(
                        name: "FK_report_branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_service_BusiestServiceId",
                        column: x => x.BusiestServiceId,
                        principalTable: "service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "counter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BranchId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    StaffId = table.Column<int>(type: "INTEGER", nullable: true),
                    Label = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsOpen = table.Column<bool>(type: "INTEGER", nullable: false),
                    StaffAvailable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_counter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_counter_branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_counter_service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_counter_staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "simulation_run",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BranchId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedByStaffId = table.Column<int>(type: "INTEGER", nullable: true),
                    RequestedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    GeneratedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    FixedServiceId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_simulation_run", x => x.Id);
                    table.ForeignKey(
                        name: "FK_simulation_run_branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_simulation_run_service_FixedServiceId",
                        column: x => x.FixedServiceId,
                        principalTable: "service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_simulation_run_staff_StartedByStaffId",
                        column: x => x.StartedByStaffId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "queue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BranchId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    TicketCode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CalledUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RecalledUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CounterId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_queue_branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_queue_counter_CounterId",
                        column: x => x.CounterId,
                        principalTable: "counter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_queue_service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QueueTicketId = table.Column<int>(type: "INTEGER", nullable: false),
                    ArrivalUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsSimulated = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_queue_QueueTicketId",
                        column: x => x.QueueTicketId,
                        principalTable: "queue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "queue_assignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QueueTicketId = table.Column<int>(type: "INTEGER", nullable: false),
                    CounterId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReleasedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_assignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_queue_assignment_counter_CounterId",
                        column: x => x.CounterId,
                        principalTable: "counter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_queue_assignment_queue_QueueTicketId",
                        column: x => x.QueueTicketId,
                        principalTable: "queue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_counter_BranchId",
                table: "counter",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_counter_ServiceId",
                table: "counter",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_counter_StaffId",
                table: "counter",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_crowd_log_BranchId",
                table: "crowd_log",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_QueueTicketId",
                table: "customer",
                column: "QueueTicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prediction_log_BranchId",
                table: "prediction_log",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_prediction_log_ServiceId",
                table: "prediction_log",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_BranchId_Status_ServiceId",
                table: "queue",
                columns: new[] { "BranchId", "Status", "ServiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_queue_CounterId",
                table: "queue",
                column: "CounterId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_ServiceId",
                table: "queue",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_assignment_CounterId",
                table: "queue_assignment",
                column: "CounterId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_assignment_QueueTicketId_ReleasedUtc",
                table: "queue_assignment",
                columns: new[] { "QueueTicketId", "ReleasedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_queue_day_sequence_BranchId_ServiceId_DateUtc",
                table: "queue_day_sequence",
                columns: new[] { "BranchId", "ServiceId", "DateUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_queue_day_sequence_ServiceId",
                table: "queue_day_sequence",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_report_BranchId_ReportDate",
                table: "report",
                columns: new[] { "BranchId", "ReportDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_BusiestServiceId",
                table: "report",
                column: "BusiestServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_service_Code",
                table: "service",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_simulation_run_BranchId",
                table: "simulation_run",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_simulation_run_FixedServiceId",
                table: "simulation_run",
                column: "FixedServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_simulation_run_StartedByStaffId",
                table: "simulation_run",
                column: "StartedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_staff_BranchId",
                table: "staff",
                column: "BranchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "crowd_log");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "prediction_log");

            migrationBuilder.DropTable(
                name: "queue_assignment");

            migrationBuilder.DropTable(
                name: "queue_day_sequence");

            migrationBuilder.DropTable(
                name: "report");

            migrationBuilder.DropTable(
                name: "simulation_run");

            migrationBuilder.DropTable(
                name: "queue");

            migrationBuilder.DropTable(
                name: "counter");

            migrationBuilder.DropTable(
                name: "service");

            migrationBuilder.DropTable(
                name: "staff");

            migrationBuilder.DropTable(
                name: "branch");
        }
    }
}
