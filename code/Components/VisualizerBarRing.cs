using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Rhythm4K;
using Sandbox;

public sealed class VisualizerBarManager : Component
{
    [Property] GameObject PlayerObject { get; set; }
    [Property] GameObject Prefab { get; set; }
    [Property]
    public int BarCount
    {
        get => _barCount;
        set
        {
            _barCount = value;
            InitBars();
        }
    }
    int _barCount = 512;
    [Property] public float RingRadius { get; set; } = 400f;
    [Property] public float BarWidth { get; set; } = 0.1f;
    [Property] public int BarWavyness { get; set; } = 4;
    [Property] public float BarWavynessAmplitude { get; set; } = 32f;
    [Property] public float WaveSpeed { get; set; } = 1f;
    [Property] public float SpinOnBeat { get; set; } = 10f;
    [Property] public float Amplitude { get; set; } = 0.1f;

    IMusicPlayer Player;

    List<GameObject> Bars = new();

    float wiggleSpeed = 1f;
    float time = 0f;

    protected override void OnStart()
    {
        InitBars();

        Player = PlayerObject.Components.GetInDescendantsOrSelf<IMusicPlayer>();
        Player.OnBeat += OnBeat;
    }

    protected override void OnUpdate()
    {
        if ( Player is null ) return;

        time += Time.Delta * wiggleSpeed;
        wiggleSpeed = wiggleSpeed.LerpTo( WaveSpeed, Time.Delta * 10f );

        var spectrum = Player.Music.Spectrum;

        for ( int i = 0; i < Bars.Count; i++ )
        {
            var index = i;
            if ( i > Bars.Count / 2 )
            {
                index = Bars.Count - i;
            }
            var value = (spectrum[index] + spectrum[index + 1] + spectrum[index + 2] + spectrum[index + 3]) / 4f;
            var bar = Bars[i];
            var width = BarWidth;
            var targetScale = new Vector3( width, width, value * Amplitude );
            bar.Transform.LocalScale = bar.Transform.LocalScale.LerpTo( targetScale, Time.Delta * 10f );
            bar.Transform.LocalPosition = new Vector3(
                MathF.Sin( i / (float)_barCount * (2 * MathF.PI) ) * RingRadius,
                MathF.Cos( i / (float)_barCount * (2 * MathF.PI) ) * RingRadius,
                value + (MathF.Sin( time + (i / (_barCount / (float)BarWavyness)) * (2 * MathF.PI) ) * BarWavynessAmplitude)
            );
        }
    }

    void OnBeat()
    {
        wiggleSpeed = MathF.Abs( wiggleSpeed * SpinOnBeat ) * MathF.Sign( SpinOnBeat );
        wiggleSpeed = wiggleSpeed.Clamp( -WaveSpeed * SpinOnBeat, WaveSpeed * SpinOnBeat );
    }

    void InitBars()
    {
        for ( int i = 0; i < Bars.Count; i++ )
        {
            Bars[i].Destroy();
        }
        Bars.Clear();

        for ( int i = 0; i < _barCount; i++ )
        {
            var bar = Prefab.Clone();
            bar.SetParent( GameObject );
            bar.Transform.LocalPosition = new Vector3( MathF.Sin( i / (float)_barCount * (2 * MathF.PI) ) * RingRadius, MathF.Cos( i / (float)_barCount * (2 * MathF.PI) ) * RingRadius, 0 );
            bar.Enabled = true;
            Bars.Add( bar );
        }
    }
}