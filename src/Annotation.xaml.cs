using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ideaSpaceApplication
{
    /// <summary>
    /// Interaction logic for Annotation.xaml
    /// </summary>
    public partial class Annotation : UserControl
    {
        public Annotation()
        {
            InitializeComponent();
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            mainLabel.Content = mainTextBox.Text;
            editGrid.Visibility = Visibility.Hidden;
            mainLabel.Visibility = Visibility.Visible;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            editGrid.Visibility = Visibility.Hidden;
            mainLabel.Visibility = Visibility.Visible;
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.Visibility = Visibility.Hidden;
        }

        private void mainLabel_TouchEnter(object sender, InputEventArgs e)
        {
            mainTextBox.Text = mainLabel.Content as String;
            editGrid.Visibility = Visibility.Visible;
            mainLabel.Visibility = Visibility.Hidden;
        }
    }
}
