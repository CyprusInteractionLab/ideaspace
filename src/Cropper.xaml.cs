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
using System.Drawing;

namespace ideaSpaceApplication
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class Cropper : UserControl
  {

    #region CropperStyle Dependancy property

    /// <summary>
    /// A DP for the Cropp Rectangle Style
    /// </summary>
    public Style CropperStyle
    {
      get { return (Style)GetValue(CropperStyleProperty); }
      set { SetValue(CropperStyleProperty, value); }
    }

    /// <summary>
    /// register the DP
    /// </summary>
    public static readonly DependencyProperty CropperStyleProperty =
        DependencyProperty.Register(
        "CropperStyle",
        typeof(Style),
        typeof(Cropper),
        new UIPropertyMetadata(null, new PropertyChangedCallback(OnCropperStyleChanged)));

    /// <summary>
    /// The callback that actually changes the Style if one was provided
    /// </summary>
    /// <param name="depObj">Cropper</param>
    /// <param name="e">The event args</param>
    static void OnCropperStyleChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
    {
      Style s = e.NewValue as Style;
      if (s != null)
      {
        Cropper uc = (Cropper)depObj;
        uc.selectCanvForImg.CropperStyle = s;
      }
    }
    #endregion

    #region Instance fields
    private string ImgUrl = "";
    private BitmapImage bmpSource = null;
    private SelectionCanvas selectCanvForImg = null;
    private DragCanvas dragCanvasForImg = null;
    private System.Windows.Controls.Image img = null;
    private Shape rubberBand;
    private float rubberBandLeft;
    private float rubberBandTop;
    private float zoomFactor = 0.7f;
    #endregion

    #region Ctor
    public Cropper()
    {
      InitializeComponent();
      //this.Unloaded += new RoutedEventHandler(Cropper_Unloaded);
      selectCanvForImg = new SelectionCanvas();
      selectCanvForImg.CropImage += new RoutedEventHandler(selectCanvForImg_CropImage);
      dragCanvasForImg = new DragCanvas();
    }


    #endregion

    #region Public properties
    public string ImageUrl
    {
      get { return this.ImgUrl; }
      set
      {

        ImgUrl = value;
        //grdCroppedImage.Visibility = Visibility.Hidden;
        createImageSource();
        createSelectionCanvas();
        //apply the default style if the user of this control didnt supply one
        if (CropperStyle == null)
        {
          Style s = gridMain.TryFindResource("defaultCropperStyle") as Style;
          if (s != null)
          {
            CropperStyle = s;
          }
        }

      }
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Deletes all occurences of previous unused temp files from the
    /// current temporary path
    /// </summary>
    /// <param name="tempPath">The temporary file path</param>
    /// <param name="fixedTempName">The file name part to search for</param>
    /// <param name="CurrentFixedTempIdx">The current temp file suffix</param>
    public void CleanUp(string tempPath, string fixedTempName, long CurrentFixedTempIdx)
    {
      //clean up the single temporary file created
      try
      {
        string filename = "";
        for (int i = 0; i < CurrentFixedTempIdx; i++)
        {
          filename = tempPath + fixedTempName + i.ToString() + ".jpg";
          //File.Delete(filename);
        }
      }
      catch (Exception)
      {
      }
    }

    /// <summary>
    /// Popup form Cancel clicked, so created the SelectionCanvas to start again
    /// </summary>
    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      //grdCroppedImage.Visibility = Visibility.Hidden;
      createSelectionCanvas();
    }

    /// <summary>
    /// creates the selection canvas, where user can draw
    /// selection rectangle
    /// </summary>
    private void createSelectionCanvas()
    {
      createImageSource();
      selectCanvForImg.Width = bmpSource.Width;
      selectCanvForImg.Height = bmpSource.Height;
      selectCanvForImg.Children.Clear();
      selectCanvForImg.rubberBand = null;
      selectCanvForImg.Children.Add(img);
      svForImg.Width = selectCanvForImg.Width;
      svForImg.Height = selectCanvForImg.Height;
      svForImg.Content = selectCanvForImg;
    }

    public void resetCrop()
    {
      createSelectionCanvas();
    }

    /// <summary>
    /// Creates the Image source for the current canvas
    /// </summary>
    private void createImageSource()
    {
      bmpSource = new BitmapImage(new Uri(ImgUrl));
      img = new System.Windows.Controls.Image();
      img.Source = bmpSource;
      //if there was a Zoom Factor applied
      img.RenderTransform = new ScaleTransform(zoomFactor, zoomFactor);
    }

    /// <summary>
    /// creates the drag canvas, where user can drag the
    /// selection rectangle
    /// </summary>
    private void createDragCanvas()
    {
      dragCanvasForImg.Width = bmpSource.Width;
      dragCanvasForImg.Height = bmpSource.Height;
      svForImg.Width = dragCanvasForImg.Width;
      svForImg.Height = dragCanvasForImg.Height;
      createImageSource();
      selectCanvForImg.Children.Remove(rubberBand);
      dragCanvasForImg.Children.Clear();
      dragCanvasForImg.Children.Add(img);
      dragCanvasForImg.Children.Add(rubberBand);
      svForImg.Content = dragCanvasForImg;
    }

    /// <summary>
    /// Raised by the <see cref="selectionCanvas">selectionCanvas</see>
    /// when the new crop shape (rectangle) has been drawn. This event
    /// then replaces the current selectionCanvas with a <see cref="DragCanvas">DragCanvas</see>
    /// which can then be used to drag the crop area around within a Canvas
    /// </summary>
    private void selectCanvForImg_CropImage(object sender, RoutedEventArgs e)
    {
      rubberBand = (Shape)selectCanvForImg.Children[1];
      createDragCanvas();
    }

    /// <summary>
    /// User cancelled out of the popup, so go back to showing original image
    /// </summary>
    private void lblExit_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      //grdCroppedImage.Visibility = Visibility.Hidden;
      createSelectionCanvas();
    }

    /// <summary>
    /// Saves the cropped image area to a temp file, and shows a confirmation
    /// popup from where the user may accept or reject the cropped image.
    /// If they accept the new cropped image will be used as the new image source
    /// for the current canvas, if they reject the crop, the existing image will
    /// be used for the current canvas
    /// </summary>
    public string SaveCroppedImage(string tempFileName)
    {
      //if (popUpImage.Source!=null)                popUpImage.Source = null; 

      try
      {
        rubberBandLeft = (float)Canvas.GetLeft(rubberBand);
        rubberBandTop = (float)Canvas.GetTop(rubberBand);
        //create a new .NET 2.0 bitmap (which allowing saving) based on the bound bitmap url
        using (System.Drawing.Bitmap source = new System.Drawing.Bitmap(ImgUrl))
        {
          //create a new .NET 2.0 bitmap (which allowing saving) to store cropped image in, should be 
          //same size as rubberBand element which is the size of the area of the original image we want to keep
          using (System.Drawing.Bitmap target = new System.Drawing.Bitmap((int)rubberBand.Width, (int)rubberBand.Height))
          {
            //create a new destination rectange
            GraphicsUnit unit = GraphicsUnit.Pixel;
            System.Drawing.RectangleF recDest = target.GetBounds(ref unit); // new System.Drawing.RectangleF(0.0f, 0.0f, (float)target.Width, (float)target.Height);
            System.Drawing.RectangleF recSrc = new System.Drawing.RectangleF(rubberBandLeft / zoomFactor, rubberBandTop / zoomFactor, (float)rubberBand.Width / zoomFactor, (float)rubberBand.Height / zoomFactor);

            using (System.Drawing.Graphics gfx = System.Drawing.Graphics.FromImage(target))
            {
              gfx.DrawImage(source, recDest, recSrc, unit);
            }
            //create a new temporary file, and delete all old ones prior to this new temp file
            //This is is a hack that I had to put in, due to GDI+ holding on to previous 
            //file handles used by the Bitmap.Save() method the last time this method was run.
            //This is a well known issue see http://support.microsoft.com/?id=814675 for example
            //do the clean
            // CleanUp(tempFileName, fixedTempName, fixedTempIdx);
            //Due to the so problem above, which believe you me I have tried and tried to resolve
            //I have tried the following to fix this, incase anyone wants to try it
            //1. Tried reading the image as a strea of bytes into a new bitmap image
            //2. I have tried to use teh WPF BitmapImage.Create()
            //3. I have used the trick where you use a 2nd Bitmap (GDI+) to be the newly saved
            //   image
            //
            //None of these worked so I was forced into using a few temp files, and pointing the 
            //cropped image to the last one, and makeing sure all others were deleted.
            //Not ideal, so if anyone can fix it please this I would love to know. So let me know

            target.Save(tempFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            ImageUrl = tempFileName;
            return tempFileName;
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return null;
      }
    }
    #endregion
  }
}
