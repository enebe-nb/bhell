using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class BorderlessGrab : MonoBehaviour, IDragHandler {
    private Vector2 lastDelta = Vector2.zero;
    public void OnDrag(PointerEventData data) {
        if (data.dragging && lastDelta != -data.delta) BorderlessSetup.MoveWindowPos(data.delta);
        lastDelta = data.delta;
    }
}

public class BorderlessSetup {
    private const string keyWidth = "ResolutionWidth";
    private const string keyHeight = "ResolutionHeight";
    private const string keyFullscreen = "FullScreen";

    protected static float ratio = 1.5f;
    protected static int width;
    protected static int height;
    protected static bool fullscreen = false;

    [DllImport("user32.dll")] private static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out WinRect lpRect);
    private struct WinRect { public int left, top, right, bottom; }
    private const int GWL_STYLE = -16;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_POPUP = 0x80000000;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void Initialize() {
        fullscreen = PlayerPrefs.GetInt(keyFullscreen, Screen.fullScreen ? 1 : 0) != 0;
        Resolution defResolution = Screen.currentResolution;
        defResolution.width = (int)((float)defResolution.height / ratio);
        width = PlayerPrefs.GetInt(keyWidth, defResolution.width);
        height = PlayerPrefs.GetInt(keyHeight, defResolution.height);
        ApplyResolution();
    }

    public static void SetAndSaveResolution(int newHeight, bool newFullscreen) {
        PlayerPrefs.SetInt(keyHeight, height = newHeight);
        PlayerPrefs.SetInt(keyWidth, width = (int)((float)height / ratio));
        PlayerPrefs.SetInt(keyFullscreen, (fullscreen = newFullscreen) ? 1 : 0);
        ApplyResolution();
    }

    private static void ApplyResolution() {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        if (!fullscreen) {
            SetWindowLong(GetActiveWindow(), GWL_STYLE, WS_POPUP | WS_VISIBLE);
        }
#endif
        Screen.SetResolution(width, height, fullscreen);
    }

    public static void MoveWindowPos(Vector2 delta) {
        if (fullscreen) return;

        IntPtr handle = GetActiveWindow();
        if (!GetWindowRect(handle, out WinRect winRect)) return;
        Resolution res = Screen.currentResolution;

        int x = winRect.left + (int)delta.x;
        if (x < 0) x = 0; if (x + width > res.width) x = res.width - width;
        int y = winRect.top - (int)delta.y;
        if (y < 0) y = 0; if (y + height > res.height) y = res.height - height;
        MoveWindow(handle, x, y, width, height, false);
    }
}
