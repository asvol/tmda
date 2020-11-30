using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Asv.Avialab.Core.View.PropertyEditor.Text.Deg
{
    /// <summary>
    /// Interaction logic for DegreePropertyView.xaml
    /// </summary>
    public partial class DegreePropertyView : UserControl
    {
        public DegreePropertyView()
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
