using System;
using System.Windows;
using System.Windows.Input;

namespace Asv.Avialab.Core
{
    /// <summary>
    /// Interaction logic for BaseWindowView.xaml
    /// </summary>
    public partial class BaseWindowView
    {
        public BaseWindowView()
        {
            InitializeComponent();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Normal:
                    this.WindowState = WindowState.Maximized;
                    break;
                case WindowState.Minimized:
                    this.WindowState = WindowState.Maximized;
                    break;
                case WindowState.Maximized:
                    this.WindowState = WindowState.Normal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BaseWindowView_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.Key == Key.RightAlt)
            {
                switch (this.WindowState)
                {
                    case WindowState.Normal:
                        this.WindowState = WindowState.Maximized;
                        break;
                    case WindowState.Minimized:
                        this.WindowState = WindowState.Maximized;
                        break;
                    case WindowState.Maximized:
                        this.WindowState = WindowState.Normal;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
