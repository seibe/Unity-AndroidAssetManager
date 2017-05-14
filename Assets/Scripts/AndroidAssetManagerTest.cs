
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

/// <summary>
/// Test code for <see cref="Android.AssetManager"/>
/// </summary>
public class AndroidAssetManagerTest
{
	[UnityTest]
	public System.Collections.IEnumerator ReadText()
    {
		yield return null;

#if UNITY_ANDROID
        using (var stream = Android.AssetManager.Open("Hello.txt"))
        {
            var byteLength = stream.ByteLength;
            Assert.NotZero(byteLength);

            var bytes = stream.ReadAllBytes();
            Assert.IsNotNull(bytes);
            Assert.AreEqual(byteLength, bytes.Length);

            Debug.LogFormat("Hello.txt = {0} bytes", byteLength);
        }
#endif
	}
}
