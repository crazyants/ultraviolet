﻿using Ultraviolet.Core;

namespace Ultraviolet.Presentation
{
    partial class PresentationFoundation
    {
        /// <summary>
        /// Draws a diagnostics panel containing various Presentation Foundation performance metrics.
        /// </summary>
        public void DrawDiagnosticsPanel()
        {
            if (diagnosticsPanel == null)
                diagnosticsPanel = new DiagnosticsPanel(Ultraviolet);

            diagnosticsPanel.Draw();
        }

        /// <summary>
        /// Gets the performance statistics which have been collected by the Ultraviolet Presentation Foundation.
        /// </summary>
        public PresentationFoundationPerformanceStats PerformanceStats
        {
            get
            {
                Contract.EnsureNotDisposed(this, Disposed);

                return performanceStats;
            }
        }

        // Diagnostics values.
        private readonly PresentationFoundationPerformanceStats performanceStats = new PresentationFoundationPerformanceStats();
        private DiagnosticsPanel diagnosticsPanel;
    }
}
