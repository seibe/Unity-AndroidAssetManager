
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_NATIVE
#endif //UNITY_ANDROID && !UNITY_EDITOR

#if UNITY_ANDROID
using UnityEngine;
using System;

#if !UNITY_ANDROID_NATIVE
using System.IO;
#endif //!UNITY_ANDROID_NATIVE

namespace Android
{
    /// <summary>
    /// android.content.res.AssetManager
    /// </summary>
    public static class AssetManager
    {
#if UNITY_ANDROID_NATIVE
        private static readonly string kMethodName_Open = "open";
        private static readonly string kMethodName_Close = "close";
        private static readonly string kMethodName_Available = "available";
        private static AndroidJavaObject _manager = null;

        [ThreadStatic]
        private static bool _isAttachCurrentThread = false;

        [RuntimeInitializeOnLoadMethod]
        private static void initializeInMainThread() { _isAttachCurrentThread = true; }
#else
        private static string _streamingAssetsPath = null;

        [RuntimeInitializeOnLoadMethod]
        private static void initializeInMainThread() { _streamingAssetsPath = Application.streamingAssetsPath; }
#endif //UNITY_ANDROID_NATIVE

        //----------------------------------------------------------------------

        public static void Initialize()
        {
#if UNITY_ANDROID_NATIVE
            if (!_isAttachCurrentThread)
            {
                if (AndroidJNI.AttachCurrentThread() == 0)
                {
                    _isAttachCurrentThread = true;
                }
            }
            if (_manager == null)
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    _manager = activity.Call<AndroidJavaObject>("getAssets");
                }
            }
#endif //UNITY_ANDROID_NATIVE
        }

        public static bool IsInitialized
        {
#if UNITY_ANDROID_NATIVE
            get { return (_isAttachCurrentThread && _manager != null); }
#else
            get { return true; }
#endif //UNITY_ANDROID_NATIVE
        }

        public static AssetInputStream Open(string fileName)
        {
#if UNITY_ANDROID_NATIVE
            if (!IsInitialized) Initialize();

            if (_manager != null)
            {
                var raw = _manager.Call<AndroidJavaObject>(kMethodName_Open, fileName);
                if (raw != null)
                {
                    return new AssetInputStream(fileName, raw);
                }
            }
#else
            var path = string.Format("{0}/{1}", _streamingAssetsPath, fileName);
            if (File.Exists(path))
            {
                return new AssetInputStream(fileName);
            }
#endif //UNITY_ANDROID_NATIVE

            return null;
        }

        //----------------------------------------------------------------------

        /// <summary>
        /// android.content.res.AssetManager.AssetInputStream
        /// </summary>
        public sealed class AssetInputStream : IDisposable
        {
#if UNITY_ANDROID_NATIVE
            private static readonly IntPtr kMethodPtr_Read;

            static AssetInputStream()
            {
                var classPtr = AndroidJNI.FindClass("java/io/InputStream");
                kMethodPtr_Read = AndroidJNIHelper.GetMethodID(classPtr, "read", "([B)I");
            }
#endif //UNITY_ANDROID_NATIVE

            public readonly string FileName = null;
#if UNITY_ANDROID_NATIVE
            private readonly jvalue[] _args = new jvalue[1];
            private AndroidJavaObject _stream = null;
#endif //UNITY_ANDROID_NATIVE
            private int _byteLength = -1;
            private bool _isDispose = false;

            //----------------------------------------------------------------------

#if UNITY_ANDROID_NATIVE
            public AssetInputStream(string fileName, AndroidJavaObject stream)
            {
                FileName = fileName;
                _stream = stream;
            }
#else
            public AssetInputStream(string fileName)
            {
                FileName = fileName;
            }
#endif //UNITY_ANDROID_NATIVE

            public byte[] ReadAllBytes()
            {
                byte[] ret = null;

#if UNITY_ANDROID_NATIVE
                if (ByteLength > 0 && _stream != null)
                {
                    var byteArray = AndroidJNI.NewByteArray(_byteLength);
                    if (byteArray != IntPtr.Zero)
                    {
                        _args[0] = new jvalue() { l = byteArray };
                        AndroidJNI.CallIntMethod(_stream.GetRawObject(), kMethodPtr_Read, _args);
                        ret = AndroidJNI.FromByteArray(byteArray);
                        AndroidJNI.DeleteLocalRef(byteArray);
                    }
                }
#else
                var path = string.Format("{0}/{1}", _streamingAssetsPath, FileName);
                if (File.Exists(path))
                {
                    return File.ReadAllBytes(path);
                }
#endif //UNITY_ANDROID_NATIVE

                return ret;
            }

            public void Dispose()
            {
                if (!_isDispose)
                {
                    _isDispose = true;

                    disposeInternal(false);
                    GC.SuppressFinalize(this);
                }
            }

            public int ByteLength
            {
                get
                {
#if UNITY_ANDROID_NATIVE
                    if (_byteLength < 0 && _stream != null)
                    {
                        _byteLength = _stream.Call<int>(kMethodName_Available);
                    }
#else
                    if (_byteLength < 0)
                    {
                        _byteLength = ReadAllBytes().Length;
                    }
#endif
                    return _byteLength;
                }
            }

            //----------------------------------------------------------------------

            ~AssetInputStream()
            {
                if (!_isDispose)
                {
                    Debug.LogWarning("Do not rely on finalization! Use Dispose!");
                    disposeInternal(true);
                }
            }

            private void disposeInternal(bool isFinallize)
            {
#if UNITY_ANDROID_NATIVE
                if (_stream != null)
                {
                    _stream.Call(kMethodName_Close);
                    _stream.Dispose();
                    _stream = null;
                }
#endif
            }
        }
    }
}
#endif //UNITY_ANDROID
