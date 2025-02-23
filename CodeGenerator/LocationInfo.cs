using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CodeGenerator;

// https://andrewlock.net/creating-a-source-generator-part-9-avoiding-performance-pitfalls-in-incremental-generators/
public record LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);
    public static LocationInfo? CreateFrom(SyntaxNode node) => CreateFrom(node.GetLocation());
    public static LocationInfo? CreateFrom(Location location) => location.SourceTree is null ?
            null :
            new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
}
