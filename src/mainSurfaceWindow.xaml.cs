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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using Microsoft.Win32;
using ideaSpaceApplication.Model;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ideaSpaceApplication
{
    /// <summary>
    /// Interaction logic for mainSurfaceWindow.xaml
    /// </summary>
    public partial class mainSurfaceWindow : Window
    {
        private Project project;
        /// <summary>
        /// Default constructor.
        /// </summary>
        public mainSurfaceWindow()
        {
            InitializeComponent();
            //Loaded += MainWindow_Loaded;

            // Disables inking in the WPF application and enables us to track touch events to properly trigger the touch keyboard
            //InkInputHelper.DisableWPFTabletSupport();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Windows 8 API to enable touch keyboard to monitor for focus tracking in this WPF application
            InputPanelConfiguration cp = new InputPanelConfiguration();
            IInputPanelConfiguration icp = cp as IInputPanelConfiguration;
            if (icp != null)
                icp.EnableFocusTracking();
        }

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }


        private void createProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (projectNameText.Text.Trim() == "")
            {
                MessageBox.Show("Project Name is mandatory");
                return;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "idea Space|*.ids";
            saveFileDialog1.Title = "Save the New Project";
            saveFileDialog1.FileName = projectNameText.Text;
            if((bool)saveFileDialog1.ShowDialog() && saveFileDialog1.FileName != "")
            {
                project = new Project();
                project.name = projectNameText.Text;
                project.save(saveFileDialog1.FileName);
                //  load edit window
                try
                {
                  projectWindow pw = new projectWindow(project);
                  pw.ShowDialog();
                  Close();
                }
                catch { }
            }
        }

        private void loadProjectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "idea Space|*.ids";
            openFileDialog1.Title = "Open Project";

            if ((bool)openFileDialog1.ShowDialog() && openFileDialog1.FileName != "")
            {
                try
                {
                    System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    project = (Project)bFormatter.Deserialize(fs);
                    fs.Close();
                    project.filename = openFileDialog1.FileName;
                    //  open edit window
                    projectWindow pw = new projectWindow(project);
                    pw.ShowDialog();
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void surfaceButton1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void projectNameText_GotFocus(object sender, RoutedEventArgs e)
        {
            KeyboardHelper.showKeyboard();
        }

        private void projectNameText_LostFocus(object sender, RoutedEventArgs e)
        {
            KeyboardHelper.hideKeyboard();
        }

    }
}