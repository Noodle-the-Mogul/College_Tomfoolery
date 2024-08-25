using UnityEngine;

public class ProximityButton : MonoBehaviour
{
    public GameObject button; // Reference to the button GameObject

    
    void Start(){
        button.SetActive(false);
    }
     private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player enters the trigger zone
        if (other.CompareTag("Player"))
        {
            // Enable the button when the player is near
            button.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the player exits the trigger zone
        if (other.CompareTag("Player"))
        {
            // Disable the button when the player moves away
            button.SetActive(false);
        }
    }
}