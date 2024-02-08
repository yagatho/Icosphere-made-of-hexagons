using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Icosahedron : MonoBehaviour
{
    #region Variables


    [Header("Scene Refrences")]
    [SerializeField] private GameObject[] hexagonPrefabs;
    [SerializeField] private GameObject pentagonPrefab;


    //Hidden
    private int resolution = 2; //Number of subdivisions for the sphere
    private GameObject sphereMesh;
    private IcosahedronGenerator icosahedron;
    private List<Hexagon> sphereHexes;
    private Mesh mesh;
    private float hexScale1 = 17f; //Change if hexagon scale is not right
    private float hexScale2 = 8.5f; //Change if hexagon scale is not right
    private float hexScale3 = 4.3f; //Change if hexagon scale is not right
    private float hexScale4 = 2.4f; //Change if hexagon scale is not right
    private float hexScale5 = 1f; //Change if hexagon scale is not right
    [HideInInspector] public bool check = false;

    #endregion

    #region Unity Functions
    private void Awake()
    {
       // GenerateMesh();
    }

    #endregion

    #region Functions

    //If you would like to use this script to generate modular icosahedron
    //mesh you will just need to add mesh renderer and filter to sphere
    //mesh object and modify code a bit
    public IEnumerator GenerateMeshSub(int subdivisions, float scale)
    {
        check = false;

        icosahedron = new IcosahedronGenerator();

        icosahedron.Initialize();
        StartCoroutine(icosahedron.Subdivide(subdivisions));

        yield return new WaitUntil(() => icosahedron.check);

        icosahedron.MapVertFace();
        icosahedron.SortPolygons();
        icosahedron.CreateTiles();

        PrepareMesh();
        StartCoroutine(CreateHexagons(subdivisions, scale));

        icosahedron.CleanUp();
        CleanUp2();

        check = true;
    }

    private IEnumerator CreateHexagons(int res, float scale = 1)
    {
        GameObject go2 = new GameObject("Sphere");
        go2.transform.SetParent(transform, true);

        //Create hexagon for each entry in list
        foreach (var i in sphereHexes)
        {
            //check if is hexagon
            if (i.verts[5] != Vector3.zero)
            {
                //Instantiate
                GameObject go = Instantiate(hexagonPrefabs[Random.Range(0,hexagonPrefabs.Length)]);
                go.name = "Hex";

                switch (res)
                {
                    case 1:
                        go.transform.localScale = new Vector3(hexScale1, hexScale1, hexScale1);
                        break;
                    case 2:
                        go.transform.localScale = new Vector3(hexScale2, hexScale2, hexScale2);
                        break;
                    case 3:
                        go.transform.localScale = new Vector3(hexScale3, hexScale3, hexScale3);
                        break;
                    case 4:
                        go.transform.localScale = new Vector3(hexScale4, hexScale4, hexScale4);
                        break;
                    case 5:
                        go.transform.localScale = new Vector3(hexScale5, hexScale5, hexScale5);
                        break;
                }

                //Calculate middle of the hexagon
                go.transform.position = Vector3.Lerp(i.verts[0], i.verts[4], .5f);

                //Calculate rotation of the hexagon
                Vector3 vecup = i.verts[4] - i.verts[0];
                Vector3 vec = Vector3.zero - go.transform.position;
                go.transform.rotation = Quaternion.LookRotation(vec, vecup);

           

                //Set parent to keep hierarchy clean
                go.transform.SetParent(go2.transform, true);

                yield return null;
            }
            else if(pentagonPrefab != null) 
            {
                //Instantiate
                GameObject go = Instantiate(pentagonPrefab);
                go.name = "Hex";

                switch (res)
                {
                    case 1:
                        go.transform.localScale = new Vector3(hexScale1, hexScale1, hexScale1);
                        break;
                    case 2:
                        go.transform.localScale = new Vector3(hexScale2, hexScale2, hexScale2);
                        break;
                    case 3:
                        go.transform.localScale = new Vector3(hexScale3, hexScale3, hexScale3);
                        break;
                    case 4:
                        go.transform.localScale = new Vector3(hexScale4, hexScale4, hexScale4);
                        break;
                    case 5:
                        go.transform.localScale = new Vector3(hexScale5, hexScale5, hexScale5);
                        break;
                }

                //Calculate middle of the hexagon
                go.transform.position = Vector3.Lerp(i.verts[0], i.verts[4], .5f);

                //Calculate rotation of the hexagon
                Vector3 vecup = i.verts[4] - i.verts[0];
                Vector3 vec = Vector3.zero - go.transform.position;
                go.transform.rotation = Quaternion.LookRotation(vec, vecup);



                //Set parent to keep hierarchy clean
                go.transform.SetParent(go2.transform, true);

                yield return null;
            }
        }

        go2.transform.localScale = new Vector3(scale, scale, scale);
    }

    private void PrepareMesh()
    {
        sphereHexes = icosahedron.hexes;

        this.sphereMesh = new GameObject("Sphere Mesh");
        this.sphereMesh.transform.parent = this.transform;

        mesh = new Mesh();

        int vertexCount = icosahedron.Polygons.Count * 3;
        int[] indices = new int[vertexCount];

        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];

        for (int i = 0; i < icosahedron.Polygons.Count; i++)
        {
            var poly = icosahedron.Polygons[i];

            indices[i * 3 + 0] = i * 3 + 0;
            indices[i * 3 + 1] = i * 3 + 1;
            indices[i * 3 + 2] = i * 3 + 2;

            vertices[i * 3 + 0] = icosahedron.Vertices[poly.vertices[0]];
            vertices[i * 3 + 1] = icosahedron.Vertices[poly.vertices[1]];
            vertices[i * 3 + 2] = icosahedron.Vertices[poly.vertices[2]];

            normals[i * 3 + 0] = icosahedron.Vertices[poly.vertices[0]];
            normals[i * 3 + 1] = icosahedron.Vertices[poly.vertices[1]];
            normals[i * 3 + 2] = icosahedron.Vertices[poly.vertices[2]];
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.SetTriangles(indices, 0);
    }

    private void CleanUp2()
    {
        Destroy(sphereMesh);
        mesh = null;
        sphereMesh = null;
        icosahedron= null;
    }

    #endregion 
}


///GENERATOR
#region Icosahendron Generator

public class IcosahedronGenerator
{
    //Verices and polygons
    private List<Polygon> polygons;
    private List<Vector3> vertices;

    //List of vertices after creating hexagon tiles
    public List<Vector3> newVerts;

    //List of created hexagons
    public List<Hexagon> hexes;

    //List of VMaps for mesh
    public List<VMap> vMaps;

    //Get set refrences
    public List<Polygon> Polygons { get => polygons; private set => polygons = value; }
    public List<Vector3> Vertices { get => vertices; set => vertices = value; }

    public bool check = false;

    //Init
    public void Initialize()
    {
        check = false;

        polygons = new List<Polygon>();
        vertices = new List<Vector3>();

        // Icosahedron has 12 vertices
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        vertices.Add(new Vector3(-1, t, 0).normalized);
        vertices.Add(new Vector3(1, t, 0).normalized);
        vertices.Add(new Vector3(-1, -t, 0).normalized);
        vertices.Add(new Vector3(1, -t, 0).normalized);
        vertices.Add(new Vector3(0, -1, t).normalized);
        vertices.Add(new Vector3(0, 1, t).normalized);
        vertices.Add(new Vector3(0, -1, -t).normalized);
        vertices.Add(new Vector3(0, 1, -t).normalized);
        vertices.Add(new Vector3(t, 0, -1).normalized);
        vertices.Add(new Vector3(t, 0, 1).normalized);
        vertices.Add(new Vector3(-t, 0, -1).normalized);
        vertices.Add(new Vector3(-t, 0, 1).normalized);

        // Create 20 faces
        polygons.Add(new Polygon(0, 11, 5));
        polygons.Add(new Polygon(0, 5, 1));
        polygons.Add(new Polygon(0, 1, 7));
        polygons.Add(new Polygon(0, 7, 10));
        polygons.Add(new Polygon(0, 10, 11));

        polygons.Add(new Polygon(1, 5, 9));
        polygons.Add(new Polygon(5, 11, 4));
        polygons.Add(new Polygon(11, 10, 2));
        polygons.Add(new Polygon(10, 7, 6));
        polygons.Add(new Polygon(7, 1, 8));

        polygons.Add(new Polygon(3, 9, 4));
        polygons.Add(new Polygon(3, 4, 2));
        polygons.Add(new Polygon(3, 2, 6));
        polygons.Add(new Polygon(3, 6, 8));
        polygons.Add(new Polygon(3, 8, 9));

        polygons.Add(new Polygon(4, 9, 5));
        polygons.Add(new Polygon(2, 4, 11));
        polygons.Add(new Polygon(6, 2, 10));
        polygons.Add(new Polygon(8, 6, 7));
        polygons.Add(new Polygon(9, 8, 1));


    }

    //Used to subdivide mesh
    public IEnumerator Subdivide(int recursions)
    {
        check = false;

        var midPointCache = new Dictionary<int, int>();

        for (int i = 0; i < recursions; i++)
        {
            var newPolys = new List<Polygon>();
            foreach (var poly in polygons)
            {
                int a = poly.vertices[0];
                int b = poly.vertices[1];
                int c = poly.vertices[2];

                // Use GetMidPointIndex to either create a new vertex between two old vertices
                int ab = GetMidPointIndex(midPointCache, a, b);
                int bc = GetMidPointIndex(midPointCache, b, c);
                int ca = GetMidPointIndex(midPointCache, c, a);

                // Create the four new polygons using our original
                newPolys.Add(new Polygon(a, ab, ca));
                newPolys.Add(new Polygon(b, bc, ab));
                newPolys.Add(new Polygon(c, ca, bc));
                newPolys.Add(new Polygon(ab, bc, ca));
            }

            // Replace all our old polygons with the new set of subdivided ones.
            polygons = newPolys;

            yield return null;
        }

        check = true;
    }

    //Map Vert Faces
    public void MapVertFace()
    {
        Polygon f;
        int pos;

        vMaps = new List<VMap>(vertices.Count);

        foreach (var face in polygons)
        {
            for (var i = 0; i < 3; i++)
            {
                //Clone face since we want to modify it
                f = face.Clone();

                //Get where in the array the point exists in the face
                pos = f.vertices.IndexOf(face.vertices[i]);

                //If vert is not first move it to first place
                if (pos != 0)
                    f.MoveToStart(pos);

                bool con = false;

                //Check if there is entry in list for this vertex
                foreach (var item in vMaps)
                {
                    if (item.vert == face.vertices[i])
                    {
                        //If so add it
                        item.faces.Add(f);

                        con = true;
                        break;
                    }
                }

                //If not create one
                if (!con)
                {
                    VMap map = new VMap(face.vertices[i], new List<Polygon>(5));
                    map.faces.Add(f);

                    vMaps.Add(map);
                }

            }
        }
    }

    public void SortPolygons()
    {
        VMap list; //Unsorted list
        VMap oList; //Sorted list

        int a;  // Vertex index looking for
        int b;  // Vertex index looking for
        int i;  // Loop Index

        Polygon face; // Reference polygon

        // Store old VMaps in placeholder and clear it
        List<VMap> _vmaps = new List<VMap>(vMaps.Count);

        foreach (var item in vMaps)
        {
            _vmaps.Add(item);
        }

        vMaps.Clear();



        for (int m = 0; m < _vmaps.Count; m++)
        {
            //Copy list
            list = _vmaps[m];

            oList = new VMap(list.vert, new List<Polygon>(5));
            oList.faces.Add(list.faces[0]);


            //Sort
            a = list.faces[0].vertices[0];
            b = list.faces[0].vertices[1];
            i = 0;

            while (list.faces.Count > 0 && i < list.faces.Count)
            {
                face = list.faces[i];

                if (face.vertices.IndexOf(a) != -1 && face.vertices.IndexOf(b) != -1)
                {
                    list.faces.RemoveAt(i);
                    oList.faces.Add(face);
                    a = face.vertices[0];
                    b = face.vertices[1];
                    i = 0;
                    continue;
                }

                i++;
            }

            vMaps.Add(oList);
        }
    }

    //Create Hex tiles
    public void CreateTiles()
    {

        //Centroid of polygon
        float[] fcPnt = new float[]
        {
            0, 0, 0
        };

        //Our map
        Map cache = new Map(vMaps.Count * 6);

        //List for newly created vertices
        newVerts = new List<Vector3>(vertices.Count);

        int vCnt = 0;
        int ip;

        //Key for mapping
        string key = "";

        //List used to store vertices in hexagons
        hexes = new List<Hexagon>(vMaps.Count);

        foreach (var vmap in vMaps)
        {
            for (int i = 0; i < vmap.faces.Count - 1; i++)
            {
                //Get centroid
                fcPnt = vmap.faces[i].Centroid(Vertices);

                //Create key 
                key = fcPnt[0] + "_" + fcPnt[1] + "_" + fcPnt[2];

                //If map has key get ip and add to hex
                if (cache.Has(key))
                {
                    ip = cache.Get(key);

                    hexes[vMaps.IndexOf(vmap)].verts[i] = new Vector3(fcPnt[0], fcPnt[1], fcPnt[2]);
                }
                //Else add new key increment ip and create new hex
                else
                {
                    ip = vCnt++;
                    cache.Set(key, ip);

                    Vector3 vec = new Vector3(fcPnt[0], fcPnt[1], fcPnt[2]);

                    newVerts.Add(vec);

                    Hexagon hex = new Hexagon();

                    hexes.Add(hex);
                    hexes[vMaps.IndexOf(vmap)].verts[i] = new Vector3(fcPnt[0], fcPnt[1], fcPnt[2]);
                }
            }
        }
    }

    //Destroy mesh and everything unnecessary 
    public void CleanUp()
    {
        polygons = null;
        vertices = null;
        newVerts = null;
        hexes = null;
        vMaps = null;
    }

    public int GetMidPointIndex(Dictionary<int, int> cache, int indexA, int indexB)
    {
        int smallerIndex = Mathf.Min(indexA, indexB);
        int greaterIndex = Mathf.Max(indexA, indexB);
        int key = (smallerIndex << 16) + greaterIndex;

        // If a midpoint is already defined, just return it.
        int ret;
        if (cache.TryGetValue(key, out ret))
            return ret;

        // If we're here, it's because a midpoint for these two vertices hasn't been created yet.
        Vector3 p1 = vertices[indexA];
        Vector3 p2 = vertices[indexB];
        Vector3 middle = Vector3.Lerp(p1, p2, 0.5f).normalized;

        ret = vertices.Count;
        vertices.Add(middle);

        cache.Add(key, ret);
        return ret;
    }
}

#endregion


///UTILITY STRUCTS
#region Utility Structs

//Used to create Vertex Maps
public class VMap
{

    public readonly int vert;
    public List<Polygon> faces;

    public VMap(int v, List<Polygon> f)
    {
        vert = v;
        faces = f;
    }
}

//Used to map tiles
public class Map
{
    public List<string> keys = new List<string>();

    public Map(int l)
    {
        keys = new List<string>(l);
    }

    public void Set(string s, int i)
    {
        keys.Insert(i, s);
    }

    public int Get(string s)
    {
        return keys.IndexOf(s);
    }

    public bool Has(string s)
    {
        if (keys.Contains(s)) return true;
        else
            return false;
    }
}

//Used to handle hexagons after making tiles
public class Hexagon
{
    public Vector3[] verts = new Vector3[6];
}



//Polygon class used to handle polygons
public class Polygon
{
    //Vertices of the polygon
    public readonly List<int> vertices;

    public Polygon(int a, int b, int c)
    {
        vertices = new List<int>() { a, b, c };
    }


    //Clone
    public Polygon Clone()
    {
        var f = new Polygon(vertices[0], vertices[1], vertices[2]);
        return f;
    }

    //Move indexed vertex to start of the array
    public void MoveToStart(int pos)
    {
        var tmp = this.vertices[pos];
        if (pos == 1) { this.vertices[1] = this.vertices[2]; this.vertices[2] = this.vertices[0]; this.vertices[0] = tmp; }
        else if (pos == 2) { this.vertices[2] = this.vertices[1]; this.vertices[1] = this.vertices[0]; this.vertices[0] = tmp; }
    }

    //Get centroid point of polygon
    public float[] Centroid(List<Vector3> verts)
    {
        float[] a = null;

        if (a != null)
            a[0] = a[1] = a[2] = 0;	
        else
            a = new float[]
            {
                0, 0, 0
            };

        for (var i = 0; i < vertices.Count; i++)
        {
            var v = verts[vertices[i]];
            a[0] += v[0];
            a[1] += v[1];
            a[2] += v[2];
        }

        a[0] = a[0] / 3;
        a[1] = a[1] / 3;
        a[2] = a[2] / 3;

        return a;
    }
}

#endregion