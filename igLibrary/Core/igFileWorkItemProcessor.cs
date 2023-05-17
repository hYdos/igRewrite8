namespace igLibrary.Core
{
	public abstract class igFileWorkItemProcessor : igObject
	{
		public igFileWorkItemList _workList;
		//public igThreadList _threadList;
		//public igSemaphore _workListLock;
		//public igSemaphore _workPending;
		public igFileWorkItemProcessor _nextProcessor;
		public bool _workerThreadsActive;
		public bool _allowPause;
		//public igSignal _pauseSignal;
		public int _pauseCounter;
		public abstract void Process(igFileWorkItem workItem);
		public void SendToNextProcessor(igFileWorkItem workItem)
		{
			if(workItem._status == igFileWorkItem.Status.kStatusComplete) return;
			if(_nextProcessor == null) throw new IOException($"Ran out of file processors trying to do work of type {workItem._type}, path {workItem._path}");
			_nextProcessor.Process(workItem);
		}
	}
}