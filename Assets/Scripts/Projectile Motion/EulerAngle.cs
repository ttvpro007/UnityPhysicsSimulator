using UnityEngine;

// eulerAngles
// Generate a cube that has different color on each face.  This shows
// the orientation of the cube as determined by the eulerAngles.
// Update the orientation every 2 seconds.

public class EulerAngle : MonoBehaviour
{
    private Quaternion quaternion;
    private GameObject cube;
    private float timeCount = 0.0f;

    void Awake()
    {
        quaternion = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        cube = CreateCube();
    }

    void Update()
    {
        if (timeCount > 2.0f)
        {
            // Every two seconds generate a random rotation for
            // the cube. The rotation is a quaternion.
            quaternion = Random.rotation;
            cube.transform.rotation = quaternion;

            timeCount = 0.0f;
        }

        timeCount = timeCount + Time.deltaTime;
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;

        // use eulerAngles to show the euler angles of the quaternion
        Vector3 angles = quaternion.eulerAngles;
        GUI.Label(new Rect(10, 10, 0, 0), angles.ToString("F3"), style);

        // note that localEulerAngles will give the same result
        // GUI.Label(new Rect(10,90,250,50), cube.transform.localEulerAngles.ToString("F3"));
    }

    private GameObject CreateCube()
    {
        // make a cube from 6 quad game objects.
        // color each side of the cube

        GameObject newCube = new GameObject("Cube");

        GameObject minusZ = GameObject.CreatePrimitive(PrimitiveType.Quad);
        minusZ.transform.position = new Vector3(0.0f, 0.0f, -0.5f);
        minusZ.GetComponent<Renderer>().material.color = Color.gray;
        minusZ.transform.parent = newCube.transform;

        GameObject plusZ = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plusZ.transform.position = new Vector3(0.0f, 0.0f, 0.5f);
        plusZ.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f));
        plusZ.GetComponent<Renderer>().material.color = Color.magenta;
        plusZ.transform.parent = newCube.transform;

        GameObject minusX = GameObject.CreatePrimitive(PrimitiveType.Quad);
        minusX.transform.position = new Vector3(0.5f, 0.0f, 0.0f);
        minusX.transform.Rotate(new Vector3(0.0f, 270.0f, 0.0f));
        minusX.GetComponent<Renderer>().material.color = Color.yellow;
        minusX.transform.parent = newCube.transform;

        GameObject plusX = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plusX.transform.position = new Vector3(-0.5f, 0.0f, 0.0f);
        plusX.transform.Rotate(new Vector3(0.0f, 90.0f, 0.0f));
        plusX.GetComponent<Renderer>().material.color = Color.blue;
        plusX.transform.parent = newCube.transform;

        GameObject minusY = GameObject.CreatePrimitive(PrimitiveType.Quad);
        minusY.transform.position = new Vector3(0.0f, -0.5f, 0.0f);
        minusY.transform.Rotate(new Vector3(270.0f, 0.0f, 0.0f));
        minusY.GetComponent<Renderer>().material.color = Color.green;
        minusY.transform.parent = newCube.transform;

        GameObject plusY = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plusY.transform.position = new Vector3(0.0f, 0.5f, 0.0f);
        plusY.transform.Rotate(new Vector3(90.0f, 0.0f, 0.0f));
        plusY.GetComponent<Renderer>().material.color = Color.red;
        plusY.transform.parent = newCube.transform;

        return newCube;
    }
}