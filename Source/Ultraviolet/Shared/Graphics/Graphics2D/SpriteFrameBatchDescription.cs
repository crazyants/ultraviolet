﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Ultraviolet.Core;

namespace Ultraviolet.Graphics.Graphics2D
{
    /// <summary>
    /// An intermediate representation of a related batch of frames within a sprite animation.
    /// </summary>
    [Preserve(AllMembers = true)]
    internal sealed class SpriteFrameBatchDescription
    {
        /// <summary>
        /// Gets the frame batch's default values for frames.
        /// </summary>
        [JsonProperty(PropertyName = "frameDefaults")]
        public SpriteFrameDefaultsDescription FrameDefaults { get; set; }
        
        /// <summary>
        /// Gets the frame batch's items.
        /// </summary>
        [JsonProperty(PropertyName = "items", Required = Required.Always)]
        public IEnumerable<SpriteFrameDescription> Items { get; set; }
    }
}
