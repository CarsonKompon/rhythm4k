using System;
using Sandbox;

namespace Rhythm4K;

[Title( "Note" )]
public sealed class NoteComponent : Component
{
	public Note Note { get; set; }
	public Lane Lane { get; set; }

	public TimeSince CurrentTime { get; set; } = 0f;

	protected override void OnStart()
	{

	}

}