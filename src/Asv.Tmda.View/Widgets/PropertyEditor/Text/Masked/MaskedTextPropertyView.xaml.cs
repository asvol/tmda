using System.Windows.Controls;
using System.Windows.Input;

namespace Asv.Avialab.Core
{
    /// <summary>
    /// Interaction logic for PropertyTextBoxView.xaml
    /// </summary>
    public partial class MaskedTextPropertyView : UserControl
    {
        public MaskedTextPropertyView()
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
    }
}
