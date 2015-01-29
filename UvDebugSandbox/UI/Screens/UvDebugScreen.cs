﻿using System;
using UvDebugSandbox.Input;
using TwistedLogik.Nucleus;
using TwistedLogik.Ultraviolet.Content;
using TwistedLogik.Ultraviolet.UI;
using TwistedLogik.Ultraviolet;

namespace UvDebugSandbox.UI.Screens
{
    /// <summary>
    /// Represents the base class for screens in this application.
    /// </summary>
    public abstract class UvDebugScreen : UIScreen
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UvDebugScreen"/> class.
        /// </summary>
        /// <param name="rootDirectory">The root directory of the panel's local content manager.</param>
        /// <param name="definitionAsset">The asset path of the screen's definition file.</param>
        /// <param name="globalContent">The content manager with which to load globally-available assets.</param>
        /// <param name="uiScreenService">The screen service which created this screen.</param>
        public UvDebugScreen(String rootDirectory, String definitionAsset, ContentManager globalContent, UIScreenService uiScreenService)
            : base(rootDirectory, definitionAsset, globalContent)
        {
            Contract.Require(uiScreenService, "uiScreenService");

            this.uiScreenService = uiScreenService;
        }

        /// <summary>
        /// Gets the screen service which created this screen.
        /// </summary>
        protected UIScreenService UIScreenService
        {
            get { return uiScreenService; }
        }

        /// <inheritdoc/>
        protected override void OnUpdating(UltravioletTime time)
        {
            if (View != null)
            {
                var actions = Ultraviolet.GetInput().GetActions();
                if (actions.NavigateUp.IsPressed(false))
                {
                    View.NavigateUp();
                }
                if (actions.NavigateDown.IsPressed(false))
                {
                    View.NavigateDown();
                }
                if (actions.NavigateLeft.IsPressed(false))
                {
                    View.NavigateLeft();
                }
                if (actions.NavigateRight.IsPressed(false))
                {
                    View.NavigateRight();
                }
                if (actions.NavigateNextTabStop.IsPressed(false))
                {
                    View.NavigateNextTabStop();
                }
                if (actions.NavigatePreviousTabStop.IsPressed(false))
                {
                    View.NavigatePreviousTabStop();
                }
            }
            base.OnUpdating(time);
        }

        // Property values.
        private readonly UIScreenService uiScreenService;
    }
}
