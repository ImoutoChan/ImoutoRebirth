using FluentAssertions.Collections;

namespace ImoutoRebirth.Common.Tests;

public static class FluentAssertionsExtensions
{
    public static void ContainAll<T>(this GenericCollectionAssertions<T> constraint, IEnumerable<T> expected)
    {
        foreach (var expectedItem in expected) 
            constraint.Contain(expectedItem);
    }

    public static void NotContainAll<T>(this GenericCollectionAssertions<T> constraint, IEnumerable<T> expected)
    {
        foreach (var expectedItem in expected) 
            constraint.NotContain(expectedItem);
    }

    public static void ContainAll(this StringCollectionAssertions constraint, IEnumerable<string> expected)
    {
        foreach (var expectedItem in expected) 
            constraint.Contain(expectedItem);
    }

    public static void NotContainAll(this StringCollectionAssertions constraint, IEnumerable<string> expected)
    {
        foreach (var expectedItem in expected) 
            constraint.NotContain(expectedItem);
    }
}
