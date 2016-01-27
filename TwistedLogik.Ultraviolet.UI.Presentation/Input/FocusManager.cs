﻿using System;
using TwistedLogik.Nucleus;
using TwistedLogik.Ultraviolet.UI.Presentation.Media;

namespace TwistedLogik.Ultraviolet.UI.Presentation.Input
{
    /// <summary>
    /// Contains attached properties relating to logical focus and focus scopes.
    /// </summary>
    [UvmlKnownType]
    public static class FocusManager
    {
        /// <summary>
        /// Gets the closest ancestor of the specified element which is a focus scope.
        /// </summary>
        /// <param name="element">The element for which to find a focus scope.</param>
        /// <returns>The closest ancestor of the specified element which is a focus scope.</returns>
        public static DependencyObject GetFocusScope(DependencyObject element)
        {
            Contract.Require(element, "element");

            if (element.GetValue<Boolean>(IsFocusScopeProperty))
                return element;

            var current = element as UIElement;
            if (current != null)
            {
                var logicalParent = LogicalTreeHelper.GetParent(current);
                if (logicalParent != null)
                {
                    return GetFocusScope(logicalParent);
                }

                var visualParent = VisualTreeHelper.GetParent(current);
                if (visualParent != null)
                {
                    return GetFocusScope(visualParent);
                }
            }

            return current;
        }

        /// <summary>
        /// Gets the element with logical focus within the specified focus scope.
        /// </summary>
        /// <param name="element">The focus scope to evaluate.</param>
        /// <returns>The element with logical focus within the specified focus scope.</returns>
        public static IInputElement GetFocusedElement(DependencyObject element)
        {
            Contract.Require(element, "element");

            return element.GetValue<IInputElement>(FocusedElementProperty);
        }

        /// <summary>
        /// Gets a value indicating whether the specified element is a focus scope.
        /// </summary>
        /// <param name="element">The element to evaluate.</param>
        /// <returns><c>true</c> if the specified element is a focus scope; otherwise, <c>false</c>.</returns>
        public static Boolean GetIsFocusScope(DependencyObject element)
        {
            Contract.Require(element, "element");

            return element.GetValue<Boolean>(IsFocusScopeProperty);
        }

        /// <summary>
        /// Sets the element with logical focus within the specified focus scope.
        /// </summary>
        /// <param name="element">The focus scope to update.</param>
        /// <param name="value">The element with logical focus within the specified focus scope.</param>
        public static void SetFocusedElement(DependencyObject element, IInputElement value)
        {
            Contract.Require(element, "element");

            element.SetValue(FocusedElementProperty, value);
        }

        /// <summary>
        /// Sets a value indicating whether the specified element is a focus scope.
        /// </summary>
        /// <param name="element">The element to evaluate.</param>
        /// <param name="value">A value indicating whether the specified element is a focus scope.</param>
        public static void SetIsFocusScope(DependencyObject element, Boolean value)
        {
            Contract.Require(element, "element");

            element.SetValue(IsFocusScopeProperty, value);
        }

        /// <summary>
        /// Adds a handler for the <see cref="E:TwistedLogik.Ultraviolet.UI.Presentation.Input.FocusManager.GotFocus"/> 
        /// attached event to the specified element.
        /// </summary>
        /// <param name="element">The element to which to add the handler.</param>
        /// <param name="handler">The handler to add to the specified element.</param>
        public static void AddGotFocusHandler(DependencyObject element, UpfRoutedEventHandler handler)
        {
            Contract.Require(element, "element");
            Contract.Require(handler, "handler");

            UIElementHelper.AddHandler(element, GotFocusEvent, handler);
        }

        /// <summary>
        /// Adds a handler for the <see cref="E:TwistedLogik.Ultraviolet.UI.Presentation.Input.FocusManager.LostFocus"/> 
        /// attached event to the specified element.
        /// </summary>
        /// <param name="element">The element to which to add the handler.</param>
        /// <param name="handler">The handler to add to the specified element.</param>
        public static void AddLostFocusHandler(DependencyObject element, UpfRoutedEventHandler handler)
        {
            Contract.Require(element, "element");
            Contract.Require(handler, "handler");

            UIElementHelper.AddHandler(element, LostFocusEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the <see cref="E:TwistedLogik.Ultraviolet.UI.Presentation.Input.FocusManager.GotFocus"/> 
        /// attached event from the specified element.
        /// </summary>
        /// <param name="element">The element from which to remove the handler.</param>
        /// <param name="handler">The handler to remove from the specified element.</param>
        public static void RemoveGotFocusHandler(DependencyObject element, UpfRoutedEventHandler handler)
        {
            Contract.Require(element, "element");
            Contract.Require(handler, "handler");

            UIElementHelper.RemoveHandler(element, GotFocusEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the <see cref="E:TwistedLogik.Ultraviolet.UI.Presentation.Input.FocusManager.LostFocus"/> 
        /// attached event from the specified element.
        /// </summary>
        /// <param name="element">The element from which to remove the handler.</param>
        /// <param name="handler">The handler to remove from the specified element.</param>
        public static void RemoveLostFocusHandler(DependencyObject element, UpfRoutedEventHandler handler)
        {
            Contract.Require(element, "element");
            Contract.Require(handler, "handler");

            UIElementHelper.RemoveHandler(element, LostFocusEvent, handler);
        }

        /// <summary>
        /// Identifies the <see cref="P:TwistedLogik.Ultraviolet.UI.Presentation.Input.FocusManager.FocusedElement"/> attached property.
        /// </summary>
        /// <value>The identifier for the <see cref="P:TwistedLogik.Ultraviolet.UI.Presentation.Input.FocusManager.FocusedElement"/> attached property.</value>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets the element with logical focus within its focus scope.
        /// </summary>
        /// <value>The <see cref="IInputElement"/> that currently has logical focus within the focus scope. The
        /// default value is <see langword="null"/>.</value>
        /// <remarks>
        /// <dprop>
        ///     <dpropField><see cref="FocusedElementProperty"/></dpropField>
        ///     <dpropStylingName>focused-element</dpropStylingName>
        ///     <dpropMetadata>None</dpropMetadata>
        /// </dprop>
        /// </remarks>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty FocusedElementProperty = DependencyProperty.RegisterAttached("FocusedElement", typeof(IInputElement), typeof(FocusManager),
            new PropertyMetadata<IInputElement>(null, PropertyMetadataOptions.None));

        /// <summary>
        /// Identifies the <see cref="P:TwistedLogik.Ultraviolet.UI.Presentation.Input.FocusManager.IsFocusScope"/> attached property.
        /// </summary>
        /// <value>The identifier for the <see cref="P:TwistedLogik.Ultraviolet.UI.Presentation.Input.FocusManager.IsFocusScope"/> attached property.</value>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets a value indicating whether the specified element is a focus scope.
        /// </summary>
        /// <value><see langword="true"/> if the element is a focus scope; otherwise, <see langword="false"/>. The default
        /// value is <see langword="false"/>.</value>
        /// <remarks>
        /// <dprop>
        ///     <dpropField><see cref="IsFocusScopeProperty"/></dpropField>
        ///     <dpropStylingName>focus-scope</dpropStylingName>
        ///     <dpropMetadata>None</dpropMetadata>
        /// </dprop>
        /// </remarks>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty IsFocusScopeProperty = DependencyProperty.RegisterAttached("IsFocusScope", typeof(Boolean), typeof(FocusManager),
            new PropertyMetadata<Boolean>(CommonBoxedValues.Boolean.False, PropertyMetadataOptions.None));
        
        /// <summary>
        /// Identifies the <see cref="E:TwistedLogik.Ultraviolet.UI.Presentation.Input.FocusManager.GotFocus"/> 
        /// attached routed event.
        /// </summary>
        /// <value>The identifier of the <see cref="E:TwistedLogik.Ultraviolet.UI.Presentation.Input.FocusManager.GotFocus"/>
        /// attached routed event.</value>
        /// <AttachedEventComments>
        /// <summary>
        /// Occurs when an element receives logical focus.
        /// </summary>
        /// <remarks>
        /// <revt>
        ///     <revtField><see cref="GotFocusEvent"/></revtField>
        ///     <revtStylingName>got-focus</revtStylingName>
        ///     <revtStrategy>Bubbling</revtStrategy>
        ///     <revtDelegate><see cref="UpfRoutedEventHandler"/></revtDelegate>
        /// </revt>
        /// </remarks>
        /// </AttachedEventComments>
        public static readonly RoutedEvent GotFocusEvent = EventManager.RegisterRoutedEvent("GotFocus", RoutingStrategy.Bubble, 
            typeof(UpfRoutedEventHandler), typeof(FocusManager));

        /// <summary>
        /// Identifies the <see cref="E:TwistedLogik.Ultraviolet.UI.Presentation.Input.FocusManager.LostFocus"/> 
        /// attached routed event.
        /// </summary>
        /// <value>The identifier of the <see cref="E:TwistedLogik.Ultraviolet.UI.Presentation.Input.FocusManager.LostFocus"/>
        /// attached routed event.</value>
        /// <AttachedEventComments>
        /// <summary>
        /// Occurs when an element loses logical focus.
        /// </summary>
        /// <remarks>
        /// <revt>
        ///     <revtField><see cref="LostFocusEvent"/></revtField>
        ///     <revtStylingName>lost-focus</revtStylingName>
        ///     <revtStrategy>Bubbling</revtStrategy>
        ///     <revtDelegate><see cref="UpfRoutedEventHandler"/></revtDelegate>
        /// </revt>
        /// </remarks>
        /// </AttachedEventComments>
        public static readonly RoutedEvent LostFocusEvent = EventManager.RegisterRoutedEvent("LostFocus", RoutingStrategy.Bubble,
            typeof(UpfRoutedEventHandler), typeof(FocusManager));
    }
}
