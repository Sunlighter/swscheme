Latest news is at the bottom.

**November 20, 2010.** I have been working on a rather large rewrite of Pascalesque. The rewritten version, unsurprisingly called Pascalesque 2, is capable of creating classes with constructors, methods, and fields. The constructor and method bodies are Pascalesque expressions.

My intention is to replace the Proxy Generator with a new one that uses Pascalesque 2 and has far more capabilities.

I will probably also write a generate-dll function for Scheme.

Today, Pascalesque 2, which is just shy of 6,000 lines of code, generated its first DLL. There is much more work to be done, though.

**December 18, 2010.** I decided to go ahead and check in Pascalesque 2. It's even larger now... but still inaccessible from Scheme. I need to add support for interfaces and abstract methods and properties in order to generate proxies.

**March 19, 2011.** I added support for a runtime library written in Scheme and loaded from an embedded resource when the TopLevel object is constructed. It might be better to use this than to do more complex proxy generation. Pascalesque 2 is still in progress, but is bogging down...

**May 28, 2011.** Chaos has broken out! Earlier, I split my Scheme into two versions, and did not know how I was going to reconcile them. One version introduced an ambitious new Scheduler; my intention was to rework the message-based object system to use it. I wanted unification between the object system and the async operations. I partially got it but the changes are incomplete. The other version was to make Pascalesque 2 accessible from Scheme. Today I checked in both versions and merged them together, even though the scheduler work was unfinished.

The new scheduler has created a situation where all Scheme operations run on the thread pool, so there is no way to synchronously start an evaluation. I may end up fixing this via the Async CTP.

**January 27, 2013.** It has been a long time since I did anything with this Scheme. I have been distracted. Now I can spare a little time to work on it. I am undecided on what to do next.

**February 5, 2013.** I discovered that several files were missing from the Mercurial repository, preventing builds. I didn't realize this, because I have been building from the same directory. Recently I tried cloning my local repository, and attempting a build there revealed the missing files. I think I have added them all now. It should finally be possible to do a build. The moral of the story is, when doing builds, clone your repository and build from the clone.

**January 15, 2014.** In late February of last year, I made substantial modifications to this Scheme's Expression Object Model so that threads would be handled in a more uniform manner. The IRunnableStep interface was removed, and functions that used to return IRunnableStep objects were modified to post Actions to the thread pool instead. Unfortunately, these changes broke the object system and some other features. This caused me not to push them to the public repository.

Recently I discovered that my interpreter is a good deal slower than expected. It seems that posting an Action to the thread pool incurs a slight delay, which adds up if every step in the computation is posted as a new Action to the thread pool. It is likely that the code in the repository is now faster than the unpublished, modified code.

At this time I think it might be a better idea to write a self-bootstrapping Scheme to C# compiler. The code in Sunlit World Scheme has a lot of interdependencies, such as that changing any part of the Expression Object Model causes large amounts of code to have to be rewritten. A compiler would fix that; if generated code were slow I could just modify the code generator.

(Update) The use of the thread pool may not be at fault. Something like {{(for 0 1000000 (lambda (i) #t))}} takes about 81 seconds to run on my system, corresponding to about 12,000 iterations per second. Since my {{for}} function is written in Scheme, it takes quite a few runnable steps (or actions) to do a single iteration. This is too fast to be limited by timeslices (which was my first hypothesis).

I don't know how many runnable steps there are per iteration; 10 seems like a good guess. That would mean I am getting 120,000 runnable steps per second. That doesn't seem too bad, considering how much C# code can run in a single runnable step.

On the other hand, Gambit Scheme ([http://dynamo.iro.umontreal.ca/wiki/index.php/Main_Page](http://dynamo.iro.umontreal.ca/wiki/index.php/Main_Page)) completes {{(for 0 1000000 (lambda (i) #t))}} in about 0.2 seconds. This is 400 times faster. I'm running Gambit Scheme on one machine and Sunlit World Scheme on another machine, and the machines have different operating systems and CPUs, so probably a fudge factor should be allowed, but that fudge factor probably cannot be more than 4x.