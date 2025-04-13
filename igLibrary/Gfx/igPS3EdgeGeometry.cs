/*
    Copyright (c) 2022-2025, The igLibrary Contributors.
    igLibrary and its libraries are free software: You can redistribute it and
    its libraries under the terms of the Apache License 2.0 as published by
    The Apache Software Foundation.
    Please see the LICENSE file for more details.
*/


namespace igLibrary.Gfx
{
    public class igPS3EdgeGeometry : igPS3EdgeGeometrySegmentList
    {
        public bool _isMorphed;
        public bool _isSkinned;
        public bool _isSpeedTree;
        public bool _isVertexAnimated;
        public bool _hasVertexColor;
        public uint _hashCode;
    }
}