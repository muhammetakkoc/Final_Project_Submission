using UnityEngine;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
    // Position of launch is its centre
    public GameObject launch;
    public GameObject spherePrefab;
    public GameObject planePrefab;
    public GameObject blockPrefab;
    public GameObject pigPrefab;

    private PhysicsSystem physicsSystem = new PhysicsSystem();
    private List<GameObject> blocks = new List<GameObject>();
    private List<GameObject> pigs = new List<GameObject>();
    private Dictionary<GameObject, float> pigToughness = new Dictionary<GameObject, float>();

    void Start()
    {
        // Register collision callback
        //physicsSystem.collisionCallback = CollisionTest;

        // Ground plane
        PhysicsBody ground = Instantiate(planePrefab).GetComponent<PhysicsBody>();
        ground.transform.position = new Vector3(0, -5, 0);
        ground.size = new Vector3(20, 1, 0);

        //PlaceBlocksAndPigs();
        //CreateBlocks();
        //CreatePigs();

        // Initialize blocks and pigs
        //InitializeCastle();
    }

    void Update()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0.0f;

        // Vector FROM mouse TO launcher (AB = B - A):
        Vector3 slingshot = launch.transform.position - mouse;
        Debug.DrawLine(mouse, launch.transform.position, Color.cyan);

        // Spawn sphere and launch based on slingshot
        if (Input.GetMouseButtonDown(0))
        {
            PhysicsBody sphere = Instantiate(spherePrefab).GetComponent<PhysicsBody>();
            sphere.transform.position = launch.transform.position;
            sphere.vel = slingshot * 5.0f;
        }

        // Object removal test
        if (Input.GetMouseButtonDown(1))
        {
            physicsSystem.Clear();
        }
    }

    void FixedUpdate()
    {
        physicsSystem.PreStep();
        physicsSystem.Step(Time.fixedDeltaTime);
        physicsSystem.PostStep();
    }
    //void PlaceBlocksAndPigs()
    //{
    //    float startX = -4.0f; // Blokların sol kenar konumu
    //    float startY = -2.0f; // Blokların alt kenar konumu
    //    float blockWidth = 1.5f; // Her bloğun genişliği
    //    float blockHeight = 1.5f; // Her bloğun yüksekliği

    //    // Blokları bir duvar gibi yerleştir (2 sıra, 4 sütun)
    //    for (int row = 0; row < 2; row++)
    //    {
    //        for (int col = 0; col < 4; col++)
    //        {
    //            // Blok konumunu hesapla
    //            float xPos = startX + col * blockWidth;
    //            float yPos = startY + row * blockHeight;

    //            // Bloku oluştur ve yerleştir
    //            GameObject block = Instantiate(blockPrefab);
    //            block.transform.position = new Vector3(xPos, yPos, 0);
    //        }
    //    }

    //    // Domuzları yerleştir (3 tane)
    //    float pigStartX = -2.0f; // Domuzların sol kenar konumu
    //    float pigStartY = 0.5f;  // Domuzların alt kenar konumu
    //    float pigSpacing = 2.0f; // Domuzlar arasındaki mesafe

    //    for (int i = 0; i < 3; i++)
    //    {
    //        // Domuz konumunu hesapla
    //        float xPos = pigStartX + i * pigSpacing;
    //        float yPos = pigStartY;

    //        // Domuzu oluştur ve yerleştir
    //        GameObject pig = Instantiate(pigPrefab);
    //        pig.transform.position = new Vector3(xPos, yPos, 0);
    //    }
    //}
    //private void InitializeCastle()
    //{
    //    // Create blocks
    //    for (int i = 0; i < 8; i++)
    //    {
    //        GameObject block = Instantiate(blockPrefab);
    //        block.transform.position = new Vector3(-2 + i * 1.5f, 0, 0);
    //        blocks.Add(block);
    //    }

    //    // Create pigs
    //    for (int i = 0; i < 3; i++)
    //    {
    //        GameObject pig = Instantiate(pigPrefab);
    //        pig.transform.position = new Vector3(-1 + i * 2, 1, 0);
    //        pigs.Add(pig);
    //        pigToughness[pig] = 5.0f; // Assign toughness value
    //    }
    //}

    //void CreateBlocks()
    //{
    //    // Blokların pozisyonlarını tanımla
    //    Vector3[] blockPositions = new Vector3[]
    //    {
    //        new Vector3(5.65f,-.45f, 0),
    //        new Vector3(5.65f, 0.87f, 0),
    //        new Vector3(11, -0.45f, 0),
    //        new Vector3(10.95f, 0.85f, 0),
    //        new Vector3(5.65f, 2.15f, 0),
    //        new Vector3(10.95f, 2.15f, 0),
    //        new Vector3(2, 1, 0),
    //        new Vector3(0, 2, 0)
    //    };

    //    // Blokları belirtilen pozisyonlarda oluştur
    //    foreach (Vector3 pos in blockPositions)
    //    {
    //        GameObject block = Instantiate(blockPrefab);
    //        block.transform.position = pos;
    //    }
    //}

    //void CreatePigs()
    //{
    //    // Domuzların pozisyonlarını tanımla
    //    Vector3[] pigPositions = new Vector3[]
    //    {
    //        new Vector3(67, -41, 0),
    //        new Vector3(116, -41, 0),
    //        new Vector3(2, 3, 0)
    //    };

    //    // Domuzları belirtilen pozisyonlarda oluştur
    //    foreach (Vector3 pos in pigPositions)
    //    {
    //        GameObject pig = Instantiate(pigPrefab);
    //        pig.transform.position = pos;
    //    }
    //}

    void CollisionTest(GameObject a, GameObject b)
    {
        PhysicsBody ba = a.GetComponent<PhysicsBody>();
        PhysicsBody bb = b.GetComponent<PhysicsBody>();
        Debug.Log("Object " + a.name + " is colliding with " + b.name);
        Debug.Log("A's velocity: " + ba.vel);
        Debug.Log("B's velocity: " + bb.vel);

        // Destroy pigs if momentum exceeds toughness
        if (pigs.Contains(a) || pigs.Contains(b))
        {
            GameObject pig = pigs.Contains(a) ? a : b;
            PhysicsBody pigBody = pig.GetComponent<PhysicsBody>();

            // Check momentum to decide if the pig is destroyed
            float momentum = pigBody.invMass * pigBody.vel.magnitude;
            if (momentum > pigToughness[pig])
            {
                Destroy(pig);
                pigs.Remove(pig);
            }
        }

        // Destroy blocks on collision
        if (blocks.Contains(a) || blocks.Contains(b))
        {
            GameObject block = blocks.Contains(a) ? a : b;
            Destroy(block);
            blocks.Remove(block);
        }
    }
}



//using UnityEngine;

//public class Game : MonoBehaviour
//{
//    // Position of launch is its centre
//    public GameObject launch;

//    public GameObject spherePrefab;
//    public GameObject planePrefab;
//    PhysicsSystem physicsSystem = new PhysicsSystem();

//    void Start()
//    {
//        // Register collision callback
//        physicsSystem.collisionCallback = CollisionTest;

//        // Ground plane
//        PhysicsBody ground = Instantiate(planePrefab).GetComponent<PhysicsBody>();
//        //ground.transform.up = Vector3.Normalize(Vector3.right * 2.0f + Vector3.up);

//        // Test sphere
//        //PhysicsBody sphere1 = Instantiate(spherePrefab).GetComponent<PhysicsBody>();
//        //sphere1.transform.position = Vector3.up * 0.5f;
//        //sphere1.vel = Vector3.right * 5.0f;
//        //sphere1.frictionCoefficient = 0.5f;
//        //sphere1.restitutionCoefficient = 0.0f;
//    }

//    // Click to spawn test sphere
//    void Update()
//    {
//        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//        mouse.z = 0.0f;

//        // Vector FROM mouse TO launcher (AB = B - A):
//        Vector3 slingshot = launch.transform.position - mouse;
//        Debug.DrawLine(mouse, launch.transform.position, Color.cyan);

//        // Spawn sphere and launch based on slingshot
//        if (Input.GetMouseButtonDown(0))
//        {
//            PhysicsBody sphere = Instantiate(spherePrefab).GetComponent<PhysicsBody>();
//            sphere.transform.position = launch.transform.position;
//            sphere.vel = slingshot * 5.0f;
//        }

//        // Object removal test
//        if (Input.GetMouseButtonDown(1))
//        {
//            physicsSystem.Clear();
//        }
//    }

//    void FixedUpdate()
//    {
//        physicsSystem.PreStep();
//        physicsSystem.Step(Time.fixedDeltaTime);
//        physicsSystem.PostStep();
//    }

//    // Example of how to respond to collision (destroy pigs here)!
//    void CollisionTest(GameObject a, GameObject b)
//    {
//        PhysicsBody ba = a.GetComponent<PhysicsBody>();
//        PhysicsBody bb = b.GetComponent<PhysicsBody>();
//        Debug.Log("Object " + a.name + " is colliding with " + b.name);
//        Debug.Log("A's velocity: " + ba.vel);
//        Debug.Log("B's velocity: " + bb.vel);

//        // Example code for destroying only if Pig
//        // Furthermore, could add additional components to separate gameplay from physics
//        // (Birds & Pigs can both be spheres, so checking by geometry alone is not enough to distinguish birds from pigs)!
//        if (ba.shapeType == ShapeType.SPHERE)
//        {
//            Destroy(a);
//        }
//        if (bb.shapeType == ShapeType.SPHERE)
//        {
//            Destroy(b);
//        }
//    }
//}
