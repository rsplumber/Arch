using System.Reflection;

namespace Core;

public abstract class Enumeration : IComparable
{
    public int Id { get; }

    public string Name { get; private set; }

    protected Enumeration(int id, string name) => (Id, Name) = (id, name);

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
        typeof(T).GetFields(BindingFlags.Public |
                            BindingFlags.Static |
                            BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();

    protected bool Equals(Enumeration other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Enumeration) obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }

    public int CompareTo(object? other) => other is null ? int.MinValue : Id.CompareTo(((Enumeration) other).Id);
}