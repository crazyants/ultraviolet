﻿using System;
using System.Runtime.InteropServices;
using Ultraviolet.Core;

#pragma warning disable 1591

namespace Ultraviolet.SDL2.Native
{
    [Preserve]
    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_SysWMinfo_win
    {
        public IntPtr window;
        public IntPtr hdc;
    }
}
