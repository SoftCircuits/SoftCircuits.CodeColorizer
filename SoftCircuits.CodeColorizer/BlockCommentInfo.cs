// Copyright (c) 2020-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

namespace SoftCircuits.CodeColorizer
{
    /// <summary>
    /// Class that describes a comment.
    /// </summary>
    public class BlockCommentInfo
    {
        /// <summary>
        /// Text that signifies the start of the comment.
        /// </summary>
        public string Start { get; set; }

        /// <summary>
        /// Text that signifies the end of the comment.
        /// </summary>
        public string End { get; set; }

        /// <summary>
        /// Constructs a new <see cref="BlockCommentInfo"/> instance.
        /// </summary>
        /// <param name="start">Text that signifies the start of a multi-line comment.</param>
        /// <param name="end">Text that signifies the end of a multi-line comment.</param>
        public BlockCommentInfo(string start, string end)
        {
            Start = start;
            End = end;
        }
    }
}
