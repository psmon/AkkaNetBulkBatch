using BulkBatchApp.Config;
using BulkBatchApp.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BulkBatchApp.Repositories
{
    public class EventRepository : DbContext
    {
        private string database = "webnori";

        private readonly AppSettings _appSettings;

        public DbSet<QueryIntResult> Native { get; set; } //For Native Query

        private readonly ILogger<EventRepository> _logger;

        public EventRepository(AppSettings appSettings, ILogger<EventRepository> logger)
        {
            _appSettings = appSettings;
            _logger = logger;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<QueryIntResult>(eb =>
                {
                    eb.HasNoKey();
                });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbOption = "Convert Zero Datetime=true;";
            string dbConnectionString = string.Empty;
            dbConnectionString = _appSettings.DBConnection + $"database={database};" + dbOption;
            optionsBuilder.UseMySql(dbConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        private string DateToDbString(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd hh:mm:ss");
        }

        public async Task BulkInsertOrUpdateToEventTable(List<TestEvent> testEvents)
        {
            StringBuilder queryStr = new StringBuilder("");
            queryStr.Append($"INSERT INTO tbl_test_bulk( id, action_type, action_name, upd_dt, reg_dt) VALUES ");

            int idx = 0;
            foreach (var testEvent in testEvents)
            {
                idx++;
                string appendQuery = $"('{testEvent.id}',{testEvent.action_type},'{testEvent.action_name}','{DateToDbString(testEvent.upd_dt)}'," +
                    $"'{DateToDbString(testEvent.reg_dt)}')";
                if (idx == testEvents.Count)
                {
                    queryStr.AppendLine(appendQuery);
                }
                else
                {
                    queryStr.AppendLine(appendQuery + ",");
                }
            }

            queryStr.AppendLine(@" 
            ON DUPLICATE KEY UPDATE action_type= VALUES(action_type), action_name=VALUES(action_name), upd_dt=VALUES(upd_dt), reg_dt=VALUES(reg_dt);
            SELECT 1 as result;");

            _logger.LogDebug("======= BulkQuery  ========");
            _logger.LogDebug(queryStr.ToString());

            await Native.FromSqlRaw(queryStr.ToString()).ToListAsync().ConfigureAwait(false);
        }

    }
}
