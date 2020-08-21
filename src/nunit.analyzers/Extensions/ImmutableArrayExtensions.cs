using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NUnit.Analyzers.Extensions
{
    public static class ImmutableArrayExtensions
    {
        public static bool TryGetValue<TKey, TValue>(this ImmutableArray<KeyValuePair<TKey, TValue>> @this,
            TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            foreach (var keyValue in @this)
            {
                if (Equals(keyValue.Key, key))
                {
                    value = keyValue.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
