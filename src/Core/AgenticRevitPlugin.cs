using System;
using System.IO;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Serilog;
using AgenticRevit.Core.Events;
using AgenticRevit.ChangeTracking;
using AgenticRevit.Graph;

namespace AgenticRevit.Core
{
    /// <summary>
    /// AgenticREVIT - Agentic BIM Plugin for Revit 2025
    /// Main application entry point implementing IExternalApplication
    /// </summary>
    public class AgenticRevitPlugin : IExternalApplication
    {
        #region Static Properties

        /// <summary>
        /// Singleton instance of the plugin
        /// </summary>
        public static AgenticRevitPlugin? Instance { get; private set; }

        /// <summary>
        /// Current UIControlledApplication reference
        /// </summary>
        public static UIControlledApplication? UiControlledApp { get; private set; }

        /// <summary>
        /// Plugin assembly directory path
        /// </summary>
        public static string AssemblyDirectory =>
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        #endregion

        #region Private Fields

        private ChangeMonitor? _changeMonitor;
        private RevisionManager? _revisionManager;
        private OntologyManager? _ontologyManager;

        #endregion

        #region IExternalApplication Implementation

        /// <summary>
        /// Called when Revit starts up
        /// </summary>
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                Instance = this;
                UiControlledApp = application;

                // Initialize logging
                InitializeLogging();
                Log.Information("AgenticREVIT Plugin starting up...");

                // Register event handlers
                RegisterEventHandlers(application);

                // Initialize core modules
                InitializeCoreModules();

                // Create ribbon UI
                CreateRibbonUI(application);

                Log.Information("AgenticREVIT Plugin initialized successfully");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize AgenticREVIT Plugin");
                TaskDialog.Show("AgenticREVIT Error",
                    $"Failed to initialize plugin: {ex.Message}");
                return Result.Failed;
            }
        }

        /// <summary>
        /// Called when Revit shuts down
        /// </summary>
        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                Log.Information("AgenticREVIT Plugin shutting down...");

                // Unregister event handlers
                UnregisterEventHandlers(application);

                // Dispose modules
                DisposeModules();

                Log.Information("AgenticREVIT Plugin shut down successfully");
                Log.CloseAndFlush();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during AgenticREVIT shutdown");
                return Result.Failed;
            }
        }

        #endregion

        #region Initialization Methods

        private void InitializeLogging()
        {
            var logPath = Path.Combine(AssemblyDirectory, "Logs", "AgenticRevit-.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        private void RegisterEventHandlers(UIControlledApplication application)
        {
            // Application-level events
            application.ControlledApplication.DocumentOpened += OnDocumentOpened;
            application.ControlledApplication.DocumentClosing += OnDocumentClosing;
            application.ControlledApplication.DocumentSaved += OnDocumentSaved;
            application.ControlledApplication.DocumentChanged += OnDocumentChanged;

            Log.Debug("Event handlers registered");
        }

        private void UnregisterEventHandlers(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentOpened -= OnDocumentOpened;
            application.ControlledApplication.DocumentClosing -= OnDocumentClosing;
            application.ControlledApplication.DocumentSaved -= OnDocumentSaved;
            application.ControlledApplication.DocumentChanged -= OnDocumentChanged;

            Log.Debug("Event handlers unregistered");
        }

        private void InitializeCoreModules()
        {
            // Initialize Change Monitor
            _changeMonitor = new ChangeMonitor();

            // Initialize Revision Manager (hourly backups)
            _revisionManager = new RevisionManager();

            // Initialize Ontology Manager
            _ontologyManager = new OntologyManager();

            Log.Debug("Core modules initialized");
        }

        private void DisposeModules()
        {
            _changeMonitor?.Dispose();
            _revisionManager?.Dispose();
            _ontologyManager?.Dispose();
        }

        #endregion

        #region Ribbon UI Creation

        private void CreateRibbonUI(UIControlledApplication application)
        {
            const string tabName = "AgenticREVIT";
            const string panelName = "BIM Intelligence";

            try
            {
                // Create custom tab
                application.CreateRibbonTab(tabName);
            }
            catch (Exception)
            {
                // Tab might already exist
            }

            // Create panel
            var panel = application.CreateRibbonPanel(tabName, panelName);

            // Add buttons
            CreateMainButton(panel);
            CreateGraphButton(panel);
            CreateBackupButton(panel);
            CreateQueryButton(panel);

            Log.Debug("Ribbon UI created");
        }

        private void CreateMainButton(RibbonPanel panel)
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            var buttonData = new PushButtonData(
                "AgenticRevitMain",
                "Dashboard",
                assemblyPath,
                "AgenticRevit.Core.Commands.ShowDashboardCommand")
            {
                ToolTip = "Open AgenticREVIT Dashboard",
                LongDescription = "Opens the main dashboard for BIM intelligence features including " +
                                 "graph visualization, change tracking, and LLM integration."
            };

            panel.AddItem(buttonData);
        }

        private void CreateGraphButton(RibbonPanel panel)
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            var buttonData = new PushButtonData(
                "GraphViewer",
                "Graph\nViewer",
                assemblyPath,
                "AgenticRevit.Core.Commands.ShowGraphViewerCommand")
            {
                ToolTip = "View BIM Ontology Graph",
                LongDescription = "Visualize the project's ontology graph showing relationships " +
                                 "between elements, spaces, tasks, and costs."
            };

            panel.AddItem(buttonData);
        }

        private void CreateBackupButton(RibbonPanel panel)
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            var buttonData = new PushButtonData(
                "BackupManager",
                "Backup\nManager",
                assemblyPath,
                "AgenticRevit.Core.Commands.ShowBackupManagerCommand")
            {
                ToolTip = "Manage Revision Backups",
                LongDescription = "View and restore hourly backup checkpoints. " +
                                 "Track changes between revisions."
            };

            panel.AddItem(buttonData);
        }

        private void CreateQueryButton(RibbonPanel panel)
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            var buttonData = new PushButtonData(
                "AIQuery",
                "AI\nQuery",
                assemblyPath,
                "AgenticRevit.Core.Commands.ShowQueryInterfaceCommand")
            {
                ToolTip = "AI-powered BIM Query",
                LongDescription = "Ask questions about your BIM model using natural language. " +
                                 "Powered by LLM integration."
            };

            panel.AddItem(buttonData);
        }

        #endregion

        #region Event Handlers

        private void OnDocumentOpened(object sender, DocumentOpenedEventArgs e)
        {
            Log.Information("Document opened: {DocumentTitle}", e.Document?.Title ?? "Unknown");

            try
            {
                // Initialize tracking for new document
                _changeMonitor?.StartMonitoring(e.Document);
                _revisionManager?.InitializeForDocument(e.Document);
                _ontologyManager?.BuildInitialGraph(e.Document);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling document opened event");
            }
        }

        private void OnDocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            Log.Information("Document closing: {DocumentTitle}", e.Document?.Title ?? "Unknown");

            try
            {
                // Stop tracking and save state
                _changeMonitor?.StopMonitoring(e.Document);
                _revisionManager?.CreateCheckpoint(e.Document, "Document Close");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling document closing event");
            }
        }

        private void OnDocumentSaved(object sender, DocumentSavedEventArgs e)
        {
            Log.Information("Document saved: {DocumentTitle}", e.Document?.Title ?? "Unknown");

            try
            {
                // Create save checkpoint
                _revisionManager?.CreateCheckpoint(e.Document, "Manual Save");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling document saved event");
            }
        }

        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            try
            {
                var doc = e.GetDocument();

                // Get change details
                var addedIds = e.GetAddedElementIds();
                var modifiedIds = e.GetModifiedElementIds();
                var deletedIds = e.GetDeletedElementIds();

                Log.Debug("Document changed - Added: {Added}, Modified: {Modified}, Deleted: {Deleted}",
                    addedIds.Count, modifiedIds.Count, deletedIds.Count);

                // Process changes
                _changeMonitor?.ProcessChanges(doc, addedIds, modifiedIds, deletedIds);

                // Update ontology graph
                _ontologyManager?.UpdateGraph(doc, addedIds, modifiedIds, deletedIds);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling document changed event");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the current change monitor instance
        /// </summary>
        public ChangeMonitor? GetChangeMonitor() => _changeMonitor;

        /// <summary>
        /// Gets the current revision manager instance
        /// </summary>
        public RevisionManager? GetRevisionManager() => _revisionManager;

        /// <summary>
        /// Gets the current ontology manager instance
        /// </summary>
        public OntologyManager? GetOntologyManager() => _ontologyManager;

        /// <summary>
        /// Force create a backup checkpoint
        /// </summary>
        public void CreateManualBackup(Document doc, string description)
        {
            _revisionManager?.CreateCheckpoint(doc, description);
        }

        #endregion
    }
}
