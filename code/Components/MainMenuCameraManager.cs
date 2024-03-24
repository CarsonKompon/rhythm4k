using System;
using Rhythm4K.Util;
using Sandbox;

namespace Rhythm4K;

public sealed class MainMenuCameraManager : Component
{
	public static MainMenuCameraManager Instance { get; private set; }

	[Property] public GameObject FrontPageCamera { get; set; }
	[Property] public GameObject PlayPageCamera { get; set; }
	[Property] public GameObject OptionsPageCamera { get; set; }
	[Property] public GameObject SongSelectPageCamera { get; set; }
	[Property] public GameObject SongSelectCarousel { get; set; }

	Transform TargetTransform;

	string currentUrl = "";

	protected override void OnAwake()
	{
		TargetTransform = Transform.World;
		Instance = this;
	}

	protected override void OnUpdate()
	{
		float delta = 1f - MathF.Pow( 0.5f, Time.Delta * 10f );
		Transform.Rotation = Rotation.Slerp( Transform.Rotation, TargetTransform.Rotation, delta );
		Transform.Position = Vector3.Lerp( Transform.Position, TargetTransform.Position, delta );

		string url = MainMenu.Instance?.CurrentUrl ?? "";
		if ( currentUrl != url )
		{
			currentUrl = url;
			SetTarget( currentUrl );
		}
	}

	public void SetTarget( string page )
	{
		SongSelectCarousel.Enabled = false;
		switch ( page )
		{
			case "/":
				FocusCamera( FrontPageCamera.Transform.World );
				break;
			case "/play":
				FocusCamera( PlayPageCamera.Transform.World );
				break;
			case "/options":
				FocusCamera( OptionsPageCamera.Transform.World );
				break;
			case "/song-select":
				SongSelectCarousel.Enabled = true;
				FocusCamera( SongSelectPageCamera.Transform.World );
				break;
		}
	}

	void FocusCamera( Transform target )
	{
		TargetTransform = target;
	}
}