#if NETSTANDARD1_6
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis
{
    public sealed class SymbolEqualityComparer : IEqualityComparer<ISymbol?>
    {
        public static readonly SymbolEqualityComparer Default = new SymbolEqualityComparer();
        public static readonly SymbolEqualityComparer IncludeNullabilty = new SymbolEqualityComparer();

        public bool Equals(ISymbol? x, ISymbol? y)
        {
            if (x is null)
            {
                return y is null;
            }

#pragma warning disable RS1024 // Compare symbols correctly
            return x.Equals(y);
#pragma warning restore RS1024 // Compare symbols correctly
        }

        public int GetHashCode(ISymbol? obj)
        {
#pragma warning disable RS1024 // Compare symbols correctly
            return obj?.GetHashCode() ?? 0;
#pragma warning restore RS1024 // Compare symbols correctly
        }
    }
}

#endif
