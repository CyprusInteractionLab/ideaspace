using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;

namespace ideaSpaceApplication.Model
{
  [Serializable]
  public class Annotation
  {
    public AnnotationType Type { get; set; }
    public string Content { get; set; }
    public Point Position { get; set; }
  }
}
