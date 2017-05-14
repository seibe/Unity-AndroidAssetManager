
#if DEBUG
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;

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

#if UNITY_ANDROID
            using (var stream = Android.AssetManager.Open("Hello.txt"))
            {
                Assert.NotNull(stream);

                var byteLength = stream.ByteLength;
                Assert.NotZero(byteLength);

                var bytes = stream.ReadAllBytes();
                Assert.IsNotNull(bytes);
                Assert.AreEqual(byteLength, bytes.Length);

                Debug.LogFormat("Hello.txt = {0} bytes", byteLength);
            }
#endif
            _isFinish = true;
        }
    }
}
#endif //DEBUG
