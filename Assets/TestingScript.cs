using UnityEngine;

public class TestingScript : MonoBehaviour {
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        // Get the SpriteRenderer component attached to this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Check if the SpriteRenderer component exists
        
    }

    // Update is called once per frame
    void Update()
    {
        // You can add any update logic here if needed
    }

    public void TestingMethod(string message)
    {
        Debug.Log(message);
        if (spriteRenderer != null)
        {
            // Set the color to green
            spriteRenderer.color = Color.green;
        } else
        {
            Debug.LogError("SpriteRenderer component not found on this GameObject.");
        }
    }
}
