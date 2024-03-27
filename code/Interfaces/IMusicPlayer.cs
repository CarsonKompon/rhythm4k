using System;
using Sandbox;

namespace Rhythm4K;

public interface IMusicPlayer
{
	MusicPlayer Music { get; set; }

	bool IsPeaking { get; set; }
	float Energy { get; set; }
	float EnergyHistoryAverage { get; set; }
	float PeakKickVolume { get; set; }

	Action OnBeat { get; set; }
}