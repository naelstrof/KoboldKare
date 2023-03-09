using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class Screenshotter {
    public delegate void ScreenshotFinishedAction(Texture2D screenshot);
    public static void GetScreenshot(ScreenshotFinishedAction action) {
        GameManager.instance.StartCoroutine(ScreenshotRoutine(action));
    }
    private static IEnumerator ScreenshotRoutine(ScreenshotFinishedAction action, bool resize = true) {
        // Disable UI rendering for a frame;
        GameManager.SetUIVisible(false);
        // We wait for the camera to finish the current render.
        yield return null;
        // Then wait one more frame to make sure that the the render doesn't include UI.
        yield return new WaitForEndOfFrame();

        Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
        action.Invoke(resize ? Resize(texture, 640, 360) : texture);
        GameManager.SetUIVisible(true);
    }
    private static Texture2D Resize(Texture2D texture2D,int targetX,int targetY) {
        RenderTexture rt=new RenderTexture(targetX, targetY,24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D,rt);
        Texture2D result=new Texture2D(targetX,targetY);
        result.ReadPixels(new Rect(0,0,targetX,targetY),0,0);
        result.Apply();
        return result;
    }
}
