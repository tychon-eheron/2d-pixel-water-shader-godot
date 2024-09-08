using System.Collections.Generic;
using Godot;

public partial class Trails : Line2D {
	[Export]
	int MAX_LENGTH = 10;
	[Export]
	SubViewport subViewport;
	[Export]
	Node2D parent;

	[Export]
	float distanceAtLargestWidth = 16 * 6;
	[Export]
	float smallestTipWidth;
	[Export]
	float largestTipWidth;
	
	float lastLength;
	Vector2I offset;
	List<Vector2> queue = new();
	
	StopwatchHistogram histogram;

	public override void _Ready() {
		offset = new Vector2I(subViewport.Size.X / 2, subViewport.Size.Y / 2);
		histogram = new(GetNode<Label>("../../TimeLog"));
		
		var globalOffset = parent.GlobalPosition + offset;
		var localOffset = parent.ToLocal(globalOffset);
		for (int i = 0, ii = MAX_LENGTH; i < ii; ++i) {
			queue.Add(globalOffset);
			AddPoint(localOffset);
		}
	}

	public override void _Process(double delta) {
		using (histogram.Capture()) {
			Vector2 globalPosition = parent.GlobalPosition + offset;
			float newSegmentLength = queue[queue.Count - 1].DistanceTo(globalPosition);
			// Early exit if idle.
			if (newSegmentLength < 0.0001f && lastLength < 0.0001f) {
				return;
			}
			
			// Remove length of first segment, add length of new last segment.
			float length = lastLength - queue[0].DistanceTo(queue[1]) + newSegmentLength;
			
			// Shift points down one place.
			for (int i = 1, ii = queue.Count; i < ii; ++i) {
				queue[i - 1] = queue[i];
				SetPointPosition(i - 1, parent.ToLocal(queue[i]));
			}
			
			// Set last point to new position.
			queue[queue.Count - 1] = globalPosition;
			SetPointPosition(queue.Count - 1, parent.ToLocal(globalPosition));
			
			if (lastLength != length) {
				float widthValue = Mathf.Lerp(smallestTipWidth, largestTipWidth, Mathf.InverseLerp(0, distanceAtLargestWidth, length));
				WidthCurve.SetPointValue(0, widthValue);
				lastLength = length;
			}
		}
	}
}
