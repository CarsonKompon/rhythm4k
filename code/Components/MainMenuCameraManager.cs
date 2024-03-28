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
	public bool FadingOut = false;

	protected override void OnAwake()
	{
		TargetTransform = Transform.World;
		Instance = this;
	}

	protected override void OnUpdate()
	{
		if ( FadingOut )
		{
			Transform.Position -= Transform.Rotation.Forward * Time.Delta * 50f;
			return;
		}

		float delta = 1f - MathF.Pow( 0.5f, Time.Delta * 10f );
		var targetRot = TargetTransform.Rotation;
		var mouseOffset = new Vector2( Screen.Width / 2f, Screen.Height / 2f ) - Mouse.Position;
		targetRot *= Rotation.From( -mouseOffset.y / 500f, mouseOffset.x / 500f, 0f );
		Transform.Rotation = Rotation.Slerp( Transform.Rotation, targetRot, delta );
		Transform.Position = Vector3.Lerp( Transform.Position, TargetTransform.Position, delta );

		string url = MainMenuScreen.Instance?.CurrentUrl ?? "";
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
				SongListCarousel.Instance?.Refresh();
				break;
		}
	}

	void FocusCamera( Transform target )
	{
		TargetTransform = target;
	}
}