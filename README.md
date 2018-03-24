# Find Awkward Expressions
Search for inappropriate words among a set of text.

It uses the Aho–Corasick algorithm.
But, not the way you would expect it.
Generally, we don't want to search for fixed expression.
We would like to use wildcard to catch more expression.

Unfortunately, the Aho–Corasick algorithm doesn't give us the possibility to do this.
To overcome this limitation, I use it in a different way.
It uses the longuest fixed expression.
The algorithm will tell us which Regex to evaluate.
But, if it is an exact expression, no need to do extra processing.
So, we reduce the complexities and the number of regex check.

The wildcard is *. It mean at least one character. It can be used to represent one word or a part of one word. You can use multiple wildcard in the same word.

# Copyright and license
Code released under the MIT license.
