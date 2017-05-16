﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Ultraviolet.Core;
using Ultraviolet.Core.Text;
using Ultraviolet.Platform;
using Ultraviolet.SDL2.Native;

namespace Ultraviolet.SDL2.Platform
{
    /// <summary>
    /// Represents the OpenGL implementation of the IUltravioletWindowInfo interface.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public sealed class SDL2UltravioletWindowInfo : IUltravioletWindowInfo, IUltravioletComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SDL2UltravioletWindowInfo"/> class.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        /// <param name="uvconfig">The Ultraviolet configuration settings for the current context.</param>
        /// <param name="winconfig">The window configuration settings for the current context.</param>
        public SDL2UltravioletWindowInfo(UltravioletContext uv, UltravioletConfiguration uvconfig, SDL2PlatformConfiguration winconfig)
        {
            Contract.Require(uv, nameof(uv));
            Contract.Require(uvconfig, nameof(uvconfig));
            Contract.Require(winconfig, nameof(winconfig));

            this.uv = uv;

            InitializePrimaryWindow(uvconfig, winconfig);
        }

        /// <summary>
        /// Draws the current window.
        /// </summary>
        /// <param name="time">Time elapsed since the last call to Draw.</param>
        public void Draw(UltravioletTime time)
        {
            var oglwin = (SDL2UltravioletWindow)current;

            oglwin.Draw(time);
        }

        /// <summary>
        /// Swaps the back buffer and the front buffer.
        /// </summary>
        public void Swap()
        {
            var oglwin = (SDL2UltravioletWindow)current;

            SDL.GL_SwapWindow((IntPtr)oglwin);
        }

        /// <summary>
        /// Designates the specified window as the primary window.
        /// </summary>
        /// <param name="window">The window to designate as the primary window, or null to clear the primary window.</param>
        public void DesignatePrimary(IUltravioletWindow window)
        {
            if (window != null && !windows.Contains(window))
                throw new InvalidOperationException(UltravioletStrings.InvalidResource);

            if (primary != window)
            {
                OnPrimaryWindowChanging();

                primary = window;

                OnPrimaryWindowChanged();
            }
        }

        /// <summary>
        /// Makes the specified window the current window.
        /// </summary>
        /// <param name="window">The window to make current.</param>
        /// <param name="context">The OpenGL context.</param>
        public void DesignateCurrent(IUltravioletWindow window, IntPtr context)
        {
            if (current != window)
            {
                OnCurrentWindowChanging();

                var glCurrentOld = current as SDL2UltravioletWindow;
                if (glCurrentOld != null)
                    glCurrentOld.IsCurrentWindow = false;

                current = window;

                var glCurrentNew = current as SDL2UltravioletWindow;
                if (glCurrentNew != null)
                    glCurrentNew.IsCurrentWindow = true;

                if (window != null && (window != glwin || window.SynchronizeWithVerticalRetrace != vsync))
                {
                    DesignateCurrentOpenGLWindow(window, context);
                }

                OnCurrentWindowChanged();
            }

            if (windows.Count == 0 || (window == null && context == IntPtr.Zero))
            {
                DesignateCurrentOpenGLWindow(null, context);
            }
        }

        /// <summary>
        /// Gets the window with the specified identifier.
        /// </summary>
        /// <returns>The window with the specified identifier, or null if no such window exists.</returns>
        public IUltravioletWindow GetByID(Int32 id)
        {
            var match = default(SDL2UltravioletWindow);
            foreach (SDL2UltravioletWindow window in windows)
            {
                if (SDL.GetWindowID((IntPtr)window) == (UInt32)id)
                {
                    match = window;
                    break;
                }
            }
            return match;
        }

        /// <summary>
        /// Gets a pointer to the SDL2 window object encapsulated by the window with the specified identifier.
        /// </summary>
        /// <returns>A pointer to the SDL2 window object encapsulated by the window with the specified identifier.</returns>
        public IntPtr GetPtrByID(Int32 id)
        {
            var window = GetByID(id);
            if (window != null)
            {
                return (IntPtr)(SDL2UltravioletWindow)window;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Gets the context's master window, which is used to create the OpenGL context.
        /// </summary>
        /// <returns>The context's master window.</returns>
        public IUltravioletWindow GetMaster()
        {
            return master;
        }

        /// <summary>
        /// Gets a pointer to the SDL2 window object encapsulated by the master window.
        /// </summary>
        /// <returns>A pointer to the SDL2 window object encapsulated by the master window.</returns>
        public IntPtr GetMasterPointer()
        {
            return (IntPtr)(SDL2UltravioletWindow)master;
        }

        /// <summary>
        /// Gets the context's primary window.
        /// </summary>
        /// <returns>The context's primary window, or null if the context is headless.</returns>
        public IUltravioletWindow GetPrimary()
        {
            return primary;
        }

        /// <summary>
        /// Gets a pointer to the SDL2 window object encapsulated by the primary window.
        /// </summary>
        /// <returns>A pointer to the SDL2 window object encapsulated by the primary window.</returns>
        public IntPtr GetPrimaryPointer()
        {
            return (IntPtr)(SDL2UltravioletWindow)primary;
        }

        /// <summary>
        /// Gets the context's current window.
        /// </summary>
        /// <returns>The context's current window.</returns>
        public IUltravioletWindow GetCurrent()
        {
            return current;
        }

        /// <summary>
        /// Gets a pointer to the SDL2 window object encapsulated by the current window.
        /// </summary>
        /// <returns>A pointer to the SDL2 window object encapsulated by the current window.</returns>
        public IntPtr GetCurrentPointer()
        {
            return (IntPtr)(SDL2UltravioletWindow)current;
        }

        /// <summary>
        /// Creates a new window and attaches it to the current context.
        /// </summary>
        /// <param name="caption">The window's caption text.</param>
        /// <param name="x">The x-coordinate at which to position the window's top-left corner.</param>
        /// <param name="y">The y-coordinate at which to position the window's top-left corner.</param>
        /// <param name="width">The width of the window's client area in pixels.</param>
        /// <param name="height">The height of the window's client area in pixels.</param>
        /// <param name="flags">A set of WindowFlags values indicating how to create the window.</param>
        /// <returns>The Ultraviolet window that was created.</returns>
        public IUltravioletWindow Create(String caption, Int32 x, Int32 y, Int32 width, Int32 height, WindowFlags flags = WindowFlags.None)
        {
            var sdlflags = (renderingAPI == SDL2PlatformRenderingAPI.OpenGL) ? SDL_WindowFlags.OPENGL : 0;

            if (Ultraviolet.SupportsHighDensityDisplayModes)
                sdlflags |= SDL_WindowFlags.ALLOW_HIGHDPI;

            if ((flags & WindowFlags.Resizable) == WindowFlags.Resizable)
                sdlflags |= SDL_WindowFlags.RESIZABLE;

            if ((flags & WindowFlags.Borderless) == WindowFlags.Borderless)
                sdlflags |= SDL_WindowFlags.BORDERLESS;

            if ((flags & WindowFlags.Hidden) == WindowFlags.Hidden)
                sdlflags |= SDL_WindowFlags.HIDDEN;
            else
                sdlflags |= SDL_WindowFlags.SHOWN;

            var sdlptr = SDL.CreateWindow(caption ?? String.Empty, x, y, width, height, sdlflags);
            if (sdlptr == IntPtr.Zero)
                throw new SDL2Exception();

            var win = new SDL2UltravioletWindow(Ultraviolet, sdlptr);
            windows.Add(win);

            Ultraviolet.Messages.Subscribe(win, SDL2UltravioletMessages.SDLEvent);

            OnWindowCreated(win);

            return win;
        }

        /// <summary>
        /// Creates a new Ultraviolet window from the specified native window and attaches it to the current context.
        /// </summary>
        /// <param name="ptr">A pointer that represents the native window to attach to the context.</param>
        /// <returns>The Ultraviolet window that was created.</returns>
        public IUltravioletWindow CreateFromNativePointer(IntPtr ptr)
        {
            var sdlptr = SDL.CreateWindowFrom(ptr);
            if (sdlptr == IntPtr.Zero)
                throw new SDL2Exception();

            var win = new SDL2UltravioletWindow(Ultraviolet, sdlptr);
            windows.Add(win);

            Ultraviolet.Messages.Subscribe(win, SDL2UltravioletMessages.SDLEvent);

            OnWindowCreated(win);

            return win;
        }

        /// <summary>
        /// Destroys the specified window.
        /// </summary>
        /// <remarks>Windows which were created from native pointers are disassociated from the current context,
        /// but are not actually destroyed.  To destroy such windows, use the native framework which created them.</remarks>
        /// <param name="window">The Ultraviolet window to destroy.</param>
        /// <returns>true if the window was destroyed; false if the window was closed.</returns>
        public Boolean Destroy(IUltravioletWindow window)
        {
            Contract.Require(window, nameof(window));

            if (!windows.Remove(window))
                throw new InvalidOperationException(UltravioletStrings.InvalidResource);

            if (window == current)
                throw new InvalidOperationException();

            OnWindowDestroyed(window);

            if (window == primary)
                DesignatePrimary(null);

            if (window == glwin && glcontext != IntPtr.Zero)
                DesignateCurrentOpenGLWindow(null, glcontext);

            var sdlwin = (SDL2UltravioletWindow)window;
            Ultraviolet.Messages.Unsubscribe(sdlwin, SDL2UltravioletMessages.SDLEvent);

            var native = sdlwin.Native;
            sdlwin.Dispose();

            return !native;
        }

        /// <summary>
        /// Destroys the window with the specified identifier.
        /// </summary>
        /// <param name="windowID">The identifier of the window to destroy.</param>
        /// <returns>true if the window was destroyed; false if the window was closed.</returns>
        public Boolean DestroyByID(Int32 windowID)
        {
            var window = GetByID(windowID);
            if (window != null)
            {
                Destroy(window);
            }
            return windows.Count == 0;
        }

        /// <summary>
        /// Gets the collection's enumerator.
        /// </summary>
        /// <returns>The collection's enumerator.</returns>
        public List<IUltravioletWindow>.Enumerator GetEnumerator()
        {
            return windows.GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns>The collection's enumerator.</returns>
        IEnumerator<IUltravioletWindow> IEnumerable<IUltravioletWindow>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns>The collection's enumerator.</returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the Ultraviolet context.
        /// </summary>
        public UltravioletContext Ultraviolet
        {
            get { return uv; }
        }

        /// <summary>
        /// Occurs after a window has been created.
        /// </summary>
        public event UltravioletWindowInfoEventHandler WindowCreated;

        /// <summary>
        /// Occurs when a window is about to be destroyed.
        /// </summary>
        public event UltravioletWindowInfoEventHandler WindowDestroyed;

        /// <summary>
        /// Occurs when the primary window is about to change.
        /// </summary>
        public event UltravioletWindowInfoEventHandler PrimaryWindowChanging;

        /// <summary>
        /// Occurs when the primary window changes.
        /// </summary>
        public event UltravioletWindowInfoEventHandler PrimaryWindowChanged;

        /// <summary>
        /// Occurs when the current window is about to change.
        /// </summary>
        public event UltravioletWindowInfoEventHandler CurrentWindowChanged;

        /// <summary>
        /// Occurs when the current window changes.
        /// </summary>
        public event UltravioletWindowInfoEventHandler CurrentWindowChanging;

        /// <summary>
        /// Initializes the context's primary window.
        /// </summary>
        private void InitializePrimaryWindow(UltravioletConfiguration uvconfig, SDL2PlatformConfiguration winconfig)
        {
            // Make sure we've been given a valid rendering API.
            if (winconfig.RenderingAPI != SDL2PlatformRenderingAPI.OpenGL)
                throw new NotSupportedException();

            renderingAPI = winconfig.RenderingAPI;

            // Retrieve the caption for our window.
            var caption = Localization.Strings.Contains("WINDOW_CAPTION") ?
                Localization.Get("WINDOW_CAPTION") : UltravioletStrings.DefaultWindowCaption.Value;

            // If this is an OpenGL window, set the appropriate attributes.
            if (winconfig.RenderingAPI == SDL2PlatformRenderingAPI.OpenGL)
            {
                // Set the OpenGL attributes for the window we're about to create.
                if (SDL.GL_SetAttribute(SDL_GLattr.MULTISAMPLEBUFFERS, winconfig.MultiSampleBuffers) < 0)
                    throw new SDL2Exception();

                if (SDL.GL_SetAttribute(SDL_GLattr.MULTISAMPLESAMPLES, winconfig.MultiSampleSamples) < 0)
                    throw new SDL2Exception();
            }
            
            // If we're running on Android or iOS, we can't create a headless context.
            var isRunningOnMobile = (Ultraviolet.Platform == UltravioletPlatform.Android || Ultraviolet.Platform == UltravioletPlatform.iOS);
            if (isRunningOnMobile && uvconfig.Headless)
                throw new InvalidOperationException(SDL2Strings.CannotCreateHeadlessContextOnMobile);

            // Initialize the hidden master window used to create the OpenGL context.
            var masterWidth = 0;
            var masterHeight = 0;
            var masterFlags = (renderingAPI == SDL2PlatformRenderingAPI.OpenGL) ? SDL_WindowFlags.OPENGL : 0;

            if (Ultraviolet.SupportsHighDensityDisplayModes)
                masterFlags |= SDL_WindowFlags.ALLOW_HIGHDPI;

            if (isRunningOnMobile)
            {
                masterFlags |= SDL_WindowFlags.FULLSCREEN | SDL_WindowFlags.RESIZABLE;
            }
            else
            {
                masterFlags |= SDL_WindowFlags.HIDDEN;
            }

            // Attempt to create the OpenGL window. If that fails, reduce our requirements and try again before failing.
            var masterptr = SDL.CreateWindow(isRunningOnMobile ? caption : String.Empty, 0, 0, masterWidth, masterHeight, masterFlags);
            if (masterptr == IntPtr.Zero)
            {
                if (SDL.GL_SetAttribute(SDL_GLattr.MULTISAMPLEBUFFERS, 0) < 0)
                    throw new SDL2Exception();

                if (SDL.GL_SetAttribute(SDL_GLattr.MULTISAMPLESAMPLES, 0) < 0)
                    throw new SDL2Exception();

                masterptr = SDL.CreateWindow(isRunningOnMobile ? caption : String.Empty, 0, 0, masterWidth, masterHeight, masterFlags);
                if (masterptr == IntPtr.Zero)
                {
                    throw new SDL2Exception();
                }
            }

            this.master = new SDL2UltravioletWindow(Ultraviolet, masterptr);

            // Set SDL_HINT_VIDEO_WINDOW_SHARE_PIXEL_FORMAT so that enlisted windows
            // will be OpenGL-enabled and set to the correct pixel format.
            if (!SDL.SetHint(SDL_Hint.VIDEO_WINDOW_SHARE_PIXEL_FORMAT, masterptr.ToStringHex()))
                throw new SDL2Exception();

            // If this is not a headless context, create the primary application window.
            if (!uvconfig.Headless)
            {
                if (isRunningOnMobile)
                {
                    this.windows.Add(this.master);
                    DesignatePrimary(this.master);
                }
                else
                {
                    var flags = uvconfig.WindowIsVisible ? WindowFlags.None : WindowFlags.Hidden;

                    if (uvconfig.WindowIsResizable)
                        flags |= WindowFlags.Resizable;

                    if (uvconfig.WindowIsBorderless)
                        flags |= WindowFlags.Borderless;

                    var primary = Create(caption,
                        uvconfig.InitialWindowPosition.X,
                        uvconfig.InitialWindowPosition.Y,
                        uvconfig.InitialWindowPosition.Width,
                        uvconfig.InitialWindowPosition.Height, flags);
                    DesignatePrimary(primary);
                }
            }
        }

        /// <summary>
        /// Raises the WindowCreated event.
        /// </summary>
        /// <param name="window">The window that was created.</param>
        private void OnWindowCreated(IUltravioletWindow window) =>
            WindowCreated?.Invoke(window);

        /// <summary>
        /// Raises the WindowDestroyed event.
        /// </summary>
        /// <param name="window">The window that is being destroyed.</param>
        private void OnWindowDestroyed(IUltravioletWindow window) =>
            WindowDestroyed?.Invoke(window);

        /// <summary>
        /// Raises the PrimaryWindowChanging event.
        /// </summary>
        private void OnPrimaryWindowChanging() =>
            PrimaryWindowChanging?.Invoke(primary);

        /// <summary>
        /// Raises the PrimaryWindowChanged event.
        /// </summary>
        private void OnPrimaryWindowChanged() =>
            PrimaryWindowChanged?.Invoke(primary);

        /// <summary>
        /// Raises the CurrentWindowChanging event.
        /// </summary>
        private void OnCurrentWindowChanging() =>
            CurrentWindowChanging?.Invoke(current);

        /// <summary>
        /// Raises the CurrentWindowChanged event.
        /// </summary>
        private void OnCurrentWindowChanged() =>
            CurrentWindowChanged?.Invoke(current);

        /// <summary>
        /// Binds the OpenGL context to the specified window.
        /// </summary>
        private void DesignateCurrentOpenGLWindow(IUltravioletWindow window, IntPtr context)
        {
            var shuttingDown = (window == null && context == IntPtr.Zero);

            if (context == IntPtr.Zero)
            {
                if (glcontext == IntPtr.Zero)
                {
                    return;
                }
                context = glcontext;
            }

            var win = (SDL2UltravioletWindow)(window ?? master);
            var winptr = (IntPtr)win;
            if (SDL.GL_MakeCurrent(winptr, context) < 0)
                throw new SDL2Exception();

            if (SDL.GL_SetSwapInterval(win.SynchronizeWithVerticalRetrace ? 1 : 0) < 0 && Ultraviolet.Platform != UltravioletPlatform.iOS)
            {
                if (!shuttingDown)
                    throw new SDL2Exception();
            }

            glwin = win;
            glcontext = context;
            vsync = win.SynchronizeWithVerticalRetrace;
        }

        // The context's attached windows.
        private readonly List<IUltravioletWindow> windows = new List<IUltravioletWindow>();

        // The primary and active windows.
        private SDL2PlatformRenderingAPI renderingAPI;
        private IUltravioletWindow master;
        private IUltravioletWindow primary;
        private IUltravioletWindow current;
        private IUltravioletWindow glwin;
        private IntPtr glcontext;
        private Boolean vsync;

        // The Ultraviolet context.
        private readonly UltravioletContext uv;
    }
}