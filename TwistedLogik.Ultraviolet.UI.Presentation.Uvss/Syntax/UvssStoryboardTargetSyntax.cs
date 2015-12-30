﻿using System;

namespace TwistedLogik.Ultraviolet.UI.Presentation.Uvss.Syntax
{
    /// <summary>
    /// Represents a UVSS storyboard target.
    /// </summary>
    public sealed class UvssStoryboardTargetSyntax : UvssNodeSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UvssStoryboardTargetSyntax"/> class.
        /// </summary>
        internal UvssStoryboardTargetSyntax(
            SyntaxToken targetKeyword,
            SyntaxToken typeNameToken,
            UvssSelectorWithParenthesesSyntax selector,
            UvssBlockSyntax body)
            : base(SyntaxKind.StoryboardTarget)
        {
            this.TargetKeyword = targetKeyword;
            ChangeParent(targetKeyword);

            this.TypeNameToken = typeNameToken;
            ChangeParent(typeNameToken);

            this.Selector = selector;
            ChangeParent(selector);

            this.Body = body;
            ChangeParent(body);

            SlotCount = 4;
        }

        /// <inheritdoc/>
        public override SyntaxNode GetSlot(Int32 index)
        {
            switch (index)
            {
                case 0: return TargetKeyword;
                case 1: return TypeNameToken;
                case 2: return Selector;
                case 3: return Body;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// The target's "target" keyword.
        /// </summary>
        public SyntaxToken TargetKeyword { get; internal set; }

        /// <summary>
        /// The target's optional type name.
        /// </summary>
        public SyntaxToken TypeNameToken { get; internal set; }

        /// <summary>
        /// The target's selector.
        /// </summary>
        public UvssSelectorWithParenthesesSyntax Selector { get; internal set; }

        /// <summary>
        /// The target's body.
        /// </summary>
        public UvssBlockSyntax Body { get; internal set; }
    }
}
