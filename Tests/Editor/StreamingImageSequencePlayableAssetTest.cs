﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.StreamingImageSequence;
using UnityEngine.TestTools;
using UnityEngine.Timeline;
using UnityEditor.Timeline;

namespace UnityEditor.StreamingImageSequence.Tests {

internal class StreamingImageSequencePlayableAssetTest {

    [UnityTest]
    public IEnumerator CreatePlayableAsset() {
        PlayableDirector director = EditorUtilityTest.NewSceneWithDirector();
        TimelineClip clip = EditorUtilityTest.CreateTestTimelineClip(director);
        StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
        Assert.IsNotNull(sisAsset);
        
        //Test the track immediately
        StreamingImageSequenceTrack track = clip.parentTrack as StreamingImageSequenceTrack;
        Assert.IsNotNull(track);
        Assert.IsNotNull(track.GetActivePlayableAsset());
        
        yield return null;

        int numImages = sisAsset.GetNumImages();
        Assert.IsTrue(numImages > 0);
        
        

        //Test that there should be no active PlayableAsset at the time above what exists in the track.
        director.time = clip.start + clip.duration + 1;
        yield return null;
        
        Assert.IsNull(track.GetActivePlayableAsset()); 
        

        EditorUtilityTest.DestroyTestTimelineAssets(clip);
        yield return null;
    }
    
    
//----------------------------------------------------------------------------------------------------------------------                
    [UnityTest]
    public IEnumerator ResizePlayableAsset() {
        PlayableDirector director = EditorUtilityTest.NewSceneWithDirector();
        TimelineClip                        clip     = EditorUtilityTest.CreateTestTimelineClip(director);
        StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
        Assert.IsNotNull(sisAsset);
        TimelineClipSISData timelineClipSISData = sisAsset.GetBoundTimelineClipSISData();
        yield return null;
        
        timelineClipSISData.RequestFrameMarkers(true); 
        Undo.IncrementCurrentGroup(); //the base of undo is here. FrameMarkerVisibility is still true after undo
        TimelineEditor.Refresh(RefreshReason.ContentsModified);
        yield return null;

        //Original length
        TrackAsset trackAsset = clip.parentTrack;
        Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
        double origClipDuration = clip.duration;

        //Resize longer
        EditorUtilityTest.ResizeSISTimelineClip(clip, origClipDuration + 3.0f); yield return null;
        Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());

        //Undo
        EditorUtilityTest.UndoAndRefreshTimelineEditor(); yield return null;
        Assert.AreEqual(origClipDuration, clip.duration);
        Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
        
        //Resize shorter
        EditorUtilityTest.ResizeSISTimelineClip(clip, Mathf.Max(0.1f, ( (float)(origClipDuration) - 3.0f))); yield return null;
        Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
        
        //Undo
        EditorUtilityTest.UndoAndRefreshTimelineEditor(); yield return null;
        Assert.AreEqual(origClipDuration, clip.duration);
        Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
        
        
        EditorUtilityTest.DestroyTestTimelineAssets(clip);
        yield return null;
    }
//----------------------------------------------------------------------------------------------------------------------                

    [UnityTest]
    public IEnumerator ReloadPlayableAsset() {
        PlayableDirector                    director = EditorUtilityTest.NewSceneWithDirector();
        TimelineClip                        clip     = EditorUtilityTest.CreateTestTimelineClip(director);
        StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
        Assert.IsNotNull(sisAsset);
        
        string folder = sisAsset.GetFolder();
        Assert.IsNotNull(folder);
        int numOriginalImages = sisAsset.GetNumImages();
        Assert.Greater(numOriginalImages,0);

        
        List<string> testImages = sisAsset.FindImages(folder);
        List<string> copiedImagePaths = new List<string>(testImages.Count);
        foreach (string imageFileName in testImages) {
            string src = Path.Combine(folder, imageFileName);
            string dest = Path.Combine(folder, "Copied_" + imageFileName);
            File.Copy(src,dest);
            copiedImagePaths.Add(dest);
        }

        yield return null;
        sisAsset.Reload();
        
        yield return null;
        Assert.AreEqual(numOriginalImages * 2 , sisAsset.GetNumImages());

        //Cleanup
        foreach (string imagePath in copiedImagePaths) {
            File.Delete(imagePath);
        }
                   

        EditorUtilityTest.DestroyTestTimelineAssets(clip);
        yield return null;
        
    }

//----------------------------------------------------------------------------------------------------------------------    
    
    [UnityTest]
    public IEnumerator ImportFromStreamingAssets() {
        PlayableDirector                    director = EditorUtilityTest.NewSceneWithDirector();
        TimelineClip                        clip     = EditorUtilityTest.CreateTestTimelineClip(director);
        StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
        Assert.IsNotNull(sisAsset);
        
        //Copy test data to streamingAssetsPath
        const string  DEST_FOLDER_NAME      = "ImportFromStreamingAssetsTest";
        string        streamingAssetsFolder = AssetEditorUtility.NormalizeAssetPath(Application.streamingAssetsPath);
        string        destFolderGUID        = AssetDatabase.CreateFolder(streamingAssetsFolder, DEST_FOLDER_NAME);
        string        destFolder            = AssetDatabase.GUIDToAssetPath(destFolderGUID);
        int numImages = sisAsset.GetNumImages();
        for (int i = 0; i < numImages; ++i) {
            string src = sisAsset.GetImageFilePath(i);
            Assert.IsNotNull(src);
            string dest = Path.Combine(destFolder, Path.GetFileName(src));
            File.Copy(src, dest,true);
        }

        AssetDatabase.Refresh();        
        yield return null;
        
        ImageSequenceImporter.ImportImages(destFolder, sisAsset);
        yield return null;
               
        Assert.AreEqual(destFolder, sisAsset.GetFolder());

        //Cleanup
        AssetDatabase.DeleteAsset(destFolder);       
        EditorUtilityTest.DestroyTestTimelineAssets(clip);
        yield return null;
        
    }
    
}

} //end namespace
