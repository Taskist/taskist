using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Taskist.Core.Common;
using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Logging;
using Taskist.Core.Security;
using Taskist.Data.Configuration;

namespace Taskist.Data;

public class ApplicationContext : DbContext
{
	private readonly IHttpHelper _httpHelper;
	private readonly ISecretProtector? _secretProtector;

	public ApplicationContext(DbContextOptions options, IHttpHelper httpHelper, ISecretProtector? secretProtector = null) : base(options)
	{
		_httpHelper = httpHelper;
		_secretProtector = secretProtector;
	}

	public DbSet<AuditLog> AuditLogs { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		var typeConfigurations = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
			(type.BaseType?.IsGenericType ?? false)
				&& (type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>)));

		foreach (var typeConfiguration in typeConfigurations)
		{
			var configuration = (IMappingConfiguration)Activator.CreateInstance(typeConfiguration);
			configuration.ApplyConfiguration(modelBuilder);
		}

		base.OnModelCreating(modelBuilder);

		ApplyEncryptedConverters(modelBuilder);
	}

	private void ApplyEncryptedConverters(ModelBuilder modelBuilder)
	{
		foreach (var entityType in modelBuilder.Model.GetEntityTypes())
		{
			foreach (var property in entityType.GetProperties())
			{
				if (property.ClrType != typeof(string))
					continue;

				if (property.PropertyInfo?.GetCustomAttribute<EncryptedAttribute>() != null)
					property.SetValueConverter(new Configuration.EncryptedStringConverter(_secretProtector));
			}
		}
	}

	#region Auditing

	public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
	{
		var drafts = CollectAuditDrafts();

		var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

		if (drafts.Count > 0)
		{
			try
			{
				AuditLogs.AddRange(BuildAuditLogs(drafts));
				await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
			}
			catch (Exception ex)
			{
				WriteTrace(ex);
			}
		}

		return result;
	}

	public override int SaveChanges(bool acceptAllChangesOnSuccess)
	{
		var drafts = CollectAuditDrafts();

		var result = base.SaveChanges(acceptAllChangesOnSuccess);

		if (drafts.Count > 0)
		{
			try
			{
				AuditLogs.AddRange(BuildAuditLogs(drafts));
				base.SaveChanges(acceptAllChangesOnSuccess);
			}
			catch (Exception ex)
			{
				WriteTrace(ex);
			}
		}

		return result;
	}

	private List<AuditDraft> CollectAuditDrafts()
	{
		var drafts = new List<AuditDraft>();

		var userId = _httpHelper?.GetCurrentUserId();
		if (userId == null)
			return drafts;

		ChangeTracker.DetectChanges();

		foreach (var entry in ChangeTracker.Entries())
		{
			if (entry.Entity is not IAuditableEntity)
				continue;

			switch (entry.State)
			{
				case EntityState.Added:
					drafts.Add(new AuditDraft { Entry = entry, Action = "Added", ResolveAfterSave = true });
					break;

				case EntityState.Deleted:
					drafts.Add(new AuditDraft
					{
						Entry = entry,
						Action = "Deleted",
						Old = entry.Properties.Where(IsAudited).ToDictionary(p => p.Metadata.Name, p => p.OriginalValue)
					});
					break;

				case EntityState.Modified:
					var changed = entry.Properties
						.Where(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue) && IsAudited(p))
						.ToList();
					if (changed.Count == 0)
						break;

					drafts.Add(new AuditDraft
					{
						Entry = entry,
						Action = IsSoftDelete(entry, changed) ? "Deleted" : "Modified",
						Old = changed.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue),
						New = changed.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)
					});
					break;
			}
		}

		return drafts;
	}

	private List<AuditLog> BuildAuditLogs(List<AuditDraft> drafts)
	{
		var userId = _httpHelper?.GetCurrentUserId();
		var ipAddress = _httpHelper?.GetCurrentIpAddress();
		var now = DateTime.UtcNow;

		var logs = new List<AuditLog>(drafts.Count);

		foreach (var draft in drafts)
		{

			if (draft.ResolveAfterSave)
				draft.New = draft.Entry.Properties.Where(IsAudited).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

			logs.Add(new AuditLog
			{
				EntityName = draft.Entry.Metadata.ClrType.Name,
				EntityId = (draft.Entry.Entity as BaseEntity)?.Id,
				Action = draft.Action,
				UserId = userId,
				OldValues = Serialize(draft.Old),
				NewValues = Serialize(draft.New),
				CreatedOnUtc = now,
				IpAddress = ipAddress
			});
		}

		return logs;
	}

	private static readonly ConcurrentDictionary<PropertyInfo, bool> _auditableCache = new();

	private static bool IsAudited(PropertyEntry property)
	{
		var info = property.Metadata.PropertyInfo;
		if (info == null)
			return true;

		return _auditableCache.GetOrAdd(info, p => p.GetCustomAttribute<NotAuditedAttribute>() == null
			&& p.GetCustomAttribute<EncryptedAttribute>() == null);
	}

	private static bool IsSoftDelete(EntityEntry entry, List<PropertyEntry> changed)
	{
		return entry.Entity is ISoftDeletedEntity
			&& changed.Any(p => p.Metadata.Name == nameof(ISoftDeletedEntity.Deleted)
				&& p.OriginalValue is false && p.CurrentValue is true);
	}

	private static string Serialize(Dictionary<string, object> values)
	{
		if (values == null || values.Count == 0)
			return null;

		try
		{
			return JsonSerializer.Serialize(values);
		}
		catch
		{

			return JsonSerializer.Serialize(values.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString()));
		}
	}

	private static void WriteTrace(Exception ex)
	{
		Trace.WriteLine($"Taskist.Data >> ApplicationContext >> Audit >> Error >> {ex.Message}");
	}

	private sealed class AuditDraft
	{
		public EntityEntry Entry { get; init; }
		public string Action { get; init; }
		public Dictionary<string, object> Old { get; set; }
		public Dictionary<string, object> New { get; set; }
		public bool ResolveAfterSave { get; init; }
	}

	#endregion
}
