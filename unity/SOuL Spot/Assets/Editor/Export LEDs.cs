using System.IO;
using UnityEditor;
using UnityEngine;

public class ExportLEDs : MonoBehaviour
{
    [MenuItem("Custom Tools/Export LEDs")]

    static void ExportLedsToFile() {
        // Looks like making JSON is hard in Unity and/or CSharp
        // I don't need a sophisticated solution, I'll just write my own
        // Schema:
        //   {
        //     "normals": [
        //       [float,float,float],
        //       ... number of LEDs ...
        //     ]
        //     "positions": [
        //       [float,float,float],
        //       ... number of LEDs ...
        //     ]
        //   }

        const string sunName = "Sun";
        const string directory = "Assets/Exports";
        const string filename = "LEDs.json";
        const string ledBasename = "LED.";
        const string ledFormatString = "d4";

        // Loop through the LEDs, create arrays of JSON representations of each vector 
        GameObject sun = GameObject.Find(sunName);
        string[] normals = new string[sun.transform.childCount];
        string[] positions = new string[sun.transform.childCount];
        for (int i = 0; i < sun.transform.childCount; i++) {
            string ledName = ledBasename + i.ToString(ledFormatString);
            Transform led = sun.transform.Find(ledName);
            Vector3 normal = led.localPosition.normalized;
            Debug.Log(ledName);
            Debug.Log(led.transform.position);
            Debug.Log(led.transform.localPosition);
            normals[i] = VectorToJsonString(normal);
            positions[i] = VectorToJsonString(led.localPosition);
        }

        // Create the JSON
        string json = "{";
        json += "\"normals\":[" + string.Join(",", normals) + "]";
        json += ",";
        json += "\"positions\":[" + string.Join(",", positions) + "]";
        json += "}";
        
        // Write the flie
        StreamWriter writer = new StreamWriter(directory + "/" + filename);
        writer.WriteLine(json);
        writer.Close();

        // Let the user know something happened
        Debug.Log("Wrote " + sun.transform.childCount + " LEDs");
    }

    static string VectorToJsonString(Vector3 vector) {
        return "[" + vector[0] + "," + vector[1] + "," + vector[2] + "]";
    }
}
