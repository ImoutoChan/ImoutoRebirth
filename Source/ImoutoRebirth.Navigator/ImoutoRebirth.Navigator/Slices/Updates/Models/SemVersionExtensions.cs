using Semver;

namespace ImoutoRebirth.Navigator.Slices.Updates.Models;

public static class SemVersionExtensions
{
    extension(SemVersion)
    {
        public static bool operator >(SemVersion left, SemVersion right)
            => left.CompareSortOrderTo(right) > 0;

        public static bool operator <(SemVersion left, SemVersion right)
            => left.CompareSortOrderTo(right) < 0;
    }
}
