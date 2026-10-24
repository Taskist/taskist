using FluentValidation;

namespace Taskist.Web.Helpers.Extensions;

public static class RuleBuilderOptionsExtension
{
    // NOTE: registered as SYNCHRONOUS rules. ASP.NET's FluentValidation auto-validation
    // pipeline is synchronous and throws if a validator contains async rules, so the
    // (short, single-lookup) predicates are run synchronously here.
    public static IRuleBuilderOptions<T, TProperty> MustAwait<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder,
        Func<T, TProperty, Task<bool>> predicate)
    {
        return ruleBuilder.Must((x, value) => predicate(x, value).GetAwaiter().GetResult());
    }

    public static IRuleBuilderOptions<T, TProperty> WhenAwait<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule,
            Func<T, Task<bool>> predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators)
    {
        return rule.When(x => predicate(x).GetAwaiter().GetResult(), applyConditionTo);
    }
}