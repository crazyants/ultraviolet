﻿using System;
using System.Runtime.InteropServices;
using Ultraviolet.Core;

namespace Ultraviolet.BASS.Native
{
    [Preserve(AllMembers = true)]
    [StructLayout(LayoutKind.Sequential)]
    internal struct BASS_CHANNELINFO
    {
        public UInt32 freq;
        public UInt32 chans;
        public UInt32 flags;
        public UInt32 ctype;
        public UInt32 origres;
        public UInt32 plugin;
        public UInt32 sample;
        public IntPtr filename;
    }
}
