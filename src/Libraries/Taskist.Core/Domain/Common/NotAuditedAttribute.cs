namespace Taskist.Core.Domain.Common;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class NotAuditedAttribute : Attribute
{
}
