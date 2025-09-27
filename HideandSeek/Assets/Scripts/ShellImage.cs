using UnityEngine;
using UnityEngine.UI;

/// Draws a sprite as multiple stacked quads along -Z to fake thickness (UI-friendly).
[RequireComponent(typeof(CanvasRenderer))]
public class ShellImage : MaskableGraphic
{
    [SerializeField] private Sprite sprite;
    [Min(1)] public int shellCount = 24;
    public float thickness = 0.08f;
    public bool useSpriteMesh = false;
    public bool twoSided = true;

    [Header("Depth Contrast")]
    [Range(0f, 1f)] public float depthDarken = 0.25f;   // 0 = none, 1 = fully dark at back
    public Color sideTint = Color.white;                // e.g., slightly cooler/warmer side
    [Range(0f, 1f)] public float sideTintAmount = 0.25f;// 0 = no tint, 1 = full tint at back

    [Header("Material (UI/URP AlphaClip)")]
    public Material urpShellMaterial;

    public Sprite Sprite { get => sprite; set { if (sprite != value) { sprite = value; SetAllDirty(); } } }
    public override Texture mainTexture => sprite ? sprite.texture : s_WhiteTexture;

    protected override void OnEnable()
    {
        base.OnEnable();
        material = urpShellMaterial;
        raycastTarget = false;
        if (canvas) canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (!sprite) return;

        var rectSize = rectTransform.rect.size;
        var sb = sprite.bounds;
        var sprSize = sb.size;
        if (sprSize.x <= 0f) sprSize.x = 1f;
        if (sprSize.y <= 0f) sprSize.y = 1f;

        float sx = rectSize.x / sprSize.x;
        float sy = rectSize.y / sprSize.y;
        float scale = Mathf.Min(sx, sy);

        float step = (shellCount <= 1) ? 0f : (thickness / (shellCount - 1));

        Vector3[] basePos;
        Vector2[] baseUV;
        int[] baseTris;

        if (useSpriteMesh && sprite.triangles != null && sprite.triangles.Length >= 3)
        {
            var v2 = sprite.vertices;
            basePos = new Vector3[v2.Length];
            for (int i = 0; i < v2.Length; i++)
            {
                Vector2 local = v2[i] - (Vector2)sb.center;
                basePos[i] = new Vector3(local.x * scale, local.y * scale, 0f);
            }
            baseUV = sprite.uv;
            baseTris = System.Array.ConvertAll(sprite.triangles, t => (int)t);
        }
        else
        {
            Vector2 half = sprSize * 0.5f;
            Vector3 p0 = new Vector3(-half.x * scale, -half.y * scale, 0);
            Vector3 p1 = new Vector3(-half.x * scale, half.y * scale, 0);
            Vector3 p2 = new Vector3(half.x * scale, half.y * scale, 0);
            Vector3 p3 = new Vector3(half.x * scale, -half.y * scale, 0);
            basePos = new Vector3[] { p0, p1, p2, p3 };

            Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            var uvsSrc = sprite.uv;
            for (int i = 0; i < uvsSrc.Length; i++)
            {
                uvMin = Vector2.Min(uvMin, uvsSrc[i]);
                uvMax = Vector2.Max(uvMax, uvsSrc[i]);
            }
            baseUV = new Vector2[]
            {
                new Vector2(uvMin.x, uvMin.y),
                new Vector2(uvMin.x, uvMax.y),
                new Vector2(uvMax.x, uvMax.y),
                new Vector2(uvMax.x, uvMin.y)
            };
            baseTris = new int[] { 0, 1, 2, 0, 2, 3 };
        }

        Color baseCol = color;

        for (int layer = 0; layer < shellCount; layer++)
        {
            float z = -step * layer;

            float t = (shellCount <= 1) ? 0f : (float)layer / (shellCount - 1);

            float shade = Mathf.Lerp(1f, 1f - depthDarken, t);

            Color tint = Color.Lerp(Color.white, sideTint, sideTintAmount * t);

            Color layerCol = new Color(
                baseCol.r * tint.r * shade,
                baseCol.g * tint.g * shade,
                baseCol.b * tint.b * shade,
                baseCol.a
            );

            int startIndex = vh.currentVertCount;
            for (int i = 0; i < basePos.Length; i++)
            {
                var world = transform.TransformPoint(basePos[i]);
                UIVertex v = UIVertex.simpleVert;
                v.position = basePos[i] + new Vector3(0, 0, z);
                v.uv0 = (i < baseUV.Length) ? baseUV[i] : Vector2.zero;
                v.uv1 = new Vector2(world.x, world.y); 
                v.color = layerCol;
                vh.AddVert(v);
            }

            for (int idx = 0; idx < baseTris.Length; idx += 3)
            {
                int a = startIndex + baseTris[idx + 0];
                int b = startIndex + baseTris[idx + 1];
                int c = startIndex + baseTris[idx + 2];
                vh.AddTriangle(a, b, c);
                if (twoSided) vh.AddTriangle(c, b, a);
            }
        }
    }
}
