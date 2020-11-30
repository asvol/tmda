using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Asv.Avialab.Core
{
    /// <summary>
    /// Interaction logic for DigitPropertyView.xaml
    /// </summary>
    public partial class DigitPropertyView : UserControl
    {
        public DigitPropertyView()
        {
            InitializeComponent();
            text.KeyDown += Text_KeyDown;
        }

        private void Text_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                text.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void digit_GotFocus(object sender, RoutedEventArgs e)
        {
            var editable = DataContext as IEditable;
            if (editable != null)
            {
                editable.IsEdit = true;
            }
        }

        private void digit_LostFocus(object sender, RoutedEventArgs e)
        {
            var editable = DataContext as IEditable;
            if (editable != null)
            {
                editable.IsEdit = false;
            }
        }
    }
}
