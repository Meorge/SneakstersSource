using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Discord;
using DiscordObj = Discord.Discord;

namespace Sneaksters.Discord
{
    public class DiscordManager : MonoBehaviour
    {
        private static DiscordManager instance;

        public static bool Connected {
            get {
                return instance.connected;
            }
        }

        public static bool RichPresenceEnabled {
            get {
                return PlayerPrefs.GetInt("DiscordRichPresence", 1) == 1;
            }
        }

        long clientID = 0;

        bool connected = false;

        DiscordObj discord;


        UserManager userManager {
            get => discord?.GetUserManager();
        }

        ActivityManager activityManager {
            get => discord?.GetActivityManager();
        }

        void Awake() {
            if (instance == null) {
                instance = this;
            } else {
                Debug.LogError("More than one DiscordManager?!");
                Destroy(gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (RichPresenceEnabled) SetUpDiscord();
        }

        public void LogHook(LogLevel level, string message) {
            //print($"DiscordManager - Log[{level}] {message}");
        }

        public static void SetActivityInSquadMission(string missionName = "On a Mission", string squadID = "ABCD", int currentNumberOfPlayers = 1, int maxNumberOfPlayers = 12) {
                var activity = new Activity {
                State = "In a Squad",
                Details = missionName,
                Timestamps = new ActivityTimestamps {
                    Start = PhotonGameManager.GetCurrentUnixTimestamp()
                },
                Party = new ActivityParty {
                    Id = squadID.GetHashCode().ToString(),
                    Size = new PartySize {
                        CurrentSize = Mathf.Max(1, currentNumberOfPlayers),
                        MaxSize = maxNumberOfPlayers
                    },
                },
                Instance = true
            };

            UpdateActivity(activity);
        }
        public static void SetActivityInSquadIdle(string squadID = "ABCD", int currentNumberOfPlayers = 1, int maxNumberOfPlayers = 12) {
            var activity = new Activity {
                State = "In a Squad",
                Details = "Waiting for Mission",
                Party = new ActivityParty {
                    Id = squadID.GetHashCode().ToString(),
                    Size = new PartySize {
                        CurrentSize = Mathf.Max(1, currentNumberOfPlayers),
                        MaxSize = maxNumberOfPlayers
                    },
                },
                //Secrets = new Discord.ActivitySecrets {
                //    Join = squadID
                //},
                Instance = true
            };

            UpdateActivity(activity);

        }

        public static void SetActivityInMenus() {
            var activity = new Activity {
                Details = "In Menus",
                Instance = false,
            };
            UpdateActivity(activity);
        }

        public static void UpdateActivity(Activity activity) {
            //print($"DiscordManager - in UpdateActivity() with activity {activity.Details}");
            instance.discord?.GetActivityManager().UpdateActivity(activity, result => {
                //print($"DiscordManager - result {result}");
                });
        }

        public static void ClearActivity() {
            instance.discord?.GetActivityManager().ClearActivity(result => print($"DiscordManager - cleared activity with result {result}"));
        }

        public static void SendInvite() {
            var channelID = 619975668700020756;
            //print($"DiscordManager - attempt to send invite to {channelID}");
            instance.discord?.GetActivityManager().SendInvite(channelID, ActivityActionType.Join, "Time to sneak!", (result) => {
                //print($"DiscordManager - result of sending invite is {result}");
            });
        }

        public static User GetCurrentUser() {
            return instance.userManager.GetCurrentUser();
        }

        void Update() {
            try {
                discord?.RunCallbacks();
            } catch (ResultException e) {
                print($"DiscordManager - Received exception {e}, trying to reconnect...");
                connected = false;
                SetUpDiscord();
            }
        }

        public static void EnableDiscord() {
            instance.SetUpDiscord();
        }

        void SetUpDiscord() {
            discord = new DiscordObj(clientID, (System.UInt64)CreateFlags.NoRequireDiscord);
            discord.SetLogHook(LogLevel.Info, LogHook);

            discord.GetUserManager().OnCurrentUserUpdate += () => {
                print($"DiscordManager - current user updated to {userManager.GetCurrentUser().Username}#{userManager.GetCurrentUser().Discriminator}");
                connected = true;
                SetActivityInMenus();
            };

            //discord.GetActivityManager().OnActivityJoin += (roomID) => {
            //    print($"DiscordManager - attempt to join room with ID {roomID}");
            //};
        }

        public static void DisableDiscord() {
            instance.discord?.Dispose();
            print("DiscordManager - Discord has been disabled");
        }

        void OnDestroy() {
            if (instance != this) return;
            DisableDiscord();
        }
    }
}