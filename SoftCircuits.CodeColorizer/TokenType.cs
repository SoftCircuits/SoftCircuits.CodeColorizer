// Copyright (c) 2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

namespace SoftCircuits.CodeColorizer
{
    /// <summary>
    /// Language token classifications.
    /// </summary>
    internal enum TokenType
    {
        Keyword,
        Symbol,
        String,
        Operator,
        Comment,
        EndOfText,
        Unclassified,
    }
}
