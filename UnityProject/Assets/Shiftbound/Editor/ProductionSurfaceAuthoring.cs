using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class ProductionUpgradeAuthoring
{
    // Offline authored maps and environment reflection. No runtime generators,
    // extra lights, camera effects or reflection capture on the phone.
    static void Surfaces()
    {
        const int size=256;
        Texture2D Write(string name,Func<float,float,Color> pixel,bool normal=false)
        {
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                pixels[y*size+x]=pixel(x/(float)size,y/(float)size);
            texture.SetPixels(pixels);texture.Apply();
            string path=Root+"/Surfaces/"+name+".png";
            File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
            importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Repeat;
            importer.maxTextureSize=size;importer.anisoLevel=2;
            var android=importer.GetPlatformTextureSettings("Android");
            android.overridden=true;android.maxTextureSize=size;android.format=TextureImporterFormat.ASTC_6x6;
            importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        float Mineral(float u,float v)
        {
            float a=u*Mathf.PI*2,b=v*Mathf.PI*2;
            return Mathf.Sin(a*3+b*2)*.38f+Mathf.Sin(a*7-b*5)*.24f+
                Mathf.Sin(a*19+b*13)*.12f+Mathf.Sin(a*43-b*37)*.06f;
        }
        var mineral=Write("MineralRender-v2",(u,v)=>
        {
            float value=.95f+Mineral(u,v)*.045f;
            return new Color(value,value,value,1);
        });
        var normalMap=Write("MineralRenderNormal-v2",(u,v)=>
        {
            const float step=1f/size;
            var n=new Vector3((Mineral(u-step,v)-Mineral(u+step,v))*.3f,
                (Mineral(u,v-step)-Mineral(u,v+step))*.3f,1).normalized;
            return new Color(n.x*.5f+.5f,n.y*.5f+.5f,n.z*.5f+.5f,1);
        },true);
        foreach(var material in facadeRenders)
        {
            material.SetTexture("_BaseMap",mineral);material.SetTexture("_BumpMap",normalMap);
            material.SetFloat("_BumpScale",.4f);material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
        }
        var enamel=Write("ServiceEnamel-v2",(u,v)=>
        {
            // Sparse paint wear; broad colour fields remain clean at game scale.
            float scratch=Mathf.Abs(Mathf.Sin((u*5+v*2)*Mathf.PI*2));
            float patch=Mathf.SmoothStep(.91f,.99f,Mathf.Sin(u*Mathf.PI*6)*Mathf.Sin(v*Mathf.PI*4));
            float value=.98f-Mineral(u,v)*.018f-patch*(scratch<.07f?.28f:.025f);
            return new Color(value,value,value,1);
        });
        foreach(string name in new[]{"Blue service enamel","Warm service enamel"})
        {
            var material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/"+name+".mat");
            if(material==null)continue;
            material.SetTexture("_BaseMap",enamel);EditorUtility.SetDirty(material);
        }

        // A baked soft environment supplies believable sky/ground reflections
        // to glazing and coated metal without mobile realtime probe rendering.
        var cube=new Cubemap(64,TextureFormat.RGBAHalf,true);
        for(int face=0;face<6;face++)
        {
            var pixels=new Color[64*64];
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
            {
                float u=(x+.5f)/32-1,v=(y+.5f)/32-1;
                Vector3 d=face==0?new Vector3(1,-v,-u):face==1?new Vector3(-1,-v,u):
                    face==2?new Vector3(u,1,v):face==3?new Vector3(u,-1,-v):
                    face==4?new Vector3(u,-v,1):new Vector3(-u,-v,-1);
                d.Normalize();
                Color sky=Color.Lerp(new Color(.70f,.76f,.78f),new Color(.22f,.43f,.69f),Mathf.Clamp01(d.y));
                Color ground=Color.Lerp(new Color(.23f,.25f,.23f),new Color(.55f,.52f,.43f),Mathf.Clamp01(d.y+1));
                Color c=Color.Lerp(ground,sky,Mathf.SmoothStep(-.12f,.15f,d.y));
                float glow=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,new Vector3(-.6f,.45f,.65f).normalized)),20);
                c+=new Color(.65f,.38f,.12f,0)*glow;c.a=1;pixels[y*64+x]=c;
            }
            cube.SetPixels(pixels,(CubemapFace)face);
        }
        cube.Apply(true);string cubePath=Root+"/Atmosphere/CitySkyReflection-v2.asset";
        var old=AssetDatabase.LoadAssetAtPath<Cubemap>(cubePath);
        if(old!=null){EditorUtility.CopySerialized(cube,old);UnityEngine.Object.DestroyImmediate(cube);cube=old;}
        else AssetDatabase.CreateAsset(cube,cubePath);
        RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;
        RenderSettings.customReflectionTexture=cube;RenderSettings.reflectionIntensity=.65f;
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("SHIFTBOUND AUTHORED SURFACES: mineral detail/normal, enamel wear, baked64px sky reflection; no runtime capture");
    }
}
