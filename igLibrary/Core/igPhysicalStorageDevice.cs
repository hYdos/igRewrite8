namespace igLibrary.Core
{
	public abstract class igPhysicalStorageDevice : igStorageDevice
	{
		public int _maxOperationsInFlight;
		public int _threadStackSize;
		public bool _scheduleReads;
		public ulong _mediaPosition;
		public float _emulatedMediaBandwidth;
		public float _emulatedMediaSeekTime;
		public igFileWorkItemList _stagingWorkItems;
		public int _stagingBufferSize;
		//public igStatHandle _writeCountStat;
		//public igStatHandle _readCountStat;
		//public igStatHandle _readSizeStat;
		//public igStatHandle _readBandwidthStat;
		//public igStatHandle _seekCountStat;
	}
}