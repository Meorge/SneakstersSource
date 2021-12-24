using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Sneaksters.Gameplay;

namespace Sneaksters.UI
{
    public class UILevelSelectButton : MonoBehaviour
    {
        public TextMeshProUGUI levelNumberLabel;
        public TextMeshProUGUI levelNameLabel;

        private Button button;

        private LevelObject levelObject;

        public void Awake() {
            button = GetComponent<Button>();
        }

        public void SetupWithLevelObject(LevelObject obj) {
            levelObject = obj;
            levelNumberLabel.text = obj.levelNumber.ToString("D2");
            levelNameLabel.text = obj.levelName;
            button.onClick.AddListener(delegate {
                PhotonGameManager.Instance.SelectLevel(obj);
            });
        }
    }
}