# Rule evaluation service

`RuleEvaluationService` evaluates a feature flag's targeting rules against a
`UserContext`. It loads the current flag and its rules from
`IFeatureFlagRepository`, evaluates active rules from highest to lowest priority,
and enables the flag as soon as one rule matches.

Source: `src/FeatureFlags/Services/RuleEvaluationService.cs`.

## Overall flag evaluation

`EvaluateAsync(FeatureFlag featureFlag, UserContext userContext,
CancellationToken cancellationToken = default)` follows this sequence:

1. It rejects a null `featureFlag` or `userContext` with
   `ArgumentNullException`. These checks happen before the method's `try` block.
2. It reloads the flag by `featureFlag.Id` with
   `IFeatureFlagRepository.GetWithRulesAsync`. Evaluation therefore uses the
   repository result, not the `Rules` collection on the argument.
3. If the repository returns null, it throws `FeatureFlagNotFoundException`
   using the key from the argument.
4. If the loaded flag has no rules, it returns `false`.
5. It keeps only rules whose `IsActive` property is `true`, then orders them by
   descending `Priority`. If none remain, it returns `false`.
6. It calls `EvaluateRuleAsync` for each rule in that order. The first matching
   rule causes an immediate `true` result; lower-priority rules are not
   evaluated.
7. It returns `false` if no active rule matches.

This gives the collection of rules OR semantics: **any matching active rule
enables the flag**. Priority controls evaluation order and short-circuiting; it
does not change the result when all evaluations are deterministic.

All exceptions raised inside the `try` block—including the not-found exception
and errors from the repository or rule evaluation—are logged and wrapped in a
`RuleEvaluationException`. The original exception is available as its inner
exception. The initial null-argument exceptions are not wrapped.

## Single-rule evaluation

`EvaluateRuleAsync(Rule rule, UserContext userContext,
CancellationToken cancellationToken = default)` first validates both arguments.
It then returns `false` when:

- the rule is inactive;
- the rule has no conditions; or
- the rule has conditions, but none are active.

Only active conditions are evaluated. The service evaluates every active
condition into a result list before combining the results; condition evaluation
does not short-circuit.

The results are combined as follows:

| `ConditionLogic` | Result |
| --- | --- |
| `"AND"`, ignoring case | `true` only when every active condition matches |
| Any other value | `true` when at least one active condition matches |

Consequently, `"OR"` is not checked explicitly: misspelled, empty, or otherwise
unexpected values also receive OR behavior. Model validation may reject those
values elsewhere, but this service does not validate the rule.

Although this method returns `Task<bool>`, its current body performs no
asynchronous work.

## Condition evaluation

`EvaluateCondition(Condition condition, UserContext userContext)` rejects null
arguments and immediately returns `false` for an inactive condition. For an
active condition it:

1. obtains the context value with
   `userContext.GetAttribute(condition.AttributeName)`; and
2. passes that value to `condition.Evaluate`.

Standard attribute names (`userid`, `email`, `country`, `tier`, and `region`)
are selected case-insensitively by `UserContext`. Other names are looked up in
`CustomAttributes` with the dictionary's configured comparer (case-sensitive by
default).

`Condition.Evaluate` returns `false` for a missing/null context value. It owns
the operator behavior, including case-insensitive string comparison for
`Equals`, `NotEquals`, `Contains`, `StartsWith`, `EndsWith`, and `In`, plus
invariant-culture numeric parsing for `GreaterThan` and `LessThan`.

Any exception while retrieving or evaluating an active condition is logged as a
warning and converted to `false`. The null-argument checks and inactive check
occur outside that exception handler.

## Finding all matching rules

`GetApplicableRulesAsync(FeatureFlag featureFlag, UserContext userContext)` is
similar to the main evaluation path, but returns every matching active rule:

- it rejects null arguments;
- reloads the flag and rules by `featureFlag.Id`;
- throws `FeatureFlagNotFoundException` directly if the flag is absent;
- considers active rules in descending priority order; and
- evaluates all of them, adding each match to the returned list.

Unlike `EvaluateAsync`, this method does not stop after the first match and does
not wrap exceptions in `RuleEvaluationException`. An empty rule collection
naturally produces an empty result.

## Cancellation and logging

The public concrete overloads of `EvaluateAsync` and `EvaluateRuleAsync` accept
a `CancellationToken`, but the current implementation does not pass it to the
repository, inspect it, or forward it to nested evaluations. Cancellation
therefore has no effect. The explicit `IRuleEvaluationService` implementations
call these overloads without a token because the interface does not expose one.

The service logs the start and outcome of overall evaluations, the start and
computed outcome of rule evaluations, warnings for inactive or empty rules, and
condition failures. One nuance is that the rule-completion log always records
`results.All(...)`; for non-AND rules, this logged value can differ from the
actual OR-style return value.

## Example

Given two active rules:

- priority 20: `country Equals US` **AND** `tier Equals premium`;
- priority 10: `region Equals beta`;

the service tests the priority-20 rule first. A US premium user enables the flag
immediately. If that rule fails, a user whose region is `beta` enables it through
the priority-10 rule. If neither rule matches, evaluation returns `false`.
