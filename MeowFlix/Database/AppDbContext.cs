using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MeowFlix.Database.Tables;

using Windows.UI;

namespace MeowFlix.Database;
public class AppDbContext : DbContext
{
    public AppDbContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var DirPath = Constants.RootDirectoryPath;
        if (!Directory.Exists(DirPath))
        {
            Directory.CreateDirectory(DirPath);
        }
        var dbFile = @$"{DirPath}\MeowFlix.db";
        optionsBuilder.UseSqlite($"Data Source={dbFile}");
    }

    public async Task DeleteAndRecreateServerTables(string table)
    {
        await Database.EnsureCreatedAsync();
        await Database.ExecuteSqlAsync($"""
            PRAGMA foreign_keys = false;

            DROP TABLE IF EXISTS "{table}";
            CREATE TABLE "{table}" (
              "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              "Title" TEXT,
              "filePath" TEXT,
              "IsActive" INTEGER NOT NULL,
              "ServerType" INTEGER NOT NULL
            );

            PRAGMA foreign_keys = true;
            """);
    }

    public async Task DeleteAndRecreateMediaTables(string table)
    {
        await Database.EnsureCreatedAsync();
        await Database.ExecuteSqlAsync($"""
            PRAGMA foreign_keys = false;

            DROP TABLE IF EXISTS "{table}";
            CREATE TABLE "{table}" (
              "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              "Title" TEXT,
              "filePath" TEXT,
              "DateTime" TEXT,
              "FileSize" TEXT,
              "GroupKey" TEXT,
              "ServerType" INTEGER NOT NULL
            );

            PRAGMA foreign_keys = true;
            """);
    }
    public DbSet<StorageTable> Storages { get; set; }
    public DbSet<MediaServerTable> MediaServers { get; set; }
    public DbSet<SubtitleServerTable> SubtitleServers { get; set; }
    public DbSet<AnimeTable> Animes { get; set; }
    public DbSet<MovieTable> Movies { get; set; }
    public DbSet<SeriesTable> Series { get; set; }
    public DbSet<AuthTable> Auth { get; set; }
}
