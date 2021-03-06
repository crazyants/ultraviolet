﻿using System;
using Ultraviolet.Content;
using Ultraviolet.Core;

namespace Ultraviolet.OpenGL.Graphics
{
    /// <summary>
    /// Loads fragment shader assets.
    /// </summary>
    [Preserve(AllMembers = true)]
    [ContentProcessor]
    public sealed class OpenGLFragmentShaderProcessor : ContentProcessor<String, OpenGLFragmentShader>
    {
        /// <inheritdoc/>
        public override OpenGLFragmentShader Process(ContentManager manager, IContentProcessorMetadata metadata, String input)
        {
            var source = ShaderSource.ProcessRawSource(manager, metadata, input);
            return new OpenGLFragmentShader(manager.Ultraviolet, new[] { (String)source });
        }
    }
}
