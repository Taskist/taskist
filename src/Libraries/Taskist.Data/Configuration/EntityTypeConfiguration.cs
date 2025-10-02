using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Common;
using Taskist.Core.Extensions;

namespace Taskist.Data.Configuration;

public partial class EntityTypeConfiguration<TEntity> : IMappingConfiguration,
    IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
{
    #region Methods

    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        //ConvertToSnakeCase(builder);
    }

    public virtual void ApplyConfiguration(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(this);
    }

    #endregion

    #region Helpers

    private void ConvertToSnakeCase(EntityTypeBuilder<TEntity> builder)
    {
        var entityType = builder.Metadata;

        entityType.SetTableName(entityType.GetTableName().ToSnakeCase());

        foreach (var property in entityType.GetProperties())
        {
            property.SetColumnName(property.Name.ToSnakeCase());
        }

        foreach (var foreignKey in entityType.GetForeignKeys())
        {
            foreignKey.SetConstraintName(foreignKey.GetConstraintName().ToSnakeCase());
        }

        foreach (var key in entityType.GetKeys())
        {
            key.SetName(key.GetName().ToSnakeCase());
        }
    }

    #endregion
}