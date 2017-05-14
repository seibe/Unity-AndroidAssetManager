
#if UNITY_ANDROID
using UnityEngine;
using System;

#if UNITY_EDITOR
using System.IO;
#endif //UNITY_EDITOR

namespace Android
{
    /// <summary>
    /// android.content.res.AssetManager
    /// </summary>
    public static class AssetManager
    {
#if !UNITY_EDITOR
        private static readonly string kMethodName_Open = "open";
        private static readonly string kMethodName_Close = "close";
        private static readonly string kMethodName_Available = "available";
        private static readonly string kMethodName_Read = "read";
        private static AndroidJavaObject _manager = null;
#endif //!UNITY_EDITOR

        //----------------------------------------------------------------------

        public static void Initialize()
        {
#if !UNITY_EDITOR
            if (_manager == null)
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    _manager = activity.Call<AndroidJavaObject>("getAssets");
                }
            }
#endif
        }

        public static bool IsInitialized
        {
#if !UNITY_EDITOR
            get { return (_manager != null); }
#else
            get { return true; }
#endif
        }

        public static AssetInputStream Open(string fileName)
        {
#if !UNITY_EDITOR
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
            var path = string.Format("{0}/{1}", Application.streamingAssetsPath, fileName);
            if (File.Exists(path))
            {
                return new AssetInputStream(fileName);
            }
#endif

            return null;
        }

        //----------------------------------------------------------------------

        /// <summary>
        /// android.content.res.AssetManager.AssetInputStream
        /// </summary>
        public sealed class AssetInputStream : IDisposable
        {
            public readonly string FileName = null;
#if !UNITY_EDITOR
            private AndroidJavaObject _stream = null;
#endif //!UNITY_EDITOR
            private int _byteLength = -1;
            private bool _isDispose = false;

            //----------------------------------------------------------------------

#if !UNITY_EDITOR
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
#endif //!UNITY_EDITOR

            public byte[] ReadAllBytes()
            {
                byte[] ret = null;

#if !UNITY_EDITOR
                if (ByteLength > 0 && _stream != null)
                {
                    var byteArray = AndroidJNI.NewByteArray(_byteLength);
                    _stream.Call<int>(kMethodName_Read, byteArray);
                    ret = AndroidJNI.FromByteArray(byteArray);
                    AndroidJNI.DeleteGlobalRef(byteArray);
                }
#else
                var path = string.Format("{0}/{1}", Application.streamingAssetsPath, FileName);
                if (File.Exists(path))
                {
                    return File.ReadAllBytes(path);
                }
#endif

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
#if !UNITY_EDITOR
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
#if !UNITY_EDITOR
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
