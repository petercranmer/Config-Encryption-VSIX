using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

namespace ConfigSectionDecryptor.Services
{
    class VisualStudioInteropService
    {
        private readonly AsyncPackage package;

        public VisualStudioInteropService(AsyncPackage package)
        {
            this.package = package;
        }

        private IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public async Task<string> GetSelectionAsync()
        {
            var service = await this.ServiceProvider.GetServiceAsync(typeof(SVsTextManager));
            var textManager = service as IVsTextManager2;
            IVsTextView view;
            int result = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);

            //view.GetSelection(out int startLine, out int startColumn, out int endLine, out int endColumn);//end could be before beginning
            view.GetSelectedText(out string selectedText);

            return selectedText;
        }

        public async Task<string> GetActiveFilePathAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var applicationObject = await this.ServiceProvider.GetServiceAsync(typeof(DTE)) as DTE2;

            Assumes.Present(applicationObject);

            return applicationObject.ActiveDocument.FullName;
        }

        public void DisplayOkMessage(string title, string message)
        {
            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}