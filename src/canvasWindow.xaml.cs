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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.IO;
using System.Reflection;
using ideaSpaceApplication.Model;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using VoiceRecorder.Audio;
using Facebook;
using System.Windows.Ink;


namespace ideaSpaceApplication
{
  /// <summary>
  /// Interaction logic for canvasWindow.xaml
  /// </summary>
  public partial class canvasWindow : Window
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// 
    private Project project;
    private DataItem item;
    private AudioRecorder recorder = new AudioRecorder();
    private Image active_image = null;
    //private Process = keyprocess;
    bool audio_annotating;

    public canvasWindow()
    {

      InitializeComponent();
      // Add handlers for window availability events
      AddWindowAvailabilityHandlers();
      createGridOfColor();
      colorSelector.Visibility = Visibility.Hidden;
      surfaceInkCanvas1.DefaultDrawingAttributes.Color = Colors.Navy;
#if DEBUG
      item = new DataItem(@"C:\Users\Maysam\Downloads\Capture.JPG", true);
      mainImage.Source = new BitmapImage(new Uri(item.Content));
      project = new Project();
#endif
    }

    private Image createAnnnotation(Annotation annotation, AnnotationType type = AnnotationType.Text)
    {
      if (annotation != null)
      {
        type = annotation.Type;
      }
      Image img = new Image();
      img.VerticalAlignment = VerticalAlignment.Top;
      img.HorizontalAlignment = HorizontalAlignment.Left;
      img.Stretch = Stretch.None;
      img.AllowDrop = true;
      img.Tag = annotation;
      mainGrid.Children.Add(img);
      if (type == AnnotationType.Text)
      {
        img.Source = new BitmapImage(new Uri("/ideaSpaceApplication;component/Resources/note.png", UriKind.Relative));
        img.Margin = new Thickness(annotation.Position.X - 64, annotation.Position.Y - 64, 0, 0);
      }
      if (type == AnnotationType.Audio)
      {
        img.Source = new BitmapImage(new Uri("/ideaSpaceApplication;component/Resources/play_audio.png", UriKind.Relative));        
        img.Margin = new Thickness(annotation.Position.X - 32, annotation.Position.Y - 32, 0, 0);
      }
      return img;
    }

    public canvasWindow(Project _project, DataItem di)
      : this()
    {
      project = _project;
      item = di;
      mainImage.Source = new BitmapImage(new Uri(item.Content));
      if (item.handAnnotation != null)
      {
        StrokeCollectionConverter converter = new StrokeCollectionConverter();
        surfaceInkCanvas1.Strokes = (StrokeCollection)converter.ConvertFromString(null, null, item.handAnnotation);
      }
      foreach (Annotation annotation in item.Annotations)
      {
        createAnnnotation(annotation);
      }
    }

    private void createGridOfColor()
    {
      PropertyInfo[] props = typeof(Brushes).GetProperties(BindingFlags.Public | BindingFlags.Static);
      // Create individual items
      foreach (PropertyInfo p in props)
      {
        Button b = new Button();
        b.Background = (SolidColorBrush)p.GetValue(null, null);
        b.Foreground = Brushes.Transparent;
        b.BorderBrush = Brushes.Black;
        b.BorderThickness = new Thickness(2);
        b.Click += new RoutedEventHandler(b_Click);
        b.MouseUp += new MouseButtonEventHandler(b_Click);
        b.TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(t_Click);
        this.ugColors.Children.Add(b);
      }

    }

    Color currColor = Colors.Black;

    private void b_Click(object sender, RoutedEventArgs e)
    {
      SolidColorBrush sb = (SolidColorBrush)(sender as Button).Background;
      surfaceInkCanvas1.DefaultDrawingAttributes.Color = sb.Color;
      colorSelector.Visibility = Visibility.Hidden;
    }
    private void t_Click(object sender, TouchEventArgs e)
    {
      SolidColorBrush sb = (SolidColorBrush)(sender as Button).Background;
      surfaceInkCanvas1.DefaultDrawingAttributes.Color = sb.Color;
      colorSelector.Visibility = Visibility.Hidden;
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

    private void surfaceButton2_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void label1_MouseUp(object sender, MouseButtonEventArgs e)
    {

      colorSelector.Visibility = Visibility.Visible;
    }

    private void save()
    {
      try
      {
        StrokeCollectionConverter converter = new StrokeCollectionConverter();
        item.handAnnotation = converter.ConvertToString(surfaceInkCanvas1.Strokes);

        //surfaceInkCanvas1.Strokes[0].StylusPoints[0]
        Close();
      }
      catch (Exception exc)
      {
        MessageBox.Show(exc.Message, Title);
      }
    }

    private void ugColors_TouchUp(object sender, TouchEventArgs e)
    {
      colorSelector.Visibility = Visibility.Hidden;
    }

    private void colorButton_Click(object sender, RoutedEventArgs e)
    {
      colorSelector.Visibility = Visibility.Visible;
    }

    const string placeholder_text = "placeholder text";

    private void mainImage_TouchDown(object sender, InputEventArgs e)
    {
      if (recorder.RecordingState == RecordingState.Recording)
      {
        return;
      }

      Point p = getPosition(e, mainGrid);
      if (text_annotating)
      {
        project.log(Activity.TextAnnotation);

        Annotation annotation = item.TextAnnotate(p, placeholder_text);
        active_image = createAnnnotation(annotation);

        img_Click(active_image);

        text_annotating = false;
        annotateImageButton.Content = "Text Annotate";
        surfaceInkCanvas1.EditingMode = SurfaceInkEditingMode.Ink;
        e.Handled = true;
      }
      if (audio_annotating)
      {
        string waveFileName = Path.Combine(project.mediaFolder(), Guid.NewGuid().ToString() + ".wav");
        Annotation audio_annotation = item.AudioAnnotate(p, waveFileName);
        active_audio = createAnnnotation(audio_annotation);
        active_audio.Source = new BitmapImage(new Uri("/ideaSpaceApplication;component/Resources/recording_microphone.png", UriKind.Relative));
        try
        {

          project.log(Activity.StartRecordAudio);

          recorder.BeginMonitoring(0);
          recorder.BeginRecording(waveFileName);
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
        }
        audio_annotating = false;
        recordAudioButton.Content = "Audio Annotate";
        surfaceInkCanvas1.EditingMode = SurfaceInkEditingMode.Ink;
        e.Handled = true;
      }
    }

    private Point getPosition(InputEventArgs e, object element)
    {
      if (element == null)
        return new Point();
      TouchEventArgs tea = e as TouchEventArgs;
      MouseButtonEventArgs mea = e as MouseButtonEventArgs;
      Point p = new Point();
      if (tea != null)
      {
        p = tea.GetTouchPoint(element as IInputElement).Position;
      }
      if (mea != null)
      {
        p = mea.GetPosition(element as IInputElement);
      }
      return p;
    }

    void img_Click(Image sender)
    {
      active_image = sender;
      Keyboard.ClearFocus();
        //Added by NN
      editAnnotation.Margin = new Thickness(sender.Margin.Left + 128, sender.Margin.Top, 0, 0);
        ////////////////
      editAnnotation.Visibility = Visibility.Visible;
      editAnnotation.Text = (active_image.Tag as Annotation).Content;
    }

    void audio_click(Image sender)
    {
      Image clicked_audio = sender as Image;
      if (recorder.RecordingState == RecordingState.Recording)
      {
        if (clicked_audio != active_audio)
        {
          return;
        }
        recorder.Stop();
        project.log(Activity.StopRecordAudio);
        project.log(Activity.SoundAnnotation);
        active_audio.Source = new BitmapImage(new Uri("/ideaSpaceApplication;component/Resources/play_audio.png", UriKind.Relative));
      }
      active_audio = clicked_audio;
      try
      {
        audioPlayer.Stop();
        audioPlayer.Source = new Uri((active_audio.Tag as Annotation).Content, UriKind.Absolute);
        audioPlayer.Position = new TimeSpan(0, 0, 0, 0);
        audioPlayer.Play();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    bool text_annotating = false;
    private void annotateImageButton_Click(object sender, RoutedEventArgs e)
    {
      if (text_annotating)
      {
        text_annotating = false;
        annotateImageButton.Content = "Text Annotate";
        surfaceInkCanvas1.EditingMode = SurfaceInkEditingMode.Ink;
      }
      else
      {
        text_annotating = true;
        annotateImageButton.Content = "Stop Annotating";
        surfaceInkCanvas1.EditingMode = SurfaceInkEditingMode.None;
      }
    }

    Image active_audio = null;

    private void editAnnotation_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (
        !editAnnotation.Text.Equals(placeholder_text, StringComparison.OrdinalIgnoreCase)
        &&
        (editAnnotation.Text != "")
        )
      {
        (active_image.Tag as Annotation).Content = editAnnotation.Text;
      }
    }

    private void editAnnotation_GotFocus(object sender, RoutedEventArgs e)
    {
      KeyboardHelper.showKeyboard();

      if (editAnnotation.Text.Equals(placeholder_text, StringComparison.OrdinalIgnoreCase))
      {
        editAnnotation.Text = string.Empty;
        e.Handled = true;
      }
      editAnnotation.SelectionStart = editAnnotation.Text.Length;
    }

    private void editAnnotation_LostFocus(object sender, RoutedEventArgs e)
    {
        KeyboardHelper.hideKeyboard();

        editAnnotation.Visibility = Visibility.Hidden;
    }

    private void eraserButton_Click(object sender, RoutedEventArgs e)
    {
      if (surfaceInkCanvas1.EditingMode == SurfaceInkEditingMode.Ink)
      {
        eraserButton.Content = "Edit Mode";
        surfaceInkCanvas1.EditingMode = SurfaceInkEditingMode.EraseByStroke;
      }
      else
      {
        eraserButton.Content = "Eraser Mode";
        surfaceInkCanvas1.EditingMode = SurfaceInkEditingMode.Ink;

      }
    }

    private void recordAudioButton_Click(object sender, RoutedEventArgs e)
    {

      if (audio_annotating)
      {
        audio_annotating = false;
        recordAudioButton.Content = "Audio Annotate";
        surfaceInkCanvas1.EditingMode = SurfaceInkEditingMode.Ink;
      }
      else
      {
        audio_annotating = true;
        recordAudioButton.Content = "Stop Annotating";
        surfaceInkCanvas1.EditingMode = SurfaceInkEditingMode.None;
      }
    }

    private void takeSnapshotButton_Click(object sender, RoutedEventArgs e)
    {
      project.log(Activity.CanvasSnapshot);
      string snapshot_filename = Path.Combine(project.mediaFolder(), Guid.NewGuid().ToString() + ".jpeg");
      projectWindow.snapshot(snapshot_filename, (int) surfaceInkCanvas1.Width, (int) surfaceInkCanvas1.Height);
      facebookWindow fW = new facebookWindow(project, snapshot_filename, PostType.Image);
      fW.ShowDialog();
    }

    private void saveButton_Click(object sender, RoutedEventArgs e)
    {
      save();
    }

    private void surfaceInkCanvas1_StrokeErased(object sender, RoutedEventArgs e)
    {
      editAnnotation.Visibility = Visibility.Hidden;
      project.log(Activity.DeleteFreehandAnnotation);
    }

    private void surfaceInkCanvas1_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
    {
      editAnnotation.Visibility = Visibility.Hidden;
      project.log(Activity.FreehandAnnotation);
    }
    #region remove annotations

    private Image draggedElement;
        
    private void DragSourcePreviewInputDeviceDown(object sender, InputEventArgs e)
    {
      FrameworkElement findSource = e.OriginalSource as FrameworkElement;
      draggedElement = null;

      // Find the ScatterViewItem object that is being touched.
      while (draggedElement == null && findSource != null)
      {
        if ((draggedElement = findSource as Image) == null)
        {
          findSource = VisualTreeHelper.GetParent(findSource) as FrameworkElement;
        }
      }

      if (draggedElement == null)
      {
        return;
      }
      Annotation data = draggedElement.Tag as Annotation;
      if (data.Type == AnnotationType.Text)
      {
        img_Click(draggedElement);
      }
      else
      {
        audio_click(draggedElement);
      }

      // If the data has not been specified as draggable, 
      // or the ScatterViewItem cannot move, return.
      if (data == null) // || !data.CanDrag 
      {
        return;
      }

      // Set the dragged element. This is needed in case the drag operation is canceled.

      // Create the cursor visual.
      ContentControl cursorVisual = new ContentControl()
      {
        Content = draggedElement,
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
      FrameworkElement dragSource = draggedElement;

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
            data,
        // The input devices that start dragging the cursor.
            devices,
        // The allowed drag-and-drop effects of the operation.
            DragDropEffects.Move);

      // If the cursor was created, the drag-and-drop operation was successfully started.
      if (cursor != null)
      {
        // This event has been handled.
        e.Handled = true;
      }
    }
    
    private void OnDropTargetDrop(object sender, SurfaceDragDropEventArgs e)
    {
      MessageBoxResult result = MessageBox.Show("Are you sure about deleting the annotation?", "Delete Annotation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
      if (result == MessageBoxResult.Yes)
      {
        Image toBeRemoved = e.Cursor.DragSource as Image;
        Annotation annotation = toBeRemoved.Tag as Annotation;
        item.removeAnnotation(annotation);
        mainGrid.Children.Remove(toBeRemoved as UIElement);
        if (annotation.Type == AnnotationType.Text)
        {
          project.log(Activity.DeleteTextAnnotation);
          active_image = null;
          editAnnotation.Visibility = Visibility.Hidden;
        }
        if (annotation.Type == AnnotationType.Audio)
        {
          project.log(Activity.DeleteSoundAnnotation);
          active_audio = null;
        }
      }
      if (draggedElement != null)
      {
        draggedElement.ReleaseAllCaptures();
        draggedElement = null;
      }
      e.Handled = true;
    }
    #endregion
  }
}