using System;
using System.Linq;
using Caliburn.Micro;

namespace Asv.Avialab.Core
{
    public static class MessageBoxExt
    {
        public static object ShowMessageBox(this IWindowManager src, string caption, string message, MessageBoxIconType icon, MessageBoxHelpButton helpButton, params MessageBoxButton[] buttons)
        {
            object result = null;

            Execute.OnUIThread(() =>
            {
                var viewModel = new MessageBoxViewModel { DisplayName = caption, Message = message, Icon = icon };
                if (buttons != null)
                {
                    foreach (var messageBoxButton in buttons)
                    {
                        viewModel.AddButton(messageBoxButton);
                    }
                }
                if (helpButton != null)
                {
                    viewModel.HelpButtonCallback = helpButton.Action;
                    viewModel.HelpButtonText = helpButton.Text;
                }
                result = IoC.Get<IWindowManager>().ShowDialog(viewModel) == true ? viewModel.DialogResult : null;

            });
            return result;
        }

        public static TResult ShowMessageBox<TResult>(this IWindowManager src, string caption, string message, MessageBoxIconType icon, MessageBoxHelpButton helpButton, params MessageBoxButton<TResult>[] buttons)
        {
            var result = src.ShowMessageBox(caption, message, icon, helpButton, buttons.Cast<MessageBoxButton>().ToArray());
            if (result == null) return default(TResult);
            return (TResult)result;
        }

        public static TResult ShowMessageBox<TResult>(this IWindowManager src, string caption, string message, MessageBoxIconType icon, params MessageBoxButton<TResult>[] buttons)
        {
            var result = src.ShowMessageBox(caption, message, icon, null, buttons);
            if (result == null) return default(TResult);
            return result;
        }

        public static bool AskUserYesNo(this IWindowManager src, string caption, string message, MessageBoxIconType icon = MessageBoxIconType.Question, MessageBoxHelpButton helpButton = null)
        {
            return src.ShowMessageBox(caption, message, icon, helpButton,
                                      new MessageBoxButton<bool>(MessageBoxRS.CaliburnDialogs_AskUserYesNo_Yes, true, true),
                                      new MessageBoxButton<bool>(MessageBoxRS.CaliburnDialogs_AskUserYesNo_No, false, false, true));
        }

        public static bool AskUserOkCancel(this IWindowManager src, string caption, string message, MessageBoxIconType icon = MessageBoxIconType.Question, MessageBoxHelpButton helpButton = null)
        {
            return src.ShowMessageBox(caption, message, icon, helpButton,
                                      new MessageBoxButton<bool>(MessageBoxRS.CaliburnDialogs_ShowError_Ok, true, true),
                                      new MessageBoxButton<bool>(MessageBoxRS.CaliburnDialogs_AskUserOkCancel_Cancel, false, false, true));
        }


        public static void ShowError(this IWindowManager src, string caption, string message, MessageBoxHelpButton helpButton = null)
        {
            src.ShowMessageBox(caption, message, MessageBoxIconType.Error, helpButton, new MessageBoxButton(MessageBoxRS.CaliburnDialogs_ShowError_Ok, null, true, true));
        }

        public static void ShowError(this IWindowManager src, string caption, string message, Exception ex)
        {
            src.ShowMessageBox(caption, message, MessageBoxIconType.Error, new MessageBoxHelpButton(MessageBoxRS.CaliburnDialogs_ShowError_More, () => src.ShowExceptionInfoDialog(MessageBoxRS.CaliburnDialogs_ShowError_More, ex)), new MessageBoxButton(MessageBoxRS.CaliburnDialogs_ShowError_Ok, null, true, true));
        }

        public static void ShowInfo(this IWindowManager src, string caption, string message, MessageBoxHelpButton helpButton = null)
        {
            src.ShowMessageBox(caption, message, MessageBoxIconType.Info, helpButton, new MessageBoxButton(MessageBoxRS.CaliburnDialogs_ShowError_Ok, null, true, true));
        }

        public static void ShowWarning(this IWindowManager src, string caption, string message, MessageBoxHelpButton helpButton = null)
        {
            src.ShowMessageBox(caption, message, MessageBoxIconType.Warning, helpButton, new MessageBoxButton(MessageBoxRS.CaliburnDialogs_ShowError_Ok, null, true, true));
        }

        public static void ShowExceptionInfoDialog(this IWindowManager src, string caption, Exception ex)
        {
            src.ShowError(caption, ex.ToString());
        }
    }
}
