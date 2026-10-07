using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FitBook.Api.Data;

/// <summary>
/// Stores every DateTime as UTC and marks values read from the database as UTC.
/// </summary>
public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : DateTime.SpecifyKind(v, DateTimeKind.Utc),
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
