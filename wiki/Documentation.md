The implementation of this Scheme is based on an [ExpressionObjectModel](ExpressionObjectModel).

Building this program requires Visual Studio 2010 Professional. It **might** work with other build systems but I haven't tested it. (see footnote 1)

The SchemeCode directory contains a batch file called CopyExes.bat which copies the debug executables from where the build puts them.

It also contains a few [SamplePrograms](SamplePrograms).

The distribution includes a program called [GraySpace](GraySpace) which provides color syntax highlighting by converting Scheme code to HTML.

The user documentation for Sunlit World Scheme is located in the **swscheme.xml** file. Drop this file on your web browser. The associated xsl file is a transformer that will cause most browsers to convert the document to HTML for viewing.

There is also information on [AddingFunctions](AddingFunctions).

Not all functions are documented in the documentation, for example (apropos-list) is not documented, neither is pi, e, sin, expt, ... coming soon I suppose. You can look in the source code to see how these undocumented functions work.

See also [some weird things in the source code](WeirdThings).
----
**1:** It may be possible to build this program with other tools, but I can't guarantee it. Even if the program currently builds with something else, I might at any time use some feature of VS 2010 Professional that the other build tool does not support.