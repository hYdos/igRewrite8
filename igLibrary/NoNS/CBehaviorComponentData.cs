/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary
{
	public class CBehaviorComponentData : CEntityComponentData
	{
		public string  _behaviorFile;
		public string  _characterName;
		public string  _behaviorEventsFile;
		public string  _startState;
		public igObject _eventFilterData;
		public igObject _handlers;
	}
}