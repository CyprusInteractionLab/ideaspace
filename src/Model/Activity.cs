using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ideaSpaceApplication.Model
{
    public enum Activity
    {
        OpenProject,
        ScanImage,
        LibrarySnapshot,
        CanvasSnapshot,
        OpenCanvas,
        LoadImageCanvas,
        DeleteImageCanvas,
        LoadImageLibrary,
        DeleteImageLibrary,
        TextAnnotation,
        SoundAnnotation,
        FreehandAnnotation,
        DeleteTextAnnotation,
        DeleteSoundAnnotation,
        DeleteFreehandAnnotation,
        StartRecordVideo,
        StopRecordVideo,
        StartRecordAudio,
        StopRecordAudio,
        UploadFacebookVideo,
        UploadFacebookImage,
        SaveVideo,
        SaveImage
    }
}
