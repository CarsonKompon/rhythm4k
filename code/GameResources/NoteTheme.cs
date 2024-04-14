using System;
using Sandbox;

namespace Rhythm4K;

[GameResource( "Note Theme", "notethem", "Describes the appearance of notes and lanes", Icon = "piano" )]
public class NoteTheme : GameResource
{
    public string Name { get; set; } = "Unnamed Theme";
    public PrefabFile NotePrefab { get; set; }
    public PrefabFile BurstPrefab { get; set; }
    public PrefabFile ReceptorPrefab { get; set; }

    public static List<NoteTheme> All => ResourceLibrary.GetAll<NoteTheme>().ToList();

    public static NoteTheme GetFromResourceName( string resourceName )
    {
        return All.FirstOrDefault( x => x.ResourceName == resourceName );
    }
}