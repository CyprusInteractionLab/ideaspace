using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ideaSpaceApplication.Model;
using System.Windows;
using System.Collections;

namespace ideaSpaceApplication
{
  public enum AnnotationType { Text, Audio }

  [Serializable()]
  public class DataItem
  {
    public DataItem(string path, bool canDrag)
    {
      this.Content = path;
      Annotations = new List<Annotation>();
    }

    public string Content { get; set; }
    public object DraggedElement { get; set; }
    public List<Annotation> Annotations { get; set; }
    public string handAnnotation { get; set; }

    public Annotation TextAnnotate(Point point, string text)
    {
      Annotation item = new Annotation();
      item.Type = AnnotationType.Text;
      item.Content = text;
      item.Position = point;
      Annotations.Add(item);
      return item;
    }

    public Annotation AudioAnnotate(Point point, string path)
    {
      Annotation item = new Annotation();
      item.Type = AnnotationType.Audio;
      item.Content = path;
      item.Position = point;
      Annotations.Add(item);
      return item;
    }
    public Boolean removeAnnotation(Annotation item)
    {
      return Annotations.Remove(item);
    }
  }
}
