﻿using System;

namespace Ultraviolet.Graphics
{
    /// <summary>
    /// Represents a dummy implementation of <see cref="GraphicsCapabilities"/>.
    /// </summary>
    public sealed class DummyGraphicsCapabilities : GraphicsCapabilities
    {
        /// <inheritdoc/>
        public override Boolean FlippedTextures
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override Boolean SupportsDepthStencilTextures
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override Boolean SupportsInstancedRendering
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override Boolean SupportsNonZeroBaseInstance
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override Boolean SupportsPreservingRenderTargetContentInHardware
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override Boolean SupportsIndependentSamplerState
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override Boolean SupportsIntegerVertexAttributes
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override Boolean SupportsDoublePrecisionVertexAttributes
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override Int32 MaximumTextureSize
        {
            get { return 0; }
        }

        /// <inheritdoc/>
        public override Int32 MaximumViewportHeight
        {
            get { return 0; }
        }

        /// <inheritdoc/>
        public override Int32 MaximumViewportWidth
        {
            get { return 0; }
        }
    }
}
