This repo houses various bits and bobs from a community project I've worked on and off somewhere between 2020 and 2022.
The project was a top-down single-player RPG made in Unity 2019-something.

The project never took off in the state that I've based this repo on, so this code never matured into anything more than a pre-production prototype.
Moreover, only the files that were produced entirely by myself are left in; other code and assets that hook into it are missing, and there might be inconsistencies between different parts due to how some work from a WIP branch was included (as I was in the middle of implementing GOAP, the project went to a different direction, I had to switch gears, and then it all kinda petered out).
Thus, this is not supposed to be complete - or even working at all - in any way, and is merely for demonstration/reference purposes.

This repo includes:
- A game sequence system for managing the lifetime of additive scenes containing menus, in-game locations, etc
- An audio source manager (though at that time it only dealt with BGM).
- A registry to hold per-tile state, thus avoiding having to create a game object per tile when gameplay-driven map updates are necessary (like when something catches fire from a fireball spell)
- An exception-friendly coroutine wrapper utility (async was a bit inconsistent then)
- An unfinished (sadly) implementation of Goal Oriented Action Planning (GOAP) AI
- And other project-specific scripts and editor hacks authored or adopted from other sources by myself over the course of participating in the project