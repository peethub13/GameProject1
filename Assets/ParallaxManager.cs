using UnityEngine;

public class ParallaxManager : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Renderer renderer;
        public float speed = 0.1f;
    }

    public Layer[] layers;
    public float globalSpeed = 1f;

    void Update()
    {
        foreach (Layer layer in layers)
        {
            if (layer.renderer == null) continue;
            float offset = Time.time * layer.speed * globalSpeed;
            layer.renderer.material.mainTextureOffset = new Vector2(offset, 0);
        }
    }
}