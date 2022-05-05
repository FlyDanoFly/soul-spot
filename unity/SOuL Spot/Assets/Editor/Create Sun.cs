using UnityEditor;
using UnityEngine;

public class CreateSoulDomeSun : MonoBehaviour
{
    [MenuItem("GameObject/Soul Dome/Create Sun", false, 10)]

    static void CreateSun(MenuCommand menuCommand) {
        const string sunName = "Sun";
        float sunCenterDiameter = InchesToMeters(9.6f);

        const int numLeds = 600;
        const string ledBasename = "LED.";
        const string ledFormatString = "d4";
        const float ledSurfaceOffset = 0.001f;
        const float ledSize = 0.005f;

        //
        // Setup the undo function
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Undo Create Sun");
        int undoGroupIndex = Undo.GetCurrentGroup();

        //
        // Create sun center object with adjusted renderer and material
        GameObject sun = CreateCenter(sunName, sunCenterDiameter);
        SetupRendering(sun, Color.black);
        
        //
        // Get points evently distributed around a unit sphere
        Vector3[] points = FibonacciSphere(numLeds);

        //
        // Create LED objects with adjusted renderer and material
        for (int x=0; x < numLeds; x++) {
            string ledName = ledBasename + x.ToString(ledFormatString);
            GameObject go = CreateLed(ledName, points[x], ledSize, ledSurfaceOffset, sunCenterDiameter);
         
            Color randomColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            SetupRendering(go, randomColor, Color.green);

            go.transform.SetParent(sun.transform, true);
        }

        // Wrap up undo
        Undo.CollapseUndoOperations(undoGroupIndex);

        // Good practice to leave the newly created object activated
        Selection.activeObject = sun;
    }

    static void SetupRendering(GameObject go, Color color, Color? emissionColor=null) {
        // Turn off casting and receiving shadows, create a new material "MAT - {go.name}" with specificed colors
        Shader shader = Shader.Find("Standard");
        Material material = new Material(shader);
        Undo.RegisterCreatedObjectUndo(material, "");
        material.name = "MAT - " + go.name;
        material.SetColor("_Color", color);
        if (emissionColor.HasValue) {
            material.SetColor("_EmissionColor", (Color) emissionColor);

            // BUG: IDK why this won't actually enable Emission. It works at runtime though, so not super important. 
            material.DisableKeyword("_EMISSION");
        }

        Renderer renderer = go.GetComponent<Renderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.material = material;
    }

    static GameObject CreateCenter(string name, float sunCenterDiameter) {
        GameObject sunCenter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Undo.RegisterCreatedObjectUndo(sunCenter, "");
        sunCenter.name = name;
        sunCenter.transform.localScale = new Vector3(sunCenterDiameter, sunCenterDiameter, sunCenterDiameter);

        DestroyImmediate(sunCenter.GetComponent<Collider>());

        return sunCenter; 
    }

    static GameObject CreateLed(string name, Vector3 normal, float ledSize, float ledSurfaceOffset, float sunDiameter) {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = name;
        go.transform.LookAt(-normal);
        go.transform.localScale = new Vector3(ledSize, ledSize, ledSize);
        go.transform.position = (ledSurfaceOffset + sunDiameter / 2.0f) * normal;
        
        DestroyImmediate(go.GetComponent<Collider>());
        
        return go;
    }

    static float InchesToMeters(float inches) {
        return 0.0254f * inches;
    }

    static Vector3[] FibonacciSphere(int samples, bool yUp=true, bool outputPoints=true) {
        // From: https://stackoverflow.com/a/26127012
        Vector3[] points = new Vector3[samples];
        
        float phi = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));  // golden angle in radians

        for (int i = 0; i < samples; i++) {
            float y = 1 - (i / (float)(samples - 1)) * 2;  // y goes from 1 to -1
            float radius = Mathf.Sqrt(1 - y * y);  // radius at y

            float theta = phi * i; //  // golden angle increment

            float x = Mathf.Cos(theta) * radius;
            float z = Mathf.Sin(theta) * radius;

            if (yUp) {
                points[i] = new Vector3(x,z,y);
                // From: https://gamedev.stackexchange.com/a/7932
                // points[i] = new Vector3(x,z,-y);
                // points[i] = new Vector3(x,z,y);
            } else {
                points[i] = new Vector3(x,y,z);
            }
        }

        return points;
    }
}