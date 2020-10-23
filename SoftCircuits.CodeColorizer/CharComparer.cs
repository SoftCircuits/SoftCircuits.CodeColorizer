// Copyright (c) 2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SoftCircuits.CodeColorizer
{
    internal abstract class CharComparer : IEqualityComparer<char>
    {
        public abstract bool Equals(char x, char y);
        public abstract int GetHashCode([DisallowNull] char obj);
        public static CharComparer Ordinal => new OrdinalComparer();
        public static CharComparer OrdinalIgnoreCase => new OrdinalIgnoreCaseComparer();
    }

    internal sealed class OrdinalComparer : CharComparer
    {
        public override bool Equals(char x, char y) => x == y;
        public override int GetHashCode([DisallowNull] char obj) => obj;
    }

    internal sealed class OrdinalIgnoreCaseComparer : CharComparer
    {
        public override bool Equals(char x, char y) => char.ToUpper(x) == char.ToUpper(y);
        public override int GetHashCode([DisallowNull] char obj) => char.ToUpper(obj);
    }
}
