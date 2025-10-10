using UnityEngine;

namespace MonkeyPlayground.Objects.Items
{
    public class Banana : Item
    {
        public Banana()
        {
            itemName = "Banana";
            itemDescription = "A delicious banana. The goal of this game is to control the monkey to get a banana.";
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            var monkey = other.GetComponent<Monkey>();
            if (!monkey)
                return;
            monkey.HasReachedBanana = true;
        }
    }
}