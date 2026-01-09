using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UrlShortener.Infrastructure.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplySnakeCaseNamingConvention(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
            {
                entity.SetTableName(tableName.ToSnakeCase());
            }

            foreach (var property in entity.GetProperties())
            {
                var storeObjectIdentifier = StoreObjectIdentifier.Table(tableName!, entity.GetSchema());
                var columnName = property.GetColumnName(storeObjectIdentifier);
                if (!string.IsNullOrEmpty(columnName))
                {
                    property.SetColumnName(columnName.ToSnakeCase());
                }
            }

            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (!string.IsNullOrEmpty(keyName))
                {
                    key.SetName(keyName.ToSnakeCase());
                }
            }

            foreach (var key in entity.GetForeignKeys())
            {
                var keyName = key.GetConstraintName();
                if (!string.IsNullOrEmpty(keyName))
                {
                    key.SetConstraintName(keyName.ToSnakeCase());
                }
            }

            foreach (var index in entity.GetIndexes())
            {
                var indexName = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(indexName))
                {
                    index.SetDatabaseName(indexName.ToSnakeCase());
                }
            }
        }
    }

    private static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var startUnderscores = Regex.Match(input, @"^_+");
        return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
    }
}
