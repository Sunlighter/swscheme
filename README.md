-*- coding: utf-8; fill-column: 78 -*-

# Sunlit World Scheme

This is Sunlit World Scheme, which I published on CodePlex a number of years
ago.

It is still licensed under GPLv2, as before.

It is a not-quite-standard-compliant Scheme interpreter which implements
call/cc among other features. It is designed to be extensible in various ways
but has a large number of unfinished extensions.

I have already released some projects on Github based on some of the code from
the extensions.

The version appearing here has not been modified since CodePlex was shut
down. I have made subsequent modifications, which I may or may not publish at
some point, but most of them do not really improve the usefulness of this
thing.

There is documentation in the form of the ``swscheme2.xml`` file, which is in
the ExprObjModel project in the source code. Back when I wrote this, it was
still possible to have a browser display XML with an XSLT stylesheet. Now,
most browsers do not allow XSLT to work from the file system, and require a
web server to serve the XML and XSLT files instead.

I prefer the XML over MarkDown because I can auto-generate simple tables of
contents, and also, I had a separate style for variables embedded in code, so
that you could distinguish parts of the syntax that had to be typed exactly as
shown from parts that you could replace with arguments and such.

The CodePlex download provided the Wiki pages in MarkDown format, and I have
included those as well, but CodePlex had its own Wiki language, and the
conversion to MarkDown was imperfect.
