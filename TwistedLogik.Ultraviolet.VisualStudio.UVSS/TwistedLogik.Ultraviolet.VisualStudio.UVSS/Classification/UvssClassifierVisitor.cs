﻿using Microsoft.VisualStudio.Text.Classification;
using System;
using TwistedLogik.Ultraviolet.UI.Presentation.Uvss;
using TwistedLogik.Ultraviolet.UI.Presentation.Uvss.Syntax;

namespace TwistedLogik.Ultraviolet.VisualStudio.UVSS.Classification
{
    /// <summary>
    /// Represents a method which is invoked when <see cref="UvssClassifierVisitor"/> marks
    /// a span of text for classification.
    /// </summary>
    /// <param name="start">The index of the first character in the span.</param>
    /// <param name="width">The number of characters in the span.</param>
    /// <param name="type">The classification type assigned to the span.</param>
    public delegate void ClassifierAction(Int32 start, Int32 width, IClassificationType type);

    /// <summary>
    /// Represents a syntax tree visitor which provides classification spans.
    /// </summary>
    public class UvssClassifierVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UvssClassifierVisitor"/> class.
        /// </summary>
        /// <param name="registry">The classification type registry service.</param>
        /// <param name="classifier">The action which is called when a span is classified.</param>
        public UvssClassifierVisitor(IClassificationTypeRegistryService registry, ClassifierAction classifier)
        {
            this.typeUvssComment = registry.GetClassificationType("UvssComment");
            this.typeUvssNumber = registry.GetClassificationType("UvssNumber");
            this.typeUvssKeyword = registry.GetClassificationType("UvssKeyword");
            this.typeUvssSelector = registry.GetClassificationType("UvssSelector");
            this.typeUvssPropertyName = registry.GetClassificationType("UvssPropertyName");
            this.typeUvssPropertyValue = registry.GetClassificationType("UvssPropertyValue");
            this.typeUvssStoryboard = registry.GetClassificationType("UvssStoryboard");
            this.typeUvssTypeName = registry.GetClassificationType("UvssTypeName");

            this.classifier = classifier;
        }

        /// <summary>
        /// Visits the specified syntax node.
        /// </summary>
        /// <param name="node">The syntax node to visit.</param>
        public void Visit(SyntaxNode node)
        {
            if (node == null)
                return;

            Visit(node.GetLeadingTrivia());

            switch (node.Kind)
            {
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                    VisitCommentTrivia((StructurelessSyntaxTrivia)node);
                    break;

                case SyntaxKind.NumberToken:
                    VisitNumber((SyntaxToken)node);
                    break;

                case SyntaxKind.AnimationKeyword:
                case SyntaxKind.AsKeyword:
                case SyntaxKind.EventKeyword:
                case SyntaxKind.HandledKeyword:
                case SyntaxKind.ImportantKeyword:
                case SyntaxKind.KeyframeKeyword:
                case SyntaxKind.PlaySfxKeyword:
                case SyntaxKind.PlayStoryboardKeyword:
                case SyntaxKind.PropertyKeyword:
                case SyntaxKind.SetHandledKeyword:
                case SyntaxKind.SetKeyword:
                case SyntaxKind.TargetKeyword:
                case SyntaxKind.TransitionKeyword:
                case SyntaxKind.TriggerKeyword:
                    VisitKeyword((SyntaxToken)node);
                    break;

                case SyntaxKind.Selector:
                    VisitSelector((UvssSelectorSyntax)node);
                    break;

                case SyntaxKind.Rule:
                    VisitRule((UvssRuleSyntax)node);
                    break;

                case SyntaxKind.EventName:
                    VisitEventName((UvssEventNameSyntax)node);
                    break;

                case SyntaxKind.PropertyName:
                    VisitPropertyName((UvssPropertyNameSyntax)node);
                    break;
                    
                case SyntaxKind.PropertyValueToken:
                    VisitPropertyValueToken((SyntaxToken)node);
                    break;

                case SyntaxKind.Storyboard:
                    VisitStoryboard((UvssStoryboardSyntax)node);
                    break;

                case SyntaxKind.StoryboardTarget:
                    VisitStoryboardTarget((UvssStoryboardTargetSyntax)node);
                    break;

                case SyntaxKind.AnimationKeyframe:
                    VisitAnimationKeyframe((UvssAnimationKeyframeSyntax)node);
                    break;

                case SyntaxKind.NavigationExpression:
                    VisitNavigationExpression((UvssNavigationExpressionSyntax)node);
                    break;
            }

            for (int i = 0; i < node.SlotCount; i++)
            {
                var child = node.GetSlot(i);
                if (child != null)
                {
                    Visit(child);
                }
            }

            Visit(node.GetTrailingTrivia());
        }

        /// <summary>
        /// Visits a comment trivia node.
        /// </summary>
        /// <param name="trivia">The trivia node to visit.</param>
        private void VisitCommentTrivia(StructurelessSyntaxTrivia trivia)
        {
            Style(trivia, typeUvssComment);
        }

        /// <summary>
        /// Visits a number node.
        /// </summary>
        /// <param name="token">The number node to visit.</param>
        private void VisitNumber(SyntaxToken token)
        {
            Style(token, typeUvssNumber);
        }

        /// <summary>
        /// Visits a keyword token node.
        /// </summary>
        /// <param name="token">The token node to visit.</param>
        private void VisitKeyword(SyntaxToken token)
        {
            Style(token, typeUvssKeyword);
        }

        /// <summary>
        /// Visits a selector node.
        /// </summary>
        /// <param name="selector">The selector node to visit.</param>
        private void VisitSelector(UvssSelectorSyntax selector)
        {
            for (int i = 0; i < selector.Components.Count; i++)
            {
                var component = selector.Components[i];
                Style(component, typeUvssSelector);
            }
        }

        /// <summary>
        /// Visits a rule node.
        /// </summary>
        /// <param name="rule">The rule node to visit.</param>
        private void VisitRule(UvssRuleSyntax rule)
        {
            Style(rule.ColonToken, typeUvssPropertyName);
        }

        /// <summary>
        /// Visits an event name node.
        /// </summary>
        /// <param name="eventName">The event name node to visit.</param>
        private void VisitEventName(UvssEventNameSyntax eventName)
        {
            Style(eventName.AttachedEventOwnerNameIdentifier, typeUvssTypeName);
            Style(eventName.EventNameIdentifier, typeUvssPropertyName);
        }

        /// <summary>
        /// Visits a property name node.
        /// </summary>
        /// <param name="propertyName">The property name node to visit.</param>
        private void VisitPropertyName(UvssPropertyNameSyntax propertyName)
        {
            Style(propertyName.AttachedPropertyOwnerNameIdentifier, typeUvssTypeName);
            Style(propertyName.PropertyNameIdentifier, typeUvssPropertyName);
        }

        /// <summary>
        /// Visits a property value token.
        /// </summary>
        /// <param name="propertyValueToken">The property value token to visit.</param>
        private void VisitPropertyValueToken(SyntaxToken propertyValueToken)
        {
            Style(propertyValueToken, typeUvssPropertyValue);
        }

        /// <summary>
        /// Visits a storyboard declaration node.
        /// </summary>
        /// <param name="storyboard">The storyboard declaration node to visit.</param>
        private void VisitStoryboard(UvssStoryboardSyntax storyboard)
        {
            Style(storyboard.AtSignToken, typeUvssStoryboard);
            Style(storyboard.NameIdentifier, typeUvssStoryboard);
            Style(storyboard.LoopIdentifier, typeUvssStoryboard);
        }

        /// <summary>
        /// Visits a storyboard target node.
        /// </summary>
        /// <param name="storyboardTarget">The storyboard target node to visit.</param>
        private void VisitStoryboardTarget(UvssStoryboardTargetSyntax storyboardTarget)
        {
            Style(storyboardTarget.TypeNameIdentifier, typeUvssTypeName);
        }

        /// <summary>
        /// Visits an animation keyframe declaration node..
        /// </summary>
        /// <param name="animationKeyframe">The animation keyframe declaration node to visit.</param>
        private void VisitAnimationKeyframe(UvssAnimationKeyframeSyntax animationKeyframe)
        {
            Style(animationKeyframe.EasingIdentifier, typeUvssKeyword);
        }

        /// <summary>
        /// Visits a navigation expression node.
        /// </summary>
        /// <param name="navigationExpression">The navigation expression node to visit.</param>
        private void VisitNavigationExpression(UvssNavigationExpressionSyntax navigationExpression)
        {
            Style(navigationExpression.TypeNameIdentifier, typeUvssTypeName);
        }

        /// <summary>
        /// Styles the specified node.
        /// </summary>
        /// <param name="node">The node to style.</param>
        /// <param name="type">The classification type to apply to the node.</param>
        /// <param name="withLeadingTrivia">A value indicating whether to style the node's leading trivia.</param>
        /// <param name="withTrailingTrivia">A value indicating whether to style the node's trailing trivia.</param>
        private void Style(SyntaxNode node, IClassificationType type,
            Boolean withLeadingTrivia = false,
            Boolean withTrailingTrivia = false)
        {
            if (node == null || node.IsMissing || type == null)
                return;

            var start = node.Position + (withLeadingTrivia ? 0 : node.GetLeadingTriviaWidth());
            var width = node.FullWidth - (
                (withLeadingTrivia ? 0 : node.GetLeadingTriviaWidth()) +
                (withTrailingTrivia ? 0 : node.GetTrailingTriviaWidth()));

            classifier(start, width, type);
        }

        // State values.
        private readonly IClassificationType typeUvssComment;
        private readonly IClassificationType typeUvssNumber;
        private readonly IClassificationType typeUvssKeyword;
        private readonly IClassificationType typeUvssSelector;
        private readonly IClassificationType typeUvssPropertyName;
        private readonly IClassificationType typeUvssPropertyValue;
        private readonly IClassificationType typeUvssStoryboard;
        private readonly IClassificationType typeUvssTypeName;
        private readonly ClassifierAction classifier;
    }
}
