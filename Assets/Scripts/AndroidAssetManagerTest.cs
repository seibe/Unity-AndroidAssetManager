
#if DEBUG
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Threading;

/// <summary>
/// Test code for <see cref="Android.AssetManager"/>
/// </summary>
public class AndroidAssetManagerTest
{
	[UnityTest]
	public System.Collections.IEnumerator ReadText()
    {
		yield return new MonoBehaviourTest<Behaviour>();
	}

    private class Behaviour : MonoBehaviour, IMonoBehaviourTest
    {
        private bool _isFinish = false;

        public bool IsTestFinished { get { return _isFinish; } }

        private void Awake()
        {
            Debug.Log("Awake");
        }

        private void Start()
        {
            Debug.Log("Start");

            tryReadText();
            ThreadPool.QueueUserWorkItem(runOnBackgroundThread);
        }

        private void runOnBackgroundThread(object state)
        {
            tryReadText();
            _isFinish = true;
        }

        [System.Diagnostics.Conditional("UNITY_ANDROID")]
        private void tryReadText()
        {
#if UNITY_ANDROID
            using (var stream = Android.Content.Res.AssetManager.Open("Hello.txt"))
            {
                Assert.NotNull(stream);

                var byteLength = stream.ByteLength;
                Assert.NotZero(byteLength);

                var bytes = stream.ReadAllBytes();
                Assert.IsNotNull(bytes);
                Assert.AreEqual(byteLength, bytes.Length);

                Debug.LogFormat("Hello.txt = {0} bytes", byteLength);
            }
#endif //UNITY_ANDROID
        }
    }
}
#endif //DEBUG
