# Introduction

This Tetris application was made for the final assignment of the subject ".NET Programming" during the second year of my
study "HBO ICT" at the Hogeschool Utrecht. The game can be played alone and in multiplayer with one other person.


## Game

When the application is started, the player is presented with 3 options. Single player, multiplayer and how to play.


### Single player
   When the player chooses single player, the player plays Tetris by himself. Score is displayed on the left side and the queue is shown on the right. 
   The player plays until the end of the game where there will be a message confirming that the game has ended.


### Multiplayer

   When the player chooses multiplayer, the player is put into a lobby where a ready-up screen is shown.  
   Another player now needs to do the same and then both players need to ready up to start the game.  
   On the left side is the screen that the player plays on, on the right side is the screen of the other player.
   Both players play their own game of tetris, but each are presented with the same queue of tetrominos.  
   The players play until their game ends. The other player keeps playing until their game ends.
   The player that ends their game first will stop transmitting data, but keep receiving from the player that is still playing. This way the player can keep watching the other player.


### How to play

   This menu option sends the user to a how-to-play Tetris guide.


### Mechanics

When the game starts, a new tetromino will spawn. The tetromino that should spawn every cycle is defined by the queue,
which is shown on the right hand side.  
This tetromino will automatically fall one place down every 1 second. The player can manually move the tetromino down
with a soft-drop or a hard-drop, or move it right or left. (Controls listed down below.)  
A soft-drop moves the tetromino down by one position. A hard-drop moves the tetromino down as far as possible.  
The player can also rotate the tetromino. The tetromino will first try to rotate 90 degrees to the right. If this move is not possible, it will try to rotate 90 degrees to the left. If neither are possible, nothing happens.  
When collision is detected and the bottom part of the tetromino touches another tetromino or the bottom of the board,
the tetromino is placed there permanently. Then, a new tetromino is spawned and the cycle repeats.  
The game ends when a new tetromino can not be spawned in the spawning position.


### Scoring system

The scoring module supports custom scoring systems, but for this project only one system was defined. It's functionality is explained
below.  
Whenever the player moves the tetromino down manually, every position moved awards one point. Automatic falling does not
reward any points.  
When the tetromino lands in it's final position, the player is awarded the amount of blocks in the tetromino, times
two.  
Once a full line is full on the board, that line will be deleted and the player will be awarded points.  
If one line is deleted, 50 points are awarded. For more than one line deleted at once, the player is awarded (
linesDeleted * 2 - 1) * 50. This means the more lines deleted at a time, the more points awarded.  
For every 5 lines removed, the timer interval for the falling tetromino is decreased. This is done with the following
formula:  
newInterval = 0.75^(totalLinesRemoved / 5).  
This means that the longer the game goes on, the less time the player has to react and the harder it gets.


### Controls

    S/ARROW DOWN    - Soft drop
    SPACE           - Hard drop
    D/ARROW RIGHT   - Move right
    A/ARROW LEFT    - Move left
    W/ARROW UP      - Rotate


## Code

This project is split up into 3 components. The TetrisClient, TetrisEngine and the TetrisServer.


### TetrisClient

    The TetrisClient is a WPF application used for representation of the game.
    The client handles the user input and displays the game in a GUI. It is updated through a callback passed to the engine. 
    This means that every time the game engine updates, the client is updated too.


### TetrisEngine
    
    The TetrisEngine is where all the game logic happens. 
    It handles the board representation, the game, the scoring and more.

### TetrisServer
    The TetrisServer is used for communication between players during the multiplayer part of the game.
    It contains very little logic and mainly serves as relay between the two players.
