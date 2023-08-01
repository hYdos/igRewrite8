# IL

I'm going insane. Behold the ramblings of a woman going progressivly deeper into n-sanity.

I've been looking into the IL bytecode for a few days now and I've learned that VV do not just use the instructions defined in ECMA-335. Here are the instructions I've noted.

| Bytes     | ECMA-335 Name | ECMA-335 differences
|-----------|---------------|-------------------
|      0x06 | ldloc.0       | Also corresponds to 0xF2
|      0x07 | ldloc.1       | Also corresponds to 0xF3
|      0x08 | ldloc.2       | Also corresponds to 0xF4
|      0x09 | ldloc.3       | Also corresponds to 0xF5
|      0x27 | jmp           | Removed
|      0x29 | calli         | Removed
|      0x41 | bge_un        | Removed