namespace Code;

public partial class MyClass
{
    [Stringification(StringificationType.Out4)]
    public decimal Property1 { get; set; }

    [Stringification(StringificationType.Even4)]
    public decimal Property2 { get; set; }
}
