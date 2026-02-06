using UnityEngine;
using TMPro;

public class DogDialogTrigger : MonoBehaviour
{
    [Header("Dialog Settings")]
    [TextArea(3, 5)]
    public string dialogMessage = "I love you";
    
    [Header("References")]
    public GameObject dogDialogUI;
    public TextMeshProUGUI dialogText;
    
    [Header("Trigger Settings")]
    public bool triggerOnce = true;
    
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
            return;
            
        if (!other.CompareTag("Player"))
            return;
        
        hasTriggered = true;
        ShowDialog();
    }
    
    private void ShowDialog()
    {
        if (dogDialogUI == null)
        {
            Debug.LogError("DogDialogTrigger: dogDialogUI is not assigned!");
            return;
        }
        
        // Set the text via reflection or by accessing the serialized field
        var dogDialogComponent = dogDialogUI.GetComponent<Game.UI.DogDialogUI>();
        if (dogDialogComponent != null)
        {
            // Use reflection to set the private fullText field
            var fieldInfo = typeof(Game.UI.DogDialogUI).GetField("fullText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(dogDialogComponent, dialogMessage);
            }
        }
        
        // Also set the text directly if we have a reference
        if (dialogText != null)
        {
            dialogText.text = "";
        }
        
        // Activate the dialog UI (this will trigger OnEnable and start typing)
        dogDialogUI.SetActive(true);
    }
    
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
