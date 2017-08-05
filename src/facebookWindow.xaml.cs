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
using System.Windows.Shapes;
using Facebook;
using System.Windows.Navigation;
using System.IO;
using Microsoft.Win32;
using ideaSpaceApplication.Model;

namespace ideaSpaceApplication
{
  public enum PostType
  {
    Image, Video
  }
  /// <summary>
  /// Interaction logic for facebookWindow.xaml
  /// </summary>
  public partial class facebookWindow : Window
  {

    const string YOURAPPID = "238891892926387";
    public static string AccessToken { get; set; }
    private FacebookClient FBClient;
    public string filename { get; set; }
    public PostType postType { get; set; }
    
    private Project project;

    public facebookWindow()
    {
      InitializeComponent();
      mainImage.Visibility = Visibility.Visible;
      mainImage.Source = new BitmapImage(new Uri("/ideaSpaceApplication;component/Resources/audio_banner.jpg", UriKind.Relative));
      filename = @"C:\Users\Maysam\Dropbox\Fernando Loizides\ideaSpace\ideaSpaceApplication\Resources\audio_banner.jpg";
    }

    public facebookWindow(Project _project, string _filename, PostType _type)
    {
      InitializeComponent();
      project = _project;
      filename = _filename;
      postType = _type;
      if (_type == PostType.Image)
      {
        mainImage.Visibility = Visibility.Visible;
        mainImage.Source = new BitmapImage(new Uri(filename, UriKind.Absolute));
      }
      else
      {
        videoPlayer.Visibility = Visibility.Visible;
        videoPlayer.Source = filename;
      }
    }

    private void webBrowser_Navigated(object sender, NavigationEventArgs e)
    {
      if (e.Uri.ToString().StartsWith("http://www.facebook.com/connect/login_success.html"))
      {
        AccessToken = e.Uri.Fragment.Split('&')[0].Replace("#access_token=", "");
        webBrowser.Visibility = Visibility.Hidden;
        facebookIt();
      }
    }

    private void facebookIt()
    {
      FBClient = new FacebookClient(AccessToken);
      var bytes = File.ReadAllBytes(filename);
      var postInfo = new Dictionary<string, object>();
      FBClient.PostCompleted += (o, args) =>
      {        
        MessageBox.Show("Posted successfully");
        Close();
      };
      progressbar.Visibility = Visibility.Visible;
      MessageBox.Show("Please wait until posting is finished");
      if (postType == PostType.Image)
      {
        FacebookMediaObject facebookUploader = new FacebookMediaObject { FileName = filename, ContentType = "image/png" };
        facebookUploader.SetValue(bytes);
        postInfo.Add("image", facebookUploader);
        FBClient.Post("/me/photos", postInfo);
        project.log(Activity.UploadFacebookImage);
      }
      else
      {
        FacebookMediaObject facebookUploader = new FacebookMediaObject { FileName = filename, ContentType = "video/3gpp" };
        facebookUploader.SetValue(bytes);
        postInfo.Add("video", facebookUploader);
        FBClient.Post("/me/videos", postInfo);
        project.log(Activity.UploadFacebookVideo);
      }
      Close();
    }

    private void discardButton_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void facebookAndSaveButton_Click(object sender, RoutedEventArgs e)
    {
      if (saveIt())
      {
        facebookButton_Click(sender, e);
      }  
    }

    private void facebookButton_Click(object sender, RoutedEventArgs e)
    {

      if (AccessToken != null)
      {
        facebookIt();
      }
      else
      {
        webBrowser.Visibility = Visibility.Visible;
        webBrowser.Navigate(new Uri("https://graph.facebook.com/oauth/authorize?client_id=" + YOURAPPID + "&redirect_uri=http://www.facebook.com/connect/login_success.html&scope=read_stream,publish_actions&type=user_agent&display=popup").AbsoluteUri);
      }
    }
    private bool saveIt()
    {
      if (filename == null)
      {
        return false;
      }
      SaveFileDialog saveFileDialog1 = new SaveFileDialog();
      if (postType == PostType.Image)
      {
        saveFileDialog1.Filter = "Images |*.jpeg";
      }
      else
      {
        saveFileDialog1.Filter = "Videos |*.avi";
      }
      saveFileDialog1.Title = "Save Locally";

      if (saveFileDialog1.ShowDialog().Value)
      {
        File.Copy(filename, saveFileDialog1.FileName, true);
        if (postType == PostType.Image)
        {
          project.log(Activity.SaveImage);
        }
        else
        {
          project.log(Activity.SaveVideo);
        }
        return true;
      }
      return false;
    }
    private void saveButton_Click(object sender, RoutedEventArgs e)
    {
      e.Handled = saveIt();
    }
  }
}
