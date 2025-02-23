using System.Collections.Generic;
using System;
using System.Linq;

namespace CodeGenerator;

public class EquatableList<T> : List<T>, IEquatable<EquatableList<T>>
{
    public EquatableList(IEnumerable<T> items) => AddRange(items);
    public bool Equals(EquatableList<T>? other) => this.SequenceEqual(other);
    public override bool Equals(object obj) => Equals(obj as EquatableList<T>);
    public override int GetHashCode()
    {
        var hc = new HashCode();
        foreach (var item in this)
            hc.Add(item);
        return hc.ToHashCode();
    }
    public static bool operator ==(EquatableList<T> list1, EquatableList<T> list2) =>
        ReferenceEquals(list1, list2) || list1 is not null && list2 is not null && list1.Equals(list2);
    public static bool operator !=(EquatableList<T> list1, EquatableList<T> list2) => !(list1 == list2);
}