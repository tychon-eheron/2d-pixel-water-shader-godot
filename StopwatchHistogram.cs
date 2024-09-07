using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

public class StopwatchHistogram 
{
	public StopwatchHistogram(Label label)
	{
		Debug.Assert(label != null);
		
		_label = label;	
	}
	
	public void Add(long ticks)
	{
		_histogram.TryGetValue(ticks, out int count);
		++count;
		_histogram[ticks] = count;
		
		_mode = _mode.Item2 < count ? (ticks, count) : _mode;
		
		_meanSum = _meanSum - _meanTicks[_meanIndex] + ticks;
		_meanTicks[_meanIndex] = ticks;
		_meanIndex = (_meanIndex + 1) % _meanTicks.Length;
		
		_label.Text =
			"mean: " + new TimeSpan(_meanSum / _meanTicks.Length).TotalMilliseconds + " ms\n"
			+ "mode: " + new TimeSpan(_mode.Item1).TotalMilliseconds + " ms";
	}
	
	public IDisposable Capture() =>
		new AutoStopwatch(this);
	
	private class AutoStopwatch : IDisposable
	{
		public AutoStopwatch(StopwatchHistogram parent)
		{
			_parent = parent;
			_timer = Stopwatch.StartNew();
		}
		
		public void Dispose()
		{
			_timer.Stop();
			_parent.Add(_timer.Elapsed.Ticks);
		}
		
		private readonly StopwatchHistogram _parent;
		private readonly Stopwatch _timer;
	}
	
	private readonly Dictionary<long, int> _histogram = new();
	
	private (long, int) _mode = (0, 0);
	
	private long[] _meanTicks = new long[128];
	private int _meanIndex = 0;
	private long _meanSum = 0;
	
	private readonly Label _label;
}
