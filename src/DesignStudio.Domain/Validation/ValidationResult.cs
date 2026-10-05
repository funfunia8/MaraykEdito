namespace DesignStudio.Domain.Validation;

public enum ValidationSeverity
{
    Valid,
    Warning,
    Error
}

public sealed record ValidationIssue(ValidationSeverity Severity, string Code, string Message);

public sealed class ValidationResult
{
    private readonly List<ValidationIssue> _issues = new();

    public IReadOnlyList<ValidationIssue> Issues => _issues;
    public bool IsValid => _issues.All(x => x.Severity != ValidationSeverity.Error);

    public void Add(ValidationSeverity severity, string code, string message)
        => _issues.Add(new ValidationIssue(severity, code, message));

    public void Error(string code, string message) => Add(ValidationSeverity.Error, code, message);
    public void Warning(string code, string message) => Add(ValidationSeverity.Warning, code, message);
}
