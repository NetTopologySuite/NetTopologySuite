# Contributing

**So, you're thinking about contributing to NetTopologySuite aka NTS. Awesome!** 

We welcome contributions and these guidelines are there to help you do just that! 

## What's this NTS?

NTS is based on JTS, or Java Topology Suite. NTS just takes over code and improvements out of JTS and adds some extra .NET-specific features. 

### So what does this mean for contributing to NTS? 

We can't accept changes in behaviour of the core project code because we want to keep in sync with JTS, that being said, if you find a bug please fix it and submit a pull-request!

These projects are directly in sync with NTS:

- NetTopologySuite
- ..

These projects are extra addons we maintain:

- NetTopologySuite.IO.*

## How to contribute?

First of all don't be afraid to contribute. Most contributions are small but very valuable. Some basic rules to make things easier for everyone:

- Don't change more than you need to to fix something or implement a new feature. No 'resharping' or refactoring please.
- Think about more than yourself, this may seem strange, but NTS is used in widely different projects. Adding a feature or fixing a bug needs to take all usecases into account.
- Don't break things, try to fix a bug without changing the API or when adding a feature make sure to add, not remove stuff. If you do need to break things, get in touch by reporting an [issue](https://github.com/NetTopologySuite/NetTopologySuite/issues).

## What can you do to help?

- Documentation & samples: If you need or find undocumented features. Or you asked a question please think about documenting this for future users. You can also contribute a sample application if the use case is generic enough. Otherwise, just upload the sample project to a seperate repo and we can link to it.
- Bug reports: Very important, if something doesn't work as expected please [report the issue](https://github.com/NetTopologySuite/NetTopologySuite/issues). We need to know!
- Bug fixes: Even better, if you can fix something, please do!
- Feature requests: Need something that's not there, add it to the [issues](https://github.com/NetTopologySuite/NetTopologySuite/issues), maybe someone already implemented what you need.
- Spread the word: A project like this grows and improves when usage increases and we have more eyes on the code. So spread the word by writing blog posts or tweet about this awesome library!