using System;
using Sandbox;

namespace Rhythm4K;

[Title( "Note" )]
public sealed class NoteComponent : Component
{
	public Note Note { get; set; }
	public Lane Lane { get; set; }

	protected override void OnStart()
	{

	}

}