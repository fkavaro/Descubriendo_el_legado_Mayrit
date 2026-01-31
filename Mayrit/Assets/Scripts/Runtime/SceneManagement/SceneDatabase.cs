using System.Collections.Generic;
using UnityEngine;

public class SceneDatabase
{
    public class Slot
    {
        public const string Session = "Session";
        public const string Milestone = "Milestone";
    }

    public class Name
    {
        public const string CoreScene = "CoreScene";
        public const string MainMenuScene = "MainMenuScene";
        public const string GamePlayScene = "GamePlayScene";
        public const string Milestone = "Milestone";
        public const string Milestone1 = "Milestone1";
        public const string Milestone2 = "Milestone2";
        public const string Milestone3 = "Milestone3";
        public const string Milestone4 = "Milestone4";
        public const string Milestone5 = "Milestone5";
        public const string Milestone6 = "Milestone6";
        public const string Milestone7 = "Milestone7";
        public const string Milestone8 = "Milestone8";
    }

    public static readonly List<string> MilestoneScenes = new()
    {
        Name.Milestone1,
        Name.Milestone2,
        Name.Milestone3,
        Name.Milestone4,
        Name.Milestone5,
        Name.Milestone6,
        Name.Milestone7,
        Name.Milestone8
    };
}
