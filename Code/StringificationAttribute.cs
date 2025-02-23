namespace Code;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
sealed class StringificationAttribute : Attribute
{
    public StringificationAttribute(StringificationType type) => Type = type;

    public StringificationType Type { get; }
}

enum StringificationType { Out4 = 0, Even4 = 1 }