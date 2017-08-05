using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using Microsoft.Win32;
using ideaSpaceApplication.Model;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using Microsoft.Surface.Core;
using System.Windows.Controls;
using AviFile;
using System.Drawing;
using VoiceRecorder.Audio;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Threading;

namespace ideaSpaceApplication
{
  /// <summary>
  /// Interaction logic for projectWindow.xaml
  /// </summary>
  public partial class projectWindow : Window
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// 
    public Project project;

    //<ADDED>
    // Private fields for scanning
    Microsoft.Surface.Core.TouchTarget touchTarget;
    IntPtr hwnd;
    private byte[] normalizedImage;
    private Microsoft.Surface.Core.ImageMetrics normalizedMetrics;
    System.Drawing.Imaging.ColorPalette pal;
    
    public projectWindow()
    {
      InitializeComponent();
#if DEBUG
      string filename = @"C:\Users\Maysam\Downloads\st1.ids";
      FileStream fs = new FileStream(filename, FileMode.Open);
      BinaryFormatter bFormatter = new BinaryFormatter();
      project = (Project)bFormatter.Deserialize(fs);
      fs.Close();
      project.filename = filename;
      scatterView.ItemsSource = project.imageFilenames;
      noteTextBox.Text = project.note;
#endif
      scannedImage = new Cropper();
      scannedImage.VerticalAlignment = VerticalAlignment.Center;
      scannedImage.HorizontalAlignment = HorizontalAlignment.Center;
      scannedImage.VerticalContentAlignment = VerticalAlignment.Center;
      scannedImage.HorizontalContentAlignment = HorizontalAlignment.Center;
      scannedImage.Margin = new Thickness(10);
      scannedImage.SetValue(Grid.RowProperty, 1);
      scannedImage.Visibility = Visibility.Hidden;
      Grid.SetRow(scannedImage, 1);
      scanGrid.Children.Add(scannedImage);

      AddWindowAvailabilityHandlers();
      InitializeSurfaceInput();
    }

    public projectWindow(Project _project)
      : this()
    {
      project = _project;
      scatterView.ItemsSource = project.imageFilenames;
      noteTextBox.Text = project.note;
      projectTitle.Content = "Project \"" + project.name + "\"";
      project.log(Activity.OpenProject);
    }
    #region scan image
    private void InitializeSurfaceInput()
    {
      // Get the hWnd for the SurfaceWindow object after it has been loaded.
      hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
      touchTarget = new Microsoft.Surface.Core.TouchTarget(hwnd);
      // Set up the TouchTarget object for the entire SurfaceWindow object.
      touchTarget.EnableInput();
      EnableRawImage();
      // Attach an event handler for the FrameReceived 
      touchTarget.EnableImage(Microsoft.Surface.Core.ImageType.Normalized);
      touchTarget.FrameReceived += new EventHandler<FrameReceivedEventArgs>(OnTouchTargetFrameReceived);
    }

    private void Convert8bppBMPToGrayscale(Bitmap bmp)
    {
      if (pal == null) // pal is defined at module level as --- ColorPalette pal;
      {
        pal = bmp.Palette;
        for (int i = 0; i < 256; i++)
        {
          pal.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
        }
      }

      bmp.Palette = pal;
    }

    private void EnableRawImage()
    {
      touchTarget.EnableImage(Microsoft.Surface.Core.ImageType.Normalized);
      touchTarget.FrameReceived += OnTouchTargetFrameReceived;
    }

    private void DisableRawImage()
    {
      touchTarget.DisableImage(Microsoft.Surface.Core.ImageType.Normalized);
      touchTarget.FrameReceived -= OnTouchTargetFrameReceived;
    }

    bool ScanNow = false;
    private void scanNowButton_Click(object sender, RoutedEventArgs e)
    {
      ScanNow = true;
    }
    private void OnTouchTargetFrameReceived(object sender, Microsoft.Surface.Core.FrameReceivedEventArgs e)
    {
      if (scanGrid.Visibility != Visibility.Visible)
        return;
      if (!ScanNow)
        return;
      ScanNow = false;
      bool imageAvailable = false;
      int paddingLeft,
            paddingRight;
      if (normalizedImage == null)
      {
        imageAvailable = e.TryGetRawImage(Microsoft.Surface.Core.ImageType.Normalized,
            Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Left,
            Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Top,
            Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Width,
            Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Height,
            out normalizedImage,
            out normalizedMetrics,
            out paddingLeft,
            out paddingRight);
      }
      else
      {
        imageAvailable = e.UpdateRawImage(Microsoft.Surface.Core.ImageType.Normalized,
             normalizedImage,
             Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Left,
             Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Top,
             Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Width,
             Microsoft.Surface.Core.InteractiveSurface.PrimarySurfaceDevice.Height);
      }

      if (imageAvailable)
      {
        DisableRawImage();

        // Copy the normalizedImage byte array into a Bitmap object.

        GCHandle h = GCHandle.Alloc(normalizedImage, GCHandleType.Pinned);
        IntPtr ptr = h.AddrOfPinnedObject();
        Bitmap imageBitmap = new Bitmap(normalizedMetrics.Width,
                              normalizedMetrics.Height,
                              normalizedMetrics.Stride,
                              System.Drawing.Imaging.PixelFormat.Format8bppIndexed,
                              ptr);

        // The preceding code converts the bitmap to an 8-bit indexed color image. 
        // The following code creates a grayscale palette for the bitmap.
        Convert8bppBMPToGrayscale(imageBitmap);

        scannedImageFilename = Path.Combine(project.mediaFolder(), Guid.NewGuid().ToString() + ".jpeg");

        if (File.Exists(scannedImageFilename))
        {
          File.Delete(scannedImageFilename);
        }

        imageBitmap.Save(scannedImageFilename, System.Drawing.Imaging.ImageFormat.Jpeg);

        normalizedImage = null;

        // Re-enable collecting raw images.
        EnableRawImage();
        scannedImage.ImageUrl = scannedImageFilename;
        scannedImage.resetCrop();

        scannedImage.Visibility = Visibility.Visible;

        beforeScanLabel.Visibility = Visibility.Hidden;
        afterScanLabel.Visibility = Visibility.Visible;

      }
    }
    string scannedImageFilename;
    Cropper scannedImage;
    private void scanImageButton_Click(object sender, RoutedEventArgs e)
    {
      project.log(Activity.ScanImage);
      scanGrid.Visibility = Visibility.Visible;
      // Create a target for surface input. 
#if DEBUG
      scannedImage.ImageUrl = @"C:\Users\Maysam\Downloads\Capture.JPG";
      beforeScanLabel.Visibility = Visibility.Hidden;
      afterScanLabel.Visibility = Visibility.Visible;
#endif
    }

    private void noScanButton_Click(object sender, RoutedEventArgs e)
    {
      scanGrid.Visibility = Visibility.Hidden;

      beforeScanLabel.Visibility = Visibility.Visible;
      afterScanLabel.Visibility = Visibility.Hidden;
      scannedImageFilename = null;
    }

    private void yesCropButton_Click(object sender, RoutedEventArgs e)
    {
      if (scannedImageFilename != null)
      {
        string cropped_file = scannedImage.SaveCroppedImage(Path.Combine(project.mediaFolder(), Guid.NewGuid().ToString() + ".jpeg"));
        if (File.Exists(cropped_file))
        {
          project.imageFilenames.Add(new DataItem(cropped_file, true));
        }
      }
      noScanButton_Click(sender, e);
    }

    private void resetCropButton_Click(object sender, RoutedEventArgs e)
    {
      if (scannedImageFilename != null)
      {
        scannedImage.resetCrop();
      }
    }
    #endregion

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
      touchTarget.EnableImage(ImageType.Normalized);
    }

    /// <summary>
    /// This is called when the user can see but not interact with the application's window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnWindowNoninteractive(object sender, EventArgs e)
    {
      //TODO: Disable audio here if it is enabled
      touchTarget.DisableImage(Microsoft.Surface.Core.ImageType.Normalized);
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
      touchTarget.DisableImage(Microsoft.Surface.Core.ImageType.Normalized);
    }

    private void surfaceButton1_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      openFileDialog1.Filter = "Images|*.jpg;*.jpeg;*.bmp;*.png;*.tif;*.gif";
      openFileDialog1.Title = "Add Image To Project";
      openFileDialog1.ShowDialog();

      if (openFileDialog1.FileName != "")
        try
        {
          project.log(Activity.LoadImageLibrary);
          project.imageFilenames.Add(new DataItem(openFileDialog1.FileName, true));
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
        }
    }
    #region note compartment
    private void openNote_Click(object sender, RoutedEventArgs e)
    {
      noteExpander.IsExpanded = !noteExpander.IsExpanded;
    }

    private void noteTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (project != null)
      {
        project.note = noteTextBox.Text;
        project.save();
      }
    }

    private void noteExpander_Expanded(object sender, RoutedEventArgs e)
    {
      noteExpander.Visibility = Visibility.Visible;
      openNote.Content = "Close Notes";
    }

    private void noteExpander_Collapsed(object sender, RoutedEventArgs e)
    {
      noteExpander.Visibility = Visibility.Hidden;
      openNote.Content = "Open Notes";
    }
    #endregion

    ScatterViewItem draggedElement;
    private void DragSourcePreviewInputDeviceDown(object sender, InputEventArgs e)
    {
      FrameworkElement findSource = e.OriginalSource as FrameworkElement;
      draggedElement = null;

      // Find the ScatterViewItem object that is being touched.
      while (draggedElement == null && findSource != null)
      {
        if ((draggedElement = findSource as ScatterViewItem) == null)
        {
          findSource = VisualTreeHelper.GetParent(findSource) as FrameworkElement;
        }
      }

      if (draggedElement == null)
      {
        return;
      }
      draggedElement.Foreground = System.Windows.Media.Brushes.Pink;

      DataItem data = draggedElement.Content as DataItem;

      // If the data has not been specified as draggable, 
      // or the ScatterViewItem cannot move, return.
      if (data == null || !draggedElement.CanMove) // || !data.CanDrag 
      {
        return;
      }
      draggedElement.InvalidateVisual();
      draggedElement.CanScale = true;
      draggedElement.IsTopmostOnActivation = true;
      scatterView.UpdateLayout();
      lastItem = data;


      // Set the dragged element. This is needed in case the drag operation is canceled.
      data.DraggedElement = draggedElement;

      // Create the cursor visual.
      ContentControl cursorVisual = new ContentControl()
      {
        Content = draggedElement.DataContext,
        Style = FindResource("CursorStyle") as Style
      };
      // Create a list of input devices, 
      // and add the device passed to this event handler.
      List<InputDevice> devices = new List<InputDevice>();
      devices.Add(e.Device);

      // If there are touch devices captured within the element,
      // add them to the list of input devices.
      foreach (InputDevice device in draggedElement.TouchesCapturedWithin)
      {
        if (device != e.Device)
        {
          devices.Add(device);
        }
      }

      // Get the drag source object.
      ItemsControl dragSource = ItemsControl.ItemsControlFromItemContainer(draggedElement);

      // Start the drag-and-drop operation.
      SurfaceDragCursor cursor =
          SurfaceDragDrop.BeginDragDrop(
        // The ScatterView object that the cursor is dragged out from.
            dragSource,
        // The ScatterViewItem object that is dragged from the drag source.
            draggedElement,
        // The visual element of the cursor.
            cursorVisual,
        // The data attached with the cursor.
            draggedElement.DataContext,
        // The input devices that start dragging the cursor.
            devices,
        // The allowed drag-and-drop effects of the operation.
            DragDropEffects.Move);

      // If the cursor was created, the drag-and-drop operation was successfully started.
      if (cursor != null)
      {
        // Hide the ScatterViewItem.
        draggedElement.Visibility = Visibility.Hidden;

        // This event has been handled.
        e.Handled = true;
      }
    }
    DataItem lastItem = null;
    private void DragCanceled(object sender, SurfaceDragDropEventArgs e)
    {
      DataItem data = e.Cursor.Data as DataItem;
      ScatterViewItem item = data.DraggedElement as ScatterViewItem;
      if (item != null)
      {
        item.Visibility = Visibility.Visible;
        item.Orientation = e.Cursor.GetOrientation(this);
        item.Center = e.Cursor.GetPosition(this);
        item.UpdateLayout();
      }
    }
    private void DropTargetDragEnter(object sender, SurfaceDragDropEventArgs e)
    {
      e.Cursor.Visual.Tag = "DragEnter";
    }

    private void DropTargetDragLeave(object sender, SurfaceDragDropEventArgs e)
    {
      e.Cursor.Visual.Tag = null;
    }

    #region remove image
    private void OnDropTargetDragEnter(object sender, SurfaceDragDropEventArgs e)
    {
      DataItem data = e.Cursor.Data as DataItem;
      /*
       * always false
      if (!data.CanDrag)
      {
        e.Effects = DragDropEffects.None;
      }
       * 
       */
    }

    private void OnDropTargetDragLeave(object sender, SurfaceDragDropEventArgs e)
    {
      // Reset the effects.
      e.Effects = e.Cursor.AllowedEffects;
    }
    private void OnTargetChanged(object sender, TargetChangedEventArgs e)
    {
      if (e.Cursor.CurrentTarget != null)
      {
        DataItem data = e.Cursor.Data as DataItem;
        e.Cursor.Visual.Tag = "CanDrop";
      }
      else
      {
        e.Cursor.Visual.Tag = null;
      }
    }
    DataItem toBeRemoved;
    private void OnDropTargetDrop(object sender, SurfaceDragDropEventArgs e)
    {
      //  MessageBox.Show("drag dropped");    (e.Cursor.Data as DataItem);
    }
    private void DragCompleted(object sender, SurfaceDragCompletedEventArgs e)
    {
      // If the operation is Move, remove the data from drag source.
      if (e.Cursor.Effects == DragDropEffects.Move)
      {
        toBeRemoved = e.Cursor.Data as DataItem;
        if (toBeRemoved.Content != "" && File.Exists(toBeRemoved.Content))
          removeImage.Source = new BitmapImage(new Uri(toBeRemoved.Content));
        removeGrid.Visibility = Visibility.Visible;
      }
    }

    private void yesRemoveButton_Click(object sender, RoutedEventArgs e)
    {
      if (toBeRemoved != null)
      {
        project.log(Activity.DeleteImageLibrary);
        project.imageFilenames.Remove(toBeRemoved);
        toBeRemoved = null;
      }
      removeGrid.Visibility = Visibility.Hidden;
    }

    private void noRemoveButton_Click(object sender, RoutedEventArgs e)
    {
      draggedElement.Visibility = Visibility.Visible;
      removeGrid.Visibility = Visibility.Hidden;
    }
    
#endregion

    private void scatterView_ContainerActivated(object sender, RoutedEventArgs e)
    {
      // Get a reference to the ScatterViewItem control.
      ScatterViewItem item = (ScatterViewItem)e.Source;
      // If content is Control, change the foreground.
      DataItem di = item.DataContext as DataItem;
      item.InvalidateVisual();
      item.UpdateLayout();
      if (item.Content is Control)
      {
        // Save the current foreground.
        ((Control)item.Content).Tag = ((Control)item.Content).Foreground;
        // Set the foreground to red.
        ((Control)item.Content).Foreground = System.Windows.Media.Brushes.Red;
      }
    }

    private void openCanvas_Click(object sender, RoutedEventArgs e)
    {
      if (project.imageFilenames.Count == 0)
        return;
      project.log(Activity.OpenCanvas);
      if (lastItem == null || !project.imageFilenames.Contains(lastItem))
        lastItem = project.imageFilenames[0];
      canvasWindow cW = new canvasWindow(project, lastItem);
      cW.ShowDialog();
      project.save();
    }

    private void snapshotButton_Click(object sender, RoutedEventArgs e)
    {
      project.log(Activity.LibrarySnapshot);
      string snapshot_filename = Path.Combine(project.mediaFolder(), Guid.NewGuid().ToString() + ".jpeg");

      MessageBox.Show("Capture Width:" + scatterView.Width + " Height:" + scatterView.Height);

      snapshot(snapshot_filename, (int) scatterView.Width, (int) scatterView.Height);
      facebookWindow fW = new facebookWindow(project, snapshot_filename, PostType.Image);
      fW.ShowDialog();
    }

    public static void snapshot(string filename,int width, int height)
    {
      BitmapSource bs = CaptureRegion(width, height);
      if (filename != "" && filename != null)
      {
        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bs));
        FileStream fs = File.Open(filename, FileMode.OpenOrCreate);
        encoder.Save(fs);
        fs.Close();
      }
    }

    [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
    static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll", EntryPoint = "GetDC")]
    static extern IntPtr GetDC(IntPtr ptr);
    [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
    static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

    [DllImport("gdi32.dll", SetLastError = true)]
    static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")]
    static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
    [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
    static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
    [DllImport("gdi32.dll")]
    static extern bool DeleteObject(IntPtr hObject);

    // capture a region of a the screen, defined by the hWnd
    public static BitmapSource CaptureRegion(int w, int h)
    {
      IntPtr sourceDC = IntPtr.Zero;
      IntPtr targetDC = IntPtr.Zero;
      IntPtr compatibleBitmapHandle = IntPtr.Zero;

      int width = 800; //1920; // Screen.PrimaryScreen.Bounds.Width;
      int height = 600; // 1080; //  Screen.PrimaryScreen.Bounds.Height;

      if (w > width) width =w;
      if (h > height) height = h;

      const int y = 100;
      const int x = 100;
      BitmapSource bitmapSource = null;
      try
      {
        // gets the main desktop and all open windows
        sourceDC = GetDC(GetDesktopWindow());

        targetDC = CreateCompatibleDC(sourceDC);

        // create a bitmap compatible with our target DC
        compatibleBitmapHandle = CreateCompatibleBitmap(sourceDC, width, height);

        // gets the bitmap into the target device context
        SelectObject(targetDC, compatibleBitmapHandle);

        // copy from source to destination
        BitBlt(targetDC, 0, 0, width, height, sourceDC, x, y, TernaryRasterOperations.SRCCOPY);

        // Here's the WPF glue to make it all work. It converts from an
        // hBitmap to a BitmapSource. Love the WPF interop functions
        bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            compatibleBitmapHandle, IntPtr.Zero, Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
      }
      catch (Exception ex)
      {
        throw new Exception(string.Format("Error capturing region {0},{1},{2},{3}", x, y, width, height), ex);
      }
      finally
      {
        DeleteObject(compatibleBitmapHandle);

        ReleaseDC(IntPtr.Zero, sourceDC);
        ReleaseDC(IntPtr.Zero, targetDC);
      }
      return bitmapSource;
    }


    //---Enum Class for TernaryRasterOperations ---

    enum TernaryRasterOperations : uint
    {
      /// <summary>dest = source</summary>
      SRCCOPY = 0x00CC0020,
      /// <summary>dest = source OR dest</summary>
      SRCPAINT = 0x00EE0086,
      /// <summary>dest = source AND dest</summary>
      SRCAND = 0x008800C6,
      /// <summary>dest = source XOR dest</summary>
      SRCINVERT = 0x00660046,
      /// <summary>dest = source AND (NOT dest)</summary>
      SRCERASE = 0x00440328,
      /// <summary>dest = (NOT source)</summary>
      NOTSRCCOPY = 0x00330008,
      /// <summary>dest = (NOT src) AND (NOT dest)</summary>
      NOTSRCERASE = 0x001100A6,
      /// <summary>dest = (source AND pattern)</summary>
      MERGECOPY = 0x00C000CA,
      /// <summary>dest = (NOT source) OR dest</summary>
      MERGEPAINT = 0x00BB0226,
      /// <summary>dest = pattern</summary>
      PATCOPY = 0x00F00021,
      /// <summary>dest = DPSnoo</summary>
      PATPAINT = 0x00FB0A09,
      /// <summary>dest = pattern XOR dest</summary>
      PATINVERT = 0x005A0049,
      /// <summary>dest = (NOT dest)</summary>
      DSTINVERT = 0x00550009,
      /// <summary>dest = BLACK</summary>
      BLACKNESS = 0x00000042,
      /// <summary>dest = WHITE</summary>
      WHITENESS = 0x00FF0062,
      /// <summary>
      /// Capture window as seen on screen.  This includes layered windows 
      /// such as WPF windows with AllowsTransparency="true"
      /// </summary>
      CAPTUREBLT = 0x40000000
    }

    DispatcherTimer dispatcherTimer;
    int snapshots_counter;
    string snapshots_directory;

    AudioRecorder audio_recorder = new AudioRecorder();
    string audio_filename;

    string video_filename;
    VideoStream aviStream = null;
    AviManager aviManager = null;
    private void takeVideoButton_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show("Video recording is not available");
      return;
      try
      {

        if (dispatcherTimer == null)
        {
          dispatcherTimer = new DispatcherTimer();
          dispatcherTimer.Tick += (o, args) =>
          {
            snapshots_counter++;
            string filename = Path.Combine(snapshots_directory, snapshots_counter + ".jpeg");
            snapshot(filename, (int) scatterView.Width, (int) scatterView.Height);
          };
        }
        if (dispatcherTimer.IsEnabled)
        {
          dispatcherTimer.Stop();
          audio_recorder.Stop();


          video_filename = Path.Combine(project.mediaFolder(), Guid.NewGuid().ToString() + ".avi");
          File.Delete(video_filename);
          aviManager = new AviFile.AviManager(video_filename, false);
          for (int i = 1; i <= snapshots_counter; i++)
          {
            string filename = Path.Combine(snapshots_directory, i + ".jpeg");

            using (Bitmap bm = Bitmap.FromFile(filename) as Bitmap)
            {
              if (aviStream == null)
              {
                int fps = 5;
                aviStream = aviManager.AddVideoStream(true, fps, bm);
              }
              else
              {
                aviStream.AddFrame(bm);
              }
            }
          }
          //        aviManager.Close();
          //          aviManager = new AviFile.AviManager(video_filename, true);
          aviManager.AddAudioStream(audio_filename, 0);
          aviManager.Close();

          takeVideoButton.Content = "Record Video";
          takeVideoButton.Foreground = System.Windows.Media.Brushes.DarkGreen;

          project.log(Activity.StopRecordVideo);
          facebookWindow fW = new facebookWindow(project, video_filename, PostType.Video);
          fW.ShowDialog();
        }
        else
        {
          dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 40);
          dispatcherTimer.Start();
          // make a temp folder and if one exists delete the files inside it
          snapshots_directory = Path.Combine(Path.GetDirectoryName(project.filename), "temp");
          if (Directory.Exists(snapshots_directory))
          {
            DirectoryInfo folderInfo = new DirectoryInfo(snapshots_directory);
            foreach (FileInfo file in folderInfo.GetFiles())
            {
              file.Delete();
            }
          }
          else
          {
            Directory.CreateDirectory(snapshots_directory);
          }

          project.log(Activity.StartRecordVideo);
          
          snapshots_counter = 0;
          audio_filename = Path.Combine(project.mediaFolder(), Guid.NewGuid().ToString() + ".wav");
          audio_recorder.BeginMonitoring(0);
          audio_recorder.BeginRecording(audio_filename);

          takeVideoButton.Content = "Stop Recording";
          takeVideoButton.Foreground = System.Windows.Media.Brushes.Red;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
    }

    private void genericAudioButton_Click(object sender, RoutedEventArgs e)
    {
      if (audio_recorder.RecordingState == RecordingState.Stopped)
      {
        genericAudioButton.Foreground = System.Windows.Media.Brushes.IndianRed;
        genericAudioButton.Content = "Recording";

        audio_filename = Path.Combine(project.mediaFolder(), Guid.NewGuid().ToString() + ".wav");
        File.Delete(audio_filename);
        project.log(Activity.StartRecordAudio);
        audio_recorder.BeginMonitoring(0);
        audio_recorder.BeginRecording(audio_filename);
      }
      else
      {
        genericAudioButton.Foreground = System.Windows.Media.Brushes.DarkGreen;
        genericAudioButton.Content = "Generic Audio";

        project.log(Activity.StopRecordAudio);
        audio_recorder.Stop();
        video_filename = Path.Combine(project.mediaFolder(), Guid.NewGuid().ToString() + ".avi");
        aviManager = new AviFile.AviManager(video_filename, false);

        using (Bitmap bm = Bitmap.FromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "audio_banner.jpg")) as Bitmap)
        {
          aviStream = aviManager.AddVideoStream(true, 1, bm);
        }
        aviManager.AddAudioStream(audio_filename, 0);
        aviManager.Close();
        facebookWindow fW = new facebookWindow(project, video_filename, PostType.Video);
        fW.ShowDialog();
      }
    }

    private void noteTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        KeyboardHelper.hideKeyboard();
    }

    private void noteTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        KeyboardHelper.showKeyboard();
    }

    private void SurfaceButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show("Are you sure you want to exit?", "Exit IdeaSpace", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
        if (result == MessageBoxResult.Yes)
        {
            Close();
        }
    }

  }
}