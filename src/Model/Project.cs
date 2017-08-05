using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Controls;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Collections;

namespace ideaSpaceApplication.Model
{

  [Serializable()]
  public class Project : ISerializable
  {
    public String name { set; get; }
    public String filename { set; get; }
    public ObservableCollection<DataItem> imageFilenames { set; get; }
    private Dictionary<Activity, string> activities;
    public Project()
    {
      imageFilenames = new ObservableCollection<DataItem>();
      imageFilenames.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(imageFilenames_CollectionChanged);
      filename = @"c:\users\maysam\Documents\untitled.ids";
      name = "untitled";
      activities = new Dictionary<Activity, string>();
      activities.Add(Activity.OpenProject, "Project Opened");
      activities.Add(Activity.ScanImage, "Scan image to Library");
      activities.Add(Activity.LibrarySnapshot, "Take snapshot of library");
      activities.Add(Activity.CanvasSnapshot, "Take snapshot of canvas");
      activities.Add(Activity.OpenCanvas, "Open Canvas");
      activities.Add(Activity.LoadImageCanvas, "Load Image to Canvas");
      activities.Add(Activity.DeleteImageCanvas, "Delete Image from Canvas");
      activities.Add(Activity.LoadImageLibrary, "Load Image from File");
      activities.Add(Activity.DeleteImageLibrary, "Delete Image from Library");
      activities.Add(Activity.TextAnnotation, "Text Annotation added");
      activities.Add(Activity.SoundAnnotation, "Sound Annotation added");
      activities.Add(Activity.FreehandAnnotation, "Freehand annotation added");
      activities.Add(Activity.DeleteTextAnnotation, "Text Annotation deleted");
      activities.Add(Activity.DeleteSoundAnnotation, "Sound Annotation deleted");
      activities.Add(Activity.DeleteFreehandAnnotation, "Freehand annotation deleted");
      activities.Add(Activity.StartRecordVideo, "Record Video started");
      activities.Add(Activity.StopRecordVideo, "Record Video stopped");
      activities.Add(Activity.StartRecordAudio, "Record Audio started");
      activities.Add(Activity.StopRecordAudio, "Record Audio stopped");
      activities.Add(Activity.UploadFacebookVideo, "Video Uploaded to Facebook");
      activities.Add(Activity.UploadFacebookImage, "Image Uploaded to Facebook");
      activities.Add(Activity.SaveVideo, "Video Saved locally");
      activities.Add(Activity.SaveImage, "Image Saved locally");
    }

    void imageFilenames_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      save();
    }
    private bool loading = false;
    public Project(SerializationInfo info, StreamingContext context)
      : this()
    {
      if (info == null)
        throw new ArgumentNullException("info");
      loading = true;
      Collection<DataItem> tempItems = new Collection<DataItem>();
      name = info.GetString("name");
      note = info.GetString("note");
      string fileNames = info.GetString("imageFilenames");
      IEnumerator enumerator = fileNames.Split(';').GetEnumerator();
      while (enumerator.MoveNext() && enumerator.Current != null)
      {
        DataItem di = new DataItem(enumerator.Current as String, true);
        enumerator.MoveNext();
        di.handAnnotation = enumerator.Current as String;
        enumerator.MoveNext();
        int annotations_count = Int16.Parse(enumerator.Current as String);
        while (annotations_count-- > 0)
        {
          enumerator.MoveNext();
          Annotation annotation = new Annotation();
          if (enumerator.Current as String == "Text")
            annotation.Type = AnnotationType.Text;
          if (enumerator.Current as String == "Audio")
            annotation.Type = AnnotationType.Audio;
          enumerator.MoveNext();
          annotation.Content = enumerator.Current as String;
          enumerator.MoveNext();
          Double x = Double.Parse(enumerator.Current as String);
          enumerator.MoveNext();
          Double y = Double.Parse(enumerator.Current as String);
          annotation.Position = new Point(x, y);
          di.Annotations.Add(annotation);
        }
        imageFilenames.Add(di);
      }
      loading = false;
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
      info.AddValue("name", this.name);
      ArrayList fileNames = new ArrayList(imageFilenames.Count);
      foreach (var imageFilename in imageFilenames)
      {
        fileNames.Add(imageFilename.Content);
        fileNames.Add(imageFilename.handAnnotation);
        fileNames.Add(imageFilename.Annotations.Count);
        foreach (Annotation annotation in imageFilename.Annotations)
        {
          fileNames.Add(annotation.Type);
          fileNames.Add(annotation.Content);
          fileNames.Add(annotation.Position.X);
          fileNames.Add(annotation.Position.Y);
        }
      }
      info.AddValue("imageFilenames", String.Join(";", fileNames.ToArray()));
      info.AddValue("note", note);

    }

    public void save(String _filename)
    {
      filename = _filename;
      save();
    }
    public string mediaFolder()
    {
      string media_folder = Path.Combine(Path.GetDirectoryName(filename), "media");
      if (!Directory.Exists(media_folder))
      {
        Directory.CreateDirectory(media_folder);
      }
      return media_folder;
    }
    public void save()
    {
      if (loading)
        return;
      if (filename == null)
      {
        MessageBox.Show("Cannot save without a file");
      }
      else
      {
        string backup_folder = Path.Combine(Path.GetDirectoryName(filename), "backups");
        if (!Directory.Exists(backup_folder))
        {
          Directory.CreateDirectory(backup_folder);
        }
        if (File.Exists(filename))
        {
          string backup_filename = Path.Combine(backup_folder, Path.GetFileNameWithoutExtension(filename) + "." + DateTime.Now.Ticks + ".ids.backup");
          File.Copy(filename, backup_filename);
        }
        System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);
        BinaryFormatter bFormatter = new BinaryFormatter();
        bFormatter.Serialize(fs, this);
        fs.Close();
      }
    }

    public void log(Activity activity)
    {
      if (filename == null)
      {
      }
      else
      {
        if(activity == Activity.OpenProject)
          File.AppendAllText(filename + ".log", Environment.NewLine);
        File.AppendAllText(filename + ".log", DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString() + ", " + activities[activity] + Environment.NewLine);
      }
    }

    public string note { get; set; }
  }
}
