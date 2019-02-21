using System;
using System.ComponentModel.Design;
using ConfigSectionDecryptor.Services;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ConfigSectionDecryptor
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class DecryptConfigSectionCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("b1562c78-a140-48c1-9d41-498df32a4c1d");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private readonly VisualStudioInteropService visualStudioInteropService;
        private readonly ConfigurationEncryptionService configurationEncryptionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecryptConfigSectionCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private DecryptConfigSectionCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.ExecuteAsync, menuCommandID);
            commandService.AddCommand(menuItem);

            this.visualStudioInteropService = new VisualStudioInteropService(this.package);
            this.configurationEncryptionService = new ConfigurationEncryptionService();
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static DecryptConfigSectionCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in DecryptConfigSectionCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new DecryptConfigSectionCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void ExecuteAsync(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var sectionName = await this.visualStudioInteropService.GetSelectionAsync();
            var currentDocumentFilename = await this.visualStudioInteropService.GetActiveFilePathAsync();

            var result = this.configurationEncryptionService.DecryptConfigSection(
                currentDocumentFilename, 
                sectionName
            );

            if (!result)
            {
                this.visualStudioInteropService.DisplayOkMessage(
                    "Error decrypting",
                    $"Unable to decrypt {sectionName}."
                );
            }
        }
    }
}
