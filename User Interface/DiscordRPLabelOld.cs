// using UnityEngine;

// public class DiscordRPLabel : MonoBehaviour
// {
//     TMPro.TextMeshProUGUI label;

//     void Start() {
//         label = GetComponent<TMPro.TextMeshProUGUI>();
//     }
//     void Update() {
//         // DiscordRPController controller = DiscordRPController.discordController;
//         // label.text = $"Discord {DiscordManager.GetCurrentUser().Username}#{DiscordManager.GetCurrentUser().Discriminator}";
//         if (DiscordManager.Connected && DiscordManager.RichPresenceEnabled) {
//             label.text = $"Discord Rich Presence\nActive";
//         }
//         else if (DiscordManager.RichPresenceEnabled) {
//             label.text = "Discord Rich Presence\nNot Active";
//         }
//         else {
//             label.text = "Discord Rich Presence\nDisabled";
//         }
//     }
// }
