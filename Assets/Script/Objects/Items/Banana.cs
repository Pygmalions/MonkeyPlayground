using UnityEngine;

namespace MonkeyPlayground.Objects.Items
{
    public class Banana : Item
    {
        public Banana()
        {
            itemName = "Banana";
            itemDescription = "A delicious banana. The goal of this game is to move the monkey to get a banana.";
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;
            var server = other.gameObject.GetComponent<MonkeyHttpController>();
            if (server != null)
            {
                server.IsTaskCompleted = true;
            }
            Debug.Log($"Task Completed: Monkey named '{server.monkey.monkeyName}' has reached the banana.");
        }
    }
}