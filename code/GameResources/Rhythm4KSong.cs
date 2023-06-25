using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Rhythm4K;

[GameResource("Rhythm4K Song", "rhythm", "Describes a Rhythm4K Song", Icon = "piano")]
public partial class Rhythm4KSong : GameResource
{
    public string ChartPath { get; set; }
    public string SoundName { get; set; }
    [ResourceType("png")]
    public string CoverArt { get; set; }
    public string[] LoadingTips { get; set;}
    
    [HideInEditor] public Song Song { get; set; }


    public static List<Rhythm4KSong> All => ResourceLibrary.GetAll<Rhythm4KSong>().ToList();
    // {
    //     get
    //     {
    //         var list = ResourceLibrary.GetAll<Rhythm4KSong>().ToList();
    //         list.AddRange(AllRuntime);
    //         return list;
    //     }
    // }
    //public static List<Rhythm4KSong> AllRuntime = new(); 

	protected override void PostLoad()
	{
		base.PostLoad();

        Init();
	}

    public void Init()
    {
        Song = SongBuilder.Load(ChartPath);
    }

}