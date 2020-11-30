
using System.Windows.Forms;
using Caliburn.Micro;

namespace Asv.Avialab.Core
{
    public static class FileDialogsExt
    {
       

        public static string ShowOpenFileDialog(this IWindowManager src, string caption, string filter = null, string initialDirectory = null)
        {
            string filename = null;
            Execute.OnUIThread(() =>
            {
                var ofd = new Microsoft.Win32.OpenFileDialog
                {
                    Title = caption,
                    CheckFileExists = true,
                    RestoreDirectory = true,
                    InitialDirectory = initialDirectory,
                    Multiselect = true,
                    Filter = filter
                };

                if (ofd.ShowDialog() == true)
                    filename = ofd.FileName;
            });
            return filename;
        }

        public static string ShowSaveFileDialog(this IWindowManager src, string caption, string defaultExt = null, string filter = null, string initialDirectory = null, string fileName = null)
        {
            string folder = null;
            Execute.OnUIThread(() =>
            {
                var ofd = new Microsoft.Win32.SaveFileDialog
                {
                    Title = caption,
                    DefaultExt = defaultExt,
                    InitialDirectory = initialDirectory,
                    CheckFileExists = false,
                    RestoreDirectory = true,
                    FileName = fileName,
                    Filter = filter
                };
                folder = ofd.ShowDialog() == true ? ofd.FileName : null;
            });

            return folder;
        }

        public static string ShowSelectFolderDialog(this IWindowManager src, string caption, string oldPath = null)
        {
            string folder = null;
            Execute.OnUIThread(() =>
            {
                var folderBrowser = new FolderBrowserDialog
                {
                    SelectedPath = oldPath,
                    Description = caption,
                    ShowNewFolderButton = true,
                };
                folder = folderBrowser.ShowDialog() == DialogResult.OK ? folderBrowser.SelectedPath : null;
            });
            return folder;
        }
    }
}
