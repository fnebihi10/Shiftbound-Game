using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static partial class ProductionBenchmarkDelivery
{
    // Edit the existing opening objects in place. This does not run or stack the
    // historical benchmark generator. Physical dimensions come from their boxes.
    public static void RefineOpeningEquipment()
    {
        EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
        string before = CollisionContract();
        Material graphite = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Materials/Equipment graphite.mat");
        Material metal = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Materials/Aged zinc flashing.mat");
        Material recess = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Materials/Recessed cool glass.mat");
        Material painted = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Materials/Equipment ochre.mat");
        foreach (string name in new[] { "Rooftop utility", "Alternate wall test" })
        {
            GameObject item = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .First(t => t.name == name).gameObject;
            Vector3 size = item.transform.localScale;
            var mesh = new MeshAuthor();
            // Casing, folded cap, recessed doors, hinges, sill and ventilation.
            // All details fit the original solid box; no collision is concealed.
            mesh.Bevel(Vector3.zero, size - new Vector3(.035f,.06f,.035f), .028f, 0);
            mesh.Bevel(new Vector3(0,size.y*.5f-.035f,0),new Vector3(size.x,.07f,size.z),.016f,1);
            mesh.Box(new Vector3(0,-size.y*.5f+.065f,0),new Vector3(size.x-.06f,.13f,size.z-.06f),1);
            float face = -size.z*.5f;
            mesh.Box(new Vector3(0,.025f,face+.020f),new Vector3(size.x-.13f,size.y-.31f,.018f),2);
            for (int i = 0; i < 2; i++)
            {
                float x = (i == 0 ? -1f : 1f) * (size.x-.17f)*.25f;
                mesh.Bevel(new Vector3(x,.025f,face+.009f),new Vector3((size.x-.19f)*.5f,size.y-.34f,.016f),.014f,0);
                mesh.Box(new Vector3(x + (i == 0 ? .15f : -.15f),.07f,face+.003f),new Vector3(.022f,.13f,.006f),1);
                for (int hinge = 0; hinge < 2; hinge++)
                    mesh.Box(new Vector3(x + (i == 0 ? -.19f : .19f),(hinge == 0 ? -.35f : .35f),face+.005f),new Vector3(.025f,.10f,.01f),1);
            }
            for (int slat = 0; slat < 7; slat++)
                mesh.Box(new Vector3(0,-size.y*.30f + slat*.036f,face+.001f),new Vector3(size.x-.27f,.014f,.002f),2);
            // Small identification plate without unreadable decorative text.
            mesh.Box(new Vector3(.18f,size.y*.30f,face+.001f),new Vector3(.16f,.065f,.002f),1);
            Mesh authored = mesh.Create(name + " constructed enclosure");
            Vector3[] vertices = authored.vertices;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vector3(vertices[i].x/size.x,vertices[i].y/size.y,vertices[i].z/size.z);
            authored.vertices = vertices; authored.RecalculateNormals(); authored.RecalculateTangents(); authored.RecalculateBounds();
            string path = Folder + "/Meshes/" + Safe(name) + "-enclosure.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null) AssetDatabase.CreateAsset(authored,path);
            else { EditorUtility.CopySerialized(authored,existing); UnityEngine.Object.DestroyImmediate(authored); authored=existing; }
            item.GetComponent<MeshFilter>().sharedMesh = authored;
            item.GetComponent<MeshRenderer>().sharedMaterials = new[] { name == "Rooftop utility" ? graphite : painted, metal, recess };
        }
        // These older decorative units intersect the solid test boxes. The new
        // casing is the visible collision surface, so retire the duplicate art.
        foreach (Renderer renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (renderer.name == "Graphite service unit" || renderer.name == "Ochre service unit") renderer.enabled = false;
        if (before != CollisionContract()) throw new InvalidOperationException("Opening equipment changed collision; refuse save.");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
        Debug.Log("SHIFTBOUND EQUIPMENT REFINEMENT PASSED: two existing collision surfaces constructed; collider contract unchanged.");
    }
}
