using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class ShaderDetail
{
    public Shader shader;
    public int nPassCount;
    //public int nTexture;
    public int nRefCount;

    public bool bBuiltIn;

    //public List<Texture> ContainTextures = new List<Texture>();
    public Dictionary<ObjectDetail, int> ReferenceInObject = new Dictionary<ObjectDetail, int>();

    public ObjectDetail AddRefInObject(Material mat, Renderer renderer)
    {
        ObjectDetail objDetail = null;

        foreach (ObjectDetail tObjDetail in ReferenceInObject.Keys)
        {
            if (tObjDetail.gameObject == renderer.gameObject)
            {
                objDetail = tObjDetail;
                ReferenceInObject[objDetail]++;
                break;
            }
        }

        if (objDetail == null)
        {
            objDetail = new ObjectDetail();
            objDetail.gameObject = renderer.gameObject;
            ReferenceInObject[objDetail] = 1;
        }


        RendererDetail rendererDetail = objDetail.AddRenderer(renderer);
        rendererDetail.AddMaterial(mat, this);

        return objDetail;
    }
}

public class MaterialDetail
{
    public Material material;
    public ShaderDetail shaderDetail;

    public List<Texture> ContainTextures = new List<Texture>();
}

public class RendererDetail
{
    public Renderer renderer;
    public List<MaterialDetail> materialDetailList = new List<MaterialDetail>();

    public void AddMaterial(Material material, ShaderDetail shaderDetail)
    {
        MaterialDetail matDetail = new MaterialDetail();
        matDetail.material = material;
        matDetail.shaderDetail = shaderDetail;
        materialDetailList.Add(matDetail);
    }
}

public class ObjectDetail
{
    public GameObject gameObject;
    public List<RendererDetail> rendererDetailList = new List<RendererDetail>();

    public RendererDetail AddRenderer(Renderer renderer)
    {
        RendererDetail rendererDetail = null;
        foreach(var tRenderDetail in rendererDetailList)
        {
            if (tRenderDetail.renderer == renderer)
            {
                rendererDetail = tRenderDetail;
                break;
            }
        }
        
        if (rendererDetail == null)
        {
            rendererDetail = new RendererDetail();
            rendererDetail.renderer = renderer;
            rendererDetailList.Add(rendererDetail);
        }

        return rendererDetail;
    }
}


//public class ShaderDetails
//{
//    public Dictionary<Material, int> FoundInMaterials = new Dictionary<Material, int>();
//    //public List<Object> FoundInRenderers = new List<Object>();
//    //public List<Object> FoundInGameobjects = new List<Object>();

//    // shader property
//    public Shader shader;
//    public bool isSystem;
//    public int nPassCount;
//    public int nRefCount;

//    public List<Texture> ContainTexture = new List<Texture>();
//}

class ObjToPixelDetail
{
    public GameObject obj;
    public Mesh mesh;
    public Bounds activeBounds;
}


public class ShaderAnalyser : EditorWindow 
{
    private enum InspectType
    {
        Shader,
        Pixel,
    };

    private List<ShaderDetail> mActiveShaders = new List<ShaderDetail>();
    private Vector2 shaderListScrollPos = new Vector2(0, 0);
    private Vector2 pixelListScrollPos = new Vector2(0, 0);
    private readonly float ThumbnailHeight = 40;
    private readonly float ThumbnailWidth = 40;

    private ShaderDetail RefObjToList = null;
    private ShaderDetail TextureToList = null;

    private Plane[] planes;

    private readonly string[] inspectToolbarStrings = { "Shader", "Pixel" };
    private InspectType ActiveInspectType = InspectType.Shader;
    //private ShaderDetail ShaderToList = null;

	// Use this for initialization

    // Mesh / pixels
    private List<ObjToPixelDetail> mObjsToPixel = new List<ObjToPixelDetail>();
   

	void Start () {
        
	}

    [MenuItem("KingsoftTools/ShaderAnalyzer")]
    public static void OnMenuItem() {
        var window = EditorWindow.GetWindow<ShaderAnalyser>();
        window.minSize = new Vector2(640, 400);
        window.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Refresh"))
        {
            Transform trs = Camera.main.transform;
            planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            FindAllShaders();
        }
        
        GUILayout.BeginHorizontal();
            GUILayout.Label("Shader Count: " + mActiveShaders.Count.ToString());
            //GUILayout.Label("PassCount");
            //GUILayout.Label("TextureCount");
            //GUILayout.Label("References");
        GUILayout.EndHorizontal();

        ActiveInspectType = (InspectType)GUILayout.Toolbar((int)ActiveInspectType, inspectToolbarStrings);

        switch (ActiveInspectType)
        {
            case InspectType.Shader:
                ListAllShaders();
                break;
            case InspectType.Pixel:
                ListPixels();
                break;
        }
    }

    void FindAllShaders()
    {
        mActiveShaders.Clear();
        mObjsToPixel.Clear();

        Renderer[] renderers = FindObjectsOfType<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            ObjectDetail objDetail = null;

            foreach(Material material in renderer.sharedMaterials)
            {
                if (material != null && material.shader != null)
                {
                    ShaderDetail tShaderDetail = FindShaderDetail(material.shader);

                    if (tShaderDetail == null)
                    {
                        tShaderDetail = new ShaderDetail();
                        tShaderDetail.shader = material.shader;
                        tShaderDetail.nPassCount = material.passCount;
                        tShaderDetail.nRefCount = 0;

                        mActiveShaders.Add(tShaderDetail);
                    }

                    tShaderDetail.nRefCount++;

                    objDetail = tShaderDetail.AddRefInObject(material, renderer);
                }
            }

            if (renderer is SkinnedMeshRenderer)
            {
                Transform transform = renderer.gameObject.transform;
                var tMeshRenderer = renderer as SkinnedMeshRenderer;

                if (tMeshRenderer.sharedMesh != null)
                {
                    Bounds activeBounds = renderer.bounds;
                    if (GeometryUtility.TestPlanesAABB(planes, activeBounds))
                    {
                        ObjToPixelDetail obj = new ObjToPixelDetail();
                        obj.obj = renderer.gameObject;
                        obj.activeBounds = activeBounds;
                        obj.mesh = tMeshRenderer.sharedMesh;
                        mObjsToPixel.Add(obj);
                    }
                }
            }

            if (renderer is MeshRenderer)
            {
                Transform transform = renderer.gameObject.transform;
                var tMeshFilter = renderer.gameObject.GetComponent<MeshFilter>();

                if (tMeshFilter != null && tMeshFilter.sharedMesh != null)
                {
                    Bounds activeBounds = renderer.bounds;

                    if (GeometryUtility.TestPlanesAABB(planes, activeBounds))
                    {
                        ObjToPixelDetail obj = new ObjToPixelDetail();
                        obj.obj = renderer.gameObject;
                        obj.activeBounds = activeBounds;
                        obj.mesh = tMeshFilter.sharedMesh;
                        mObjsToPixel.Add(obj);
                    }
                }
            }

            if (renderer is ParticleSystemRenderer)
            {
                //var tRenderer = renderer as ParticleSystemRenderer;
                //var tMesh = tRenderer.mesh;   
            }
        }
    }

    public float CalcScreenPercentage(Bounds bounds)
    {
        float minX = Mathf.Infinity;
        float minY = Mathf.Infinity;
        float maxX = Mathf.Infinity;
        float maxY = Mathf.Infinity;

        Vector3 v3Center = bounds.center;
        Vector3 v3Extents = bounds.extents;

        Vector3[] corners = new Vector3[8];

        corners[0] = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top left corner
        corners[1] = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top right corner
        corners[2] = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom left corner
        corners[3] = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom right corner
        corners[4] = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top left corner
        corners[5] = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top right corner
        corners[6] = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom left corner
        corners[7] = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom right corner

        for (var i = 0; i < corners.Length; i++)
        {
            var corner = corners[i];
            corner = Camera.main.WorldToScreenPoint(corner);
            if (i == 0)
            {
                minX = maxX = corner.x;
                minY = maxY = corner.y;
            }
            else
            {
                if (corner.x > maxX)
                    maxX = corner.x;
                if (corner.x < minX)
                    minX = corner.x;
                if (corner.y > maxY)
                    maxY = corner.y;
                if (corner.y < minY)
                    minY = corner.y;
            }
        }

        var width = Camera.main.pixelWidth;
        var height = Camera.main.pixelHeight;
        maxX = Mathf.Clamp(maxX, 0.0f, width);
        minX = Mathf.Clamp(minX, 0.0f, width);
        maxY = Mathf.Clamp(maxY, 0.0f, height);
        minY = Mathf.Clamp(minY, 0.0f, height);

        width = maxX - minX;
        height = maxY - minY;
        var area = width * height;
        float percentage = area / (Camera.main.pixelWidth * Camera.main.pixelHeight) * 100.0f;

        return percentage;
    }

    public void ListPixels()
    {
        pixelListScrollPos = EditorGUILayout.BeginScrollView(pixelListScrollPos);

        foreach (ObjToPixelDetail obj in mObjsToPixel)
        {
            GUILayout.BeginHorizontal();
                if (GUILayout.Button(obj.obj.name, GUILayout.Width(300)))
                {
                    SelectObject(obj.obj);
                }

                GUILayout.Label("VertexCount:: " + obj.mesh.vertexCount.ToString(), GUILayout.Width(200));
                float per = CalcScreenPercentage(obj.activeBounds);
                GUILayout.Label("Screen Percent: " + per.ToString() + "%", GUILayout.Width(200));
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    public void ListAllShaders()
    {
        shaderListScrollPos = EditorGUILayout.BeginScrollView(shaderListScrollPos);

        foreach(ShaderDetail tShaderDetail in mActiveShaders)
        {
            GUILayout.BeginHorizontal();
                GUILayout.Label(tShaderDetail.shader.name, GUILayout.Width(300));

                if (GUILayout.Button(tShaderDetail.nPassCount.ToString() + " Pass", GUILayout.Width(100)))
                {
                    SelectObject(tShaderDetail.shader);
                }

                if (GUILayout.Button("Textures", GUILayout.Width(100)))
                {
                    if (TextureToList == tShaderDetail)
                    {
                        TextureToList = null;
                    }
                    else
                    {
                        TextureToList = tShaderDetail;
                    }
                }

                if (GUILayout.Button(tShaderDetail.nRefCount.ToString() + " Ref", GUILayout.Width(100)))
                {
                    if (RefObjToList == tShaderDetail)
                    {
                        RefObjToList = null;
                    }
                    else
                    {
                        RefObjToList = tShaderDetail;
                    }
                }
            GUILayout.EndHorizontal();

            ListShaderReferences(tShaderDetail);

        }

        EditorGUILayout.EndScrollView();
    }

    public ShaderDetail FindShaderDetail(Shader tShader)
    {
        foreach (var tShaderDetail in mActiveShaders)
        {
            if (tShaderDetail.shader == tShader)
            {
                return tShaderDetail;
            }
        }
        return null;
    }

    void Update()
    {
        
    }

    public void ListShaderReferences(ShaderDetail tShaderDetail)
    {
        if (RefObjToList == tShaderDetail)
        {
            foreach (ObjectDetail tObjDetail in tShaderDetail.ReferenceInObject.Keys)
            {
                foreach (RendererDetail rendererDetail in tObjDetail.rendererDetailList)
                {
                    foreach (MaterialDetail matDetail in rendererDetail.materialDetailList)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(" -" +
                                        rendererDetail.renderer.ToString() + "/" +
                                        matDetail.material.ToString());

                        if (GUILayout.Button("Select", GUILayout.Width(100)))
                        {
                            SelectObject(matDetail.material);
                            SelectObject(tObjDetail.gameObject);
                        }
                        GUILayout.EndHorizontal();

                        if (tShaderDetail == TextureToList)
                            ListTextures(matDetail);
                    }
                }

            }
        }
    }

    public void ListTextures(MaterialDetail tMatDetail)
    {
        if (tMatDetail.ContainTextures.Count == 0)
        {
            var dependencies = EditorUtility.CollectDependencies(new Object[] { tMatDetail.material });
            foreach (var obj in dependencies)
            {
                if (obj is Texture)
                {
                    Texture tTexture = obj as Texture;
                    tMatDetail.ContainTextures.Add(tTexture);
                }
            }
        }

        foreach (var tTex in tMatDetail.ContainTextures)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Box(tTex, GUILayout.Width(ThumbnailWidth), GUILayout.Height(ThumbnailHeight));
            
            if (GUILayout.Button(tTex.name, GUILayout.Width(150)))
            {
                SelectObject(tTex);
            }
            if (tTex is Texture2D)
            {
                var mipMap = ((Texture2D)tTex).mipmapCount;

                GUILayout.Label("mipMap: " + mipMap, GUILayout.Width(150));

                //Debug.Log(((Texture2D)tTex).GetPixels().Length);
            }
            
            
            var tWidth = tTex.width;
            var tHeight = tTex.height;

            var sizeLabel = "" + tTex.width + "x" + tTex.height;
            //if (tDetails.isCubeMap) sizeLabel += "x6";
            TextureFormat format = TextureFormat.RGBA32;

            int menSize = CalculateTextureSizeBytes(tTex, ref format) / 1024;

            sizeLabel += "\n" + menSize.ToString() + "KB - " + format + "";

            GUILayout.Label(sizeLabel, GUILayout.Width(120));
            GUILayout.EndHorizontal();
        }
    }
    

    private int GetBitsPerPixel(TextureFormat format)
    {
        switch (format)
        {
            case TextureFormat.Alpha8: //	 Alpha-only texture format.
                return 8;
            case TextureFormat.ARGB4444: //	 A 16 bits/pixel texture format. Texture stores color with an alpha channel.
                return 16;
            case TextureFormat.RGBA4444: //	 A 16 bits/pixel texture format.
                return 16;
            case TextureFormat.RGB24: // A color texture format.
                return 24;
            case TextureFormat.RGBA32: //Color with an alpha channel texture format.
                return 32;
            case TextureFormat.ARGB32: //Color with an alpha channel texture format.
                return 32;
            case TextureFormat.RGB565: //	 A 16 bit color texture format.
                return 16;
            case TextureFormat.DXT1: // Compressed color texture format.
                return 4;
            case TextureFormat.DXT5: // Compressed color with alpha channel texture format.
                return 8;
            /*
            case TextureFormat.WiiI4:	// Wii texture format.
            case TextureFormat.WiiI8:	// Wii texture format. Intensity 8 bit.
            case TextureFormat.WiiIA4:	// Wii texture format. Intensity + Alpha 8 bit (4 + 4).
            case TextureFormat.WiiIA8:	// Wii texture format. Intensity + Alpha 16 bit (8 + 8).
            case TextureFormat.WiiRGB565:	// Wii texture format. RGB 16 bit (565).
            case TextureFormat.WiiRGB5A3:	// Wii texture format. RGBA 16 bit (4443).
            case TextureFormat.WiiRGBA8:	// Wii texture format. RGBA 32 bit (8888).
            case TextureFormat.WiiCMPR:	//	 Compressed Wii texture format. 4 bits/texel, ~RGB8A1 (Outline alpha is not currently supported).
                return 0;  //Not supported yet
            */
            case TextureFormat.PVRTC_RGB2: //	 PowerVR (iOS) 2 bits/pixel compressed color texture format.
                return 2;
            case TextureFormat.PVRTC_RGBA2: //	 PowerVR (iOS) 2 bits/pixel compressed with alpha channel texture format
                return 2;
            case TextureFormat.PVRTC_RGB4: //	 PowerVR (iOS) 4 bits/pixel compressed color texture format.
                return 4;
            case TextureFormat.PVRTC_RGBA4: //	 PowerVR (iOS) 4 bits/pixel compressed with alpha channel texture format
                return 4;
            case TextureFormat.ETC_RGB4: //	 ETC (GLES2.0) 4 bits/pixel compressed RGB texture format.
                return 4;
            case TextureFormat.ATC_RGB4: //	 ATC (ATITC) 4 bits/pixel compressed RGB texture format.
                return 4;
            case TextureFormat.ATC_RGBA8: //	 ATC (ATITC) 8 bits/pixel compressed RGB texture format.
                return 8;
            case TextureFormat.BGRA32: //	 Format returned by iPhone camera
                return 32;
            case TextureFormat.ATF_RGB_DXT1: //	 Flash-specific RGB DXT1 compressed color texture format.
            case TextureFormat.ATF_RGBA_JPG: //	 Flash-specific RGBA JPG-compressed color texture format.
            case TextureFormat.ATF_RGB_JPG: //	 Flash-specific RGB JPG-compressed color texture format.
                return 0; //Not supported yet
        }
        return 0;
    }

    private int CalculateTextureSizeBytes(Texture tTexture, ref TextureFormat format)
    {
        var tWidth = tTexture.width;
        var tHeight = tTexture.height;

        format = TextureFormat.RGBA32;

        if (tTexture is Texture2D)
        {
            var tTex2D = tTexture as Texture2D;
            var bitsPerPixel = GetBitsPerPixel(tTex2D.format);
            format = tTex2D.format;

            var mipMapCount = tTex2D.mipmapCount;
            var mipLevel = 1;
            var tSize = 0;
            while (mipLevel <= mipMapCount)
            {
                tSize += tWidth * tHeight * bitsPerPixel / 8;
                tWidth = tWidth / 2;
                tHeight = tHeight / 2;
                mipLevel++;
            }
            return tSize;
        }

        if (tTexture is Cubemap)
        {
            var tCubemap = tTexture as Cubemap;
            format = tCubemap.format;
            var bitsPerPixel = GetBitsPerPixel(tCubemap.format);
            return tWidth * tHeight * 6 * bitsPerPixel / 8;
        }
        return 0;
    }

    public void SelectObject(Object selectedObject)
    {
        Selection.activeObject = selectedObject;
    }
}
