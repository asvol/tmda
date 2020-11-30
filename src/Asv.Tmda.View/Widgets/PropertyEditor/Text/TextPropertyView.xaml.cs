using System.Windows.Controls;
using System.Windows.Input;

namespace Asv.Avialab.Core
{
    
    public partial class TextPropertyView : UserControl
    {
        public TextPropertyView()
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

        private void text_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var editable = DataContext as IEditable;
            if (editable != null)
            {
                editable.IsEdit = true;
            }
            
        }

        private void text_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var editable = DataContext as IEditable;
            if (editable != null)
            {
                editable.IsEdit = false;
            }
        }
    }
}
