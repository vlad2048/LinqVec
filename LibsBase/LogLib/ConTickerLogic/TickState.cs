namespace LogLib.ConTickerLogic;

class TickState
{
	private static readonly TimeSpan MaxTickTime = TimeSpan.FromMilliseconds(100);

	private readonly System.Collections.Generic.HashSet<int> busySlots = [];
	public DateTime StartTime { get; }

	public TickState(DateTime startTime)
	{
		StartTime = startTime;
	}

	public bool FillSlotIFP(int slotIdx, DateTime timeNow) =>
		timeNow - StartTime < MaxTickTime &&
		busySlots.Add(slotIdx);
}