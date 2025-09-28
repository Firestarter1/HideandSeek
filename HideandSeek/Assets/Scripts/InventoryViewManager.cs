using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryViewManager : MonoBehaviour
{
    public static InventoryViewManager Instance;

    public List<RawImage> slotImages;

    public ModelPreviewRenderer modelPreviewRenderer;

    List<RenderTexture> rts = new List<RenderTexture>();
    List<RenderTexture> availableRts = new List<RenderTexture>();

    public Vector3 additionalRotation = Vector3.zero;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        CreateRenderTextures();
    }

    private void OnDestroy()
    {
        CleanupRenderTextures();
    }

    void CreateRenderTextures()
    {
        CleanupRenderTextures();
        for (int i = 0; i < slotImages.Count; i++)
        {
            RenderTexture rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
            rt.name = "InvSlot_RT_" + i.ToString();
            rt.format = RenderTextureFormat.ARGB32;
            rt.useMipMap = false;
            rt.autoGenerateMips = false;
            rt.Create();
            rts.Add(rt);
            slotImages[i].texture = rt;
        }
    }

    public void AddSlot(RawImage newImg, int slotIndex)
    {
        GrowList(slotImages, slotIndex);
        GrowList(rts, slotIndex);

        if (rts[slotIndex])
        {
            rts[slotIndex].Release();
        }

        RenderTexture rt = new RenderTexture(256, 256, 16);
        rt.name = "InvSlot_RT_" + slotIndex.ToString();
        rt.format = RenderTextureFormat.ARGB32;
        rt.useMipMap = false;
        rt.autoGenerateMips = false;
        rt.Create();
        rts[slotIndex] = rt;

        slotImages[slotIndex] = newImg;
        if (newImg)
        {
            slotImages[slotIndex].texture = rt;
            slotImages[slotIndex].enabled = true;
        }
        
    }

    void GrowList<T>(List<T> list, int targetIndexInclusive)
    {
        while (list.Count <= targetIndexInclusive)
        {
            list.Add(default);
        }
    }

    void CleanupRenderTextures()
    {
        foreach (RenderTexture renderTexture in rts)
        {
            if (renderTexture)
            {
                renderTexture.Release();
            }
        }

        rts.Clear();
    }

    RenderTexture GetOrMakeRT(int slotIndex)
    {
        GrowList(rts, slotIndex);
        if (!rts[slotIndex])
        {
            RenderTexture rt = new RenderTexture(256, 256, 16);
            rt.name = "InvSlot_RT_" + slotIndex.ToString();
            rt.format = RenderTextureFormat.ARGB32;
            rt.useMipMap = false;
            rt.autoGenerateMips = false;
            rt.Create();
            rts[slotIndex] = rt;
        }
        return rts[slotIndex];
    }

    public void SetSlotItemDisplay(int slotIndex, GameObject model)
    {
        GrowList(slotImages, slotIndex);

        RawImage img = slotImages[slotIndex];
        RenderTexture rt = GetOrMakeRT(slotIndex);

        if (img)
        {
            img.texture = rt;
            img.enabled = model != null;
        }

        if (!model) return;

        GameObject objInst = Instantiate(model, modelPreviewRenderer.modelTransform);
        objInst.transform.localPosition = Vector3.zero;
        objInst.transform.localRotation = Quaternion.Euler(model.transform.rotation.eulerAngles + additionalRotation);
        objInst.transform.localScale = Vector3.one;

        modelPreviewRenderer.Render(objInst, rts[slotIndex]);
        Destroy(objInst);
    }

    public void SetSlotItemDisplay(RawImage img, GameObject model)
    {
        int slotIndex = slotImages.IndexOf(img);
        if (slotIndex < 0)
        {
            slotIndex = slotImages.Count;
            AddSlot(img, slotIndex);
        }

        SetSlotItemDisplay(slotIndex, model);
        
    }
}
