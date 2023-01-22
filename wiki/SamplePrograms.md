Three sample programs have been written so far.

**doom.scm** demonstrates generate-exe by generating a texture browser for classic Doom. Just edit doom.scm to contain the correct path to your DOOM.WAD file, and then use **SchemeRun doom.scm** to generate the executable. The executable is called **doomshow.exe**. Running **doomshow list** prints a list of all the wall textures in Doom. Running **doomshow show **_texturename_ displays the named texture in a window. Texture names are case-sensitive!

doom.scm demonstrates the make-bitmap-maker function. The make-bitmap-m function defined in the file takes parameters describing various low-color bitmap formats, and builds the Pascalesque source for a conversion function, which is then compiled directly into MSIL.

Any executable created with generate-exe will rely on ExprObjModel.dll and ControlledWindowLib.dll. Importantly, the executable generated by generate-exe will require the _same_ DLLs that were used when generate-exe was called; otherwise deserialization may not work right!

**make-bitmap-maker.scm** includes another copy of the make-bitmap-m function (actually the first copy) and some ways to test it.

**invert-server.scm** demonstrates a TCP client/server application. Instructions are here: [InvertServer](InvertServer)

**keytest.scm** demonstrates windowing using the new object system. Loading keytest.scm creates a window and a window controller. A cursor is created in the window and it can be moved around a grid with the arrow keys.