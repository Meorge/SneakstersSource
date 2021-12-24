using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Realtime;

using Sneaksters.Gameplay;
using Sneaksters.UI.Animation;

namespace Sneaksters.UI.Menus
{
    public class SquadRoomPlayerLabel : MonoBehaviour
    {
        [SerializeField] Shader thiefIconShader = null;
    
        Material thiefIconMaterial = null;

        [SerializeField] Image thiefIconImage = null;
        [SerializeField] Sprite thiefIcon = null;

        public SquadRoomPlayerLabelAnimator animator { get; private set; } = null;

        public int Index
        {
            get => index;
            set
            {
                index = value;
            }
        }

        private int index = 0;

        void Awake()
        {
            animator = GetComponent<SquadRoomPlayerLabelAnimator>();
            SetUpThiefIconMaterial();
        }
        private void Start()
        {



        }

        void SetUpThiefIconMaterial() {
            thiefIconShader = Shader.Find("Unlit/UIMulticolorBlack");
            thiefIconMaterial = new Material(thiefIconShader);

            thiefIconMaterial.SetFloat("_Alpha", 1f);
            thiefIconMaterial.SetColor("_ReplaceBlackColor", Color.black);

            thiefIconMaterial.SetTexture("_MainTex", thiefIcon.texture);
            thiefIconMaterial.SetTexture("_OverlayTex", Texture2D.blackTexture);

            thiefIconImage.material = thiefIconMaterial;
            thiefIconImage.sprite = thiefIcon;

        }
        public void UpdateContents()
        {
            // Check if this index exists in the list
            List<Player> players = SquadRoomManager.Players;

            // If there's no player to fill this spot, set it to waiting
            if (Index >= players.Count)
            {
                animator.AnimateToWaiting();
                return;
            }

            // There is a player to fill this spot - let's get them!
            Player player = players[Index];
            animator.AnimateIn();
            animator.SetPlayerName(player.NickName);

            var hashtable = player.CustomProperties;

            // Determine new player tag
            if (player.IsMasterClient)
            {
                animator.SetPlayerTag("Leader");
            }

            // Not the master client, so let's see if they're ready...
            else if (hashtable.ContainsKey("ready") && (bool)hashtable["ready"])
            {
                animator.SetPlayerTag("Ready!");
            } else {
                animator.SetPlayerTag("");
            }


            // Determine player job
            if (!hashtable.ContainsKey("job"))
            {
                animator.SetJobIcon(SquadRoomManager.ThinkingSprite);
            }
            else
            {
                Sprite sprite;

                PlayerType pType = (PlayerType)hashtable["job"];
                switch (pType)
                {
                    case PlayerType.Thief:
                        sprite = SquadRoomManager.GemSprite;
                        break;
                    case PlayerType.ComputerGuy:
                        sprite = SquadRoomManager.ComputerSprite;
                        break;
                    case PlayerType.Undecided:
                        sprite = SquadRoomManager.ThinkingSprite;
                        break;
                    default:
                        sprite = SquadRoomManager.UnknownSprite;
                        break;
                }
                animator.SetJobIcon(sprite);
            }

            // Get player color!
            Color playerColor;
            
            if (hashtable.ContainsKey("playerColor"))
                playerColor = ColorExtensions.FromHex((string)hashtable["playerColor"]);
            else
                playerColor = Color.black;

            thiefIconMaterial.SetColor("_ReplaceBlackColor", playerColor);
            
        }

        #region Animation
        public void AnimateIn()
        {
            // Debug.Log("Animate waiting in");
            animator.AnimateWaitingIn();
        }

        public void AnimateOut()
        {
            animator.AnimateWaitingOut();
        }
        #endregion
    }
}