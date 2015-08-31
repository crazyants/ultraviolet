﻿using System;
using System.Collections.Generic;
using TwistedLogik.Nucleus;
using TwistedLogik.Nucleus.Collections;
using TwistedLogik.Ultraviolet.Graphics;
using TwistedLogik.Ultraviolet.Graphics.Graphics2D;

namespace TwistedLogik.Ultraviolet.UI.Presentation
{
    /// <summary>
    /// Contains methods for rendering UI elements out-of-band, that is, prior to rendering the rest of the
    /// visual tree. This is necessary in order to properly render arbitrarily transformed elements.
    /// </summary>
    internal sealed partial class OutOfBandRenderer : UltravioletResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutOfBandRenderer"/> class.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        public OutOfBandRenderer(UltravioletContext uv)
            : base(uv)
        {
            this.renderTargetPool = new ExpandingPool<OutOfBandRenderTarget>(4, 32, () =>
            {
                return new OutOfBandRenderTarget(uv);
            });

            this.spriteBatch = SpriteBatch.Create();

            this.drawingContext = new DrawingContext();
            this.drawingContext.SpriteBatch = spriteBatch;
        }

        /// <summary>
        /// Gets a value indicating whether the specified element is rendered out-of-band.
        /// </summary>
        /// <param name="element">The element to evaluate.</param>
        /// <returns><c>true</c> if the specified element is rendered out-of-band; otherwise, <c>false</c>.</returns>
        public Boolean IsRenderedOutOfBand(UIElement element)
        {
            Contract.Require(element, "element");
            Contract.EnsureNotDisposed(this, Disposed);

            return registeredElements.ContainsKey(element);
        }

        /// <summary>
        /// Gets a value indicating whether the specified element's texture is ready to be used.
        /// </summary>
        /// <param name="element">The element to evaluate.</param>
        /// <returns><c>true</c> if the specified element's texture is ready; otherwise, <c>false</c>.</returns>
        public Boolean IsTextureReady(UIElement element)
        {
            OutOfBandRenderTarget rtarget;
            if (registeredElements.TryGetValue(element, out rtarget))
            {
                return rtarget.IsReady;
            }
            return false;
        }

        /// <summary>
        /// Registers an element with the out-of-band renderer.
        /// </summary>
        /// <param name="element">The element to register.</param>
        public void Register(UIElement element)
        {
            Contract.Require(element, "element");
            Contract.EnsureNotDisposed(this, Disposed);

            if (IsRenderedOutOfBand(element))
                return;
            
            var target = renderTargetPool.Retrieve();
            var bounds = default(RectangleD);
            target.ResizeForElement(element, out bounds);

            registeredElements.Add(element, target);
        }

        /// <summary>
        /// Unregisters an element from the out-of-band renderer.
        /// </summary>
        /// <param name="element">The element to unregister.</param>
        public void Unregister(UIElement element)
        {
            Contract.Require(element, "element");
            Contract.EnsureNotDisposed(this, Disposed);

            OutOfBandRenderTarget rt;
            if (registeredElements.TryGetValue(element, out rt))
            {
                rt.Resize(1, 1);
                renderTargetPool.Release(rt);
                registeredElements.Remove(element);
            }
        }

        /// <summary>
        /// Draws out-of-band elements to their render buffers.
        /// </summary>
        /// <param name="time">Time elapsed since the last call to <see cref="UltravioletContext.Draw(UltravioletTime)"/>.</param>
        public void DrawRenderTargets(UltravioletTime time)
        {
            Contract.EnsureNotDisposed(this, Disposed);

            var graphics = Ultraviolet.GetGraphics();

            try
            {
                isDrawingRenderTargets = true;

                foreach (var kvp in registeredElements)
                    kvp.Value.IsReady = false;

                foreach (var kvp in registeredElements)
                {
                    var element = kvp.Key;
                    var rtarget = kvp.Value;
                    
                    if (element.TransformedVisualBounds.IsEmpty)
                        continue;

                    var bounds = default(RectangleD);
                    rtarget.ResizeForElement(element, out bounds);
                    
                    graphics.SetRenderTarget(rtarget.RenderTarget);
                    graphics.Clear(Color.Transparent);

                    drawingContext.Reset(element.View.Display);
                    drawingContext.SpriteBatch = spriteBatch;

                    var centerX = rtarget.RenderTarget.Width / 2f;
                    var centerY = rtarget.RenderTarget.Height / 2f;

                    var display = element.View.Display;
                    
                    var pxOffsetX = -(Int32)display.DipsToPixels(bounds.X);
                    var pxOffsetY = -(Int32)display.DipsToPixels(bounds.Y);

                    var translate = new Vector2(pxOffsetX, pxOffsetY);
                    var transform = Matrix.CreateTranslation(translate.X, translate.Y, 0);

                    drawingContext.Begin(SpriteSortMode.Deferred, null, transform);
                    element.Draw(time, drawingContext);
                    drawingContext.End();

                    rtarget.VisualBounds = bounds;
                    rtarget.IsReady = true;
                }
            }
            finally
            {
                isDrawingRenderTargets = false;
            }

            graphics.SetRenderTarget(null);
            graphics.Clear(Color.Transparent);
        }
        
        /// <summary>
        /// Gets the render target that represents the specified element.
        /// </summary>
        /// <param name="element">The element for which to retrieve a render target.</param>
        /// <returns>The render target associated with the specified out-of-band element, or <c>null if the element is
        /// not registered for out-of-band rendering.</c></returns>
        public OutOfBandRenderTarget GetElementRenderTarget(UIElement element)
        {
            Contract.Require(element, "element");
            Contract.EnsureNotDisposed(this, Disposed);

            OutOfBandRenderTarget buffer;
            if (registeredElements.TryGetValue(element, out buffer))
            {
                return buffer;
            }
            return null;
        }

        /// <summary>
        /// Gets a value indicating whether the out-of-band renderer is currently in use (that is,
        /// whether it currently has any registered elements).
        /// </summary>
        public Boolean IsCurrentlyInUse
        {
            get
            {
                Contract.EnsureNotDisposed(this, Disposed);

                return registeredElements.Count > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the out-of-band renderer is in the process of drawing its render targets.
        /// </summary>
        public Boolean IsDrawingRenderTargets
        {
            get
            {
                Contract.EnsureNotDisposed(this, Disposed);

                return isDrawingRenderTargets;
            }
        }
       
        /// <inheritdoc/>
        protected override void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                SafeDispose.Dispose(spriteBatch);

                foreach (var kvp in registeredElements)
                {
                    kvp.Value.Dispose();
                }
                registeredElements.Clear();
            }
            base.Dispose(disposing);
        }

        // The pool of available render buffers.
        private readonly IPool<OutOfBandRenderTarget> renderTargetPool;

        // The collection of registered elements and their render buffers.
        private readonly SortedDictionary<UIElement, OutOfBandRenderTarget> registeredElements =
            new SortedDictionary<UIElement, OutOfBandRenderTarget>(new UIElementComparer());

        // The drawing context used to render elements.
        private readonly DrawingContext drawingContext;
        private readonly SpriteBatch spriteBatch;

        // Property values.
        private bool isDrawingRenderTargets;
    }
}