using System;
using System.Collections;
using UnityEngine;

namespace Scripts.Handlers
{
    public class ImageLoader
    {
        public static IEnumerator LoadTextures(float width, float height, int count, Action<Texture2D[]> _afterLoading)
        {
            var loadedTextures = new Texture2D[count];
            for (var i = 0; i < count; i++)
            {
                WWW www = new WWW($"https://picsum.photos/{width}/{height}");
                yield return www;
                loadedTextures[i] = www.texture;
            }

            _afterLoading.Invoke(loadedTextures);
        }
    }
}
