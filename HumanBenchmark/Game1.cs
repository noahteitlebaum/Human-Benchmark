// Author: Noah Teitlebaum
// File Name: Game1.cs
// Project Name: PASS2
// Creation Date: April. 28, 2023
// Modified Date: May. 12, 2023
// Description: This program is built to recreate a small collection of mini-games designed to test basic abilities as a human, such as memory, reaction time and accuracy.

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Animation2D;
using Helper;

namespace HumanBenchmark
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        static Random rng = new Random();

        //Store the game states
        const int MENU = 0;
        const int PREGAME = 1;
        const int GAME = 2;
        const int ENDGAME = 3;

        //Store the game types
        const int NONE = -1;
        const int REACTION = 0;
        const int AIM = 1;
        const int SEQUENCE = 2;

        //Store the reaction game states
        const int WAIT = 0;
        const int CLICK = 1;
        const int RECLICK = 2;
        const int FINISH = 3;

        //Store the reaction wait min and max times
        const int MIN_WAIT_TIME = 1500;
        const int MAX_WAIT_TIME = 3501;

        //Store the X and Y min and max values for the aim targets
        const int MIN_TARGET_VALUE = 0;
        const int MAX_TARGET_X = 981;
        const int MAX_TARGET_Y = 551;

        //Store durations for sequence prepare and square blink timers
        const int PREPARE_TIMER_DURATION = 800;
        const int SQUARE_TIMER_DURATION = 500;

        //Store the multiple game start and end rounds
        const int START_REACTION_ROUND = 0;
        const int END_REACTION_ROUND = 4;
        const int START_AIM_ROUND = 29;
        const int END_AIM_ROUND = 0;
        const int START_SEQUENCE_ROUND = 0;
        const int END_SEQUENCE_ROUND = 14;

        //Store the hover status
        const int OFF = 0;
        const int ON = 1;

        //Store the UI layout offsets
        const int TITLE_OFFSET = 20;
        const int TITLE_SPACER = 10;
        const int MENU_TEXT_SPACER = 25;
        const int MENU_HORIZ_SPACER = 20;
        const int MENU_BTN_SPACER = 10;
        const int PRE_VERT_SPACER = 100;
        const int PRE_TEXT_SPACER = 50;
        const int END_VERT_BUTTON_SPACER = 150;
        const int END_HORIZ_BUTTON_SPACER = 100;
        const int END_TITLE_SPACER = 120;
        const int END_INST_SPACER = 200;
        const int END_ICON_SPACER = 10;
        const int WAIT_ICON_SPACER = 200;
        const int OOPS_ICON_SPACER = 140;
        const int CLOCK_ICON_SPACER = 140;
        const int REACTION_INST_SPACER = 325;
        const int SQUARE_SPACER = 135;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        //Store the reaction game colors
        Color bgRed = new Color(206, 38, 54);
        Color bgGreen = new Color(75, 219, 106);
        Color bgBlue = new Color(43, 135, 209);
        Color[] bgColors;

        //Store the current game state and game type
        int gameState = MENU;
        int gameType = NONE;

        //Store the current reaction game state and round
        int reactionGameState = WAIT;
        int reactionRound = START_REACTION_ROUND;

        //Store the reaction wait timer and the randomized duration of the timer
        Timer waitTimer;
        int ranWaitNum = rng.Next(MIN_WAIT_TIME, MAX_WAIT_TIME);

        //Store how much time has passed until the user clicks the screen
        Timer reactionClickTimer;
        int[] reactionClickTimePassed = new int[5];

        //Store the current aim game round
        int aimRound = START_AIM_ROUND;

        //Store how much time has passed until the user clicks a target
        Timer aimClickTimer;
        int[] aimClickTimePassed = new int[30];

        //Store the randomized position of the targets
        int ranTargetX;
        int ranTargetY;

        //Store the current sequence game round and the amount of squares activated / clicked
        int sequenceRound = START_SEQUENCE_ROUND;
        int squareRound;
        int numClicks;

        //Store the sequence game timers
        Timer prepareTimer;
        Timer squareTimer;

        //Store the order in which the squares will be turned on
        int[] seqOrder = new int[15];

        //Store the final game and best scores
        int[] endScores = new int[3];
        int[] bestScores = new int[] { int.MaxValue, int.MaxValue, int.MinValue };

        //Store the mouse states
        MouseState mouse;
        MouseState prevMouse;

        //Store the screen dimensions
        int screenWidth;
        int screenHeight;

        //Store the text fonts
        SpriteFont titleFont;
        SpriteFont instFont;
        SpriteFont menuFont;

        //Store the game images
        Texture2D reactionIconImg;
        Texture2D[] pregameImgs = new Texture2D[3];
        Texture2D[] reactionGameIconImgs = new Texture2D[4];
        Texture2D[] sequenceSqs = new Texture2D[2];

        //Store the on/off button images
        Texture2D[] reactionBtns = new Texture2D[2];
        Texture2D[] aimBtns = new Texture2D[2];
        Texture2D[] sequenceBtns = new Texture2D[2];
        Texture2D[] startBtns = new Texture2D[2];
        Texture2D[] tryBtns = new Texture2D[2];
        Texture2D[] saveBtns = new Texture2D[2];

        //Store the menu icon and button bounding boxes
        Rectangle menuIconRec;
        Rectangle[] menuBtnRecs = new Rectangle[3];

        //Store pregame icon bounding boxes
        Rectangle[] preIconRecs = new Rectangle[3];

        //Store the bounding boxes of the game icons
        Rectangle[] reactionGameIconRecs = new Rectangle[4];
        Rectangle[] targetRecs = new Rectangle[30];
        Rectangle[] sequenceSqRecs = new Rectangle[9];

        //Store the bounding boxes of the instruction buttons 
        Rectangle startRec;
        Rectangle tryBtnsRec;
        Rectangle saveBtnsRec;

        //Store the menu text locations
        Vector2 menuTitleLoc;
        Vector2 menuDescLoc;

        //Store the menu game text locations
        Vector2 menuReactionLoc;
        Vector2 menuAimLoc;
        Vector2 menuSequenceLoc;

        //Store the locations of the final game and end scores
        Vector2[] menuBestLocs = new Vector2[3];
        Vector2[] endScoreLocs = new Vector2[3];

        //Store the pregame text locations
        Vector2 pregameTitleLoc;
        Vector2 pregameDescLoc;

        //Store the reaction game text locations
        Vector2 [] reactionTitleLocs = new Vector2[4];     
        Vector2[] reactionDescLocs = new Vector2[4];

        //Store the aim and sequence game text locations
        Vector2 aimGameLoc;
        Vector2 sequenceGameLoc;

        //Store the end game text locations
        Vector2[] endTitleLocs = new Vector2[3];
        Vector2 endInstLoc;

        //Store the menu title and description texts
        string menuTitle = "Human Benchmark";
        string menuDesc = "Measure your abilities with brain games and cognitive tests.";

        //Store the menu game texts
        string menuReaction = "Reaction Time";
        string menuAim = "Aim Trainer";
        string menuSequence = "Sequence Memory";

        //Store the final game and end score texts
        string[] menuBestScores = new string[] { "--- ms", "--- ms", "--- pts" };
        string[] endScoreTexts = new string[3];

        //Store the pregame title and description texts
        string[] preTitle = new string[] { "Reaction Time Test",
                                           "Aim Trainer",
                                           "Sequence Memory Test" };
        string[] preDesc = new string[] { "When the red box turns green, click as quickly as you can.",
                                          "Hit 30 targets as quickly as you can.",
                                          "Memorize the pattern." };

        //Store the reaction game titles and description texts
        string[] reactionTitles = new string[] { "Wait for green", "Click!", "Too Soon!", "" };
        string[] reactionDescs = new string[] { "", "", "Click to try again.", "Click to keep going." };

        //Store the aim and sequence remaining/level texts
        string aimRemainingText = "Remaining 30";
        string sequenceLevelText = "Level: 1";

        //Store the end game texts and instructions
        string[] endTitles = new string[] { "Reaction Time Test", "Aim Trainer", "Sequence Memory Test" };
        string endInst = "Save your score to see how you compare.";

        //Store the on/off buttons
        bool hoverReactionBtn = false;
        bool hoverAimBtn = false;
        bool hoverSequenceBtn = false;
        bool hoverStartBtn = false;
        bool hoverTryBtn = false;
        bool hoverSaveBtn = false;
        bool[] hoverSquareBtns = new bool[] { false, false, false, false, false, false, false, false, false };

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            //Determine the new screen dimensions
            graphics.PreferredBackBufferWidth = 980;
            graphics.PreferredBackBufferHeight = 550;

            //Activate the visibility of the mouse
            IsMouseVisible = true;

            //Apply the initilization changes
            graphics.ApplyChanges();

            //Set the new screen dimensions
            screenWidth = graphics.GraphicsDevice.Viewport.Width;
            screenHeight = graphics.GraphicsDevice.Viewport.Height;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Determine the background colors
            bgColors = new Color[] { bgRed, bgGreen, bgBlue, bgBlue };

            //Set the reaction wait timer to the randomized duration
            waitTimer = new Timer(ranWaitNum, true);

            //Set the reaction and aim click timers
            reactionClickTimer = new Timer(Timer.INFINITE_TIMER, true);
            aimClickTimer = new Timer(Timer.INFINITE_TIMER, true);

            //Set the sequence prepare and square blink timers
            prepareTimer = new Timer(PREPARE_TIMER_DURATION, true);
            squareTimer = new Timer(SQUARE_TIMER_DURATION, false);

            //Determine the fonts for the text
            titleFont = Content.Load<SpriteFont>("Fonts/TitleFont");
            instFont = Content.Load<SpriteFont>("Fonts/InstFont");
            menuFont = Content.Load<SpriteFont>("Fonts/MenuFont");

            //Determine the reaction icon image
            reactionIconImg = Content.Load<Texture2D>("Images/Sprites/ReactionIconLg");

            //Determine the pregame icon images
            pregameImgs[REACTION] = reactionIconImg;
            pregameImgs[AIM] = Content.Load<Texture2D>("Images/Sprites/Target");
            pregameImgs[SEQUENCE] = Content.Load<Texture2D>("Images/Sprites/SequenceIconLg");

            //Determine the images for the reaction game icons
            reactionGameIconImgs[WAIT] = Content.Load<Texture2D>("Images/Sprites/ReactionWaitIcon");
            reactionGameIconImgs[CLICK] = Content.Load<Texture2D>("Images/Sprites/ReactionWaitIcon");
            reactionGameIconImgs[RECLICK] = Content.Load<Texture2D>("Images/Sprites/OopsIcon");
            reactionGameIconImgs[FINISH] = Content.Load<Texture2D>("Images/Sprites/ReactionClockIcon");

            //Load the on/off sequence square images
            sequenceSqs[OFF] = Content.Load<Texture2D>("Images/Sprites/SequenceSqOff");
            sequenceSqs[ON] = Content.Load<Texture2D>("Images/Sprites/SequenceSqOn");

            //Load the on/off buttons
            reactionBtns[OFF] = Content.Load<Texture2D>("Images/Sprites/ReactionBtn");
            reactionBtns[ON] = Content.Load<Texture2D>("Images/Sprites/ReactionHoverBtn");
            aimBtns[OFF] = Content.Load<Texture2D>("Images/Sprites/AimBtn");
            aimBtns[ON] = Content.Load<Texture2D>("Images/Sprites/AimHoverBtn");
            sequenceBtns[OFF] = Content.Load<Texture2D>("Images/Sprites/SequenceBtn");
            sequenceBtns[ON] = Content.Load<Texture2D>("Images/Sprites/SequenceHoverBtn");
            startBtns[OFF] = Content.Load<Texture2D>("Images/Sprites/StartBtn");
            startBtns[ON] = Content.Load<Texture2D>("Images/Sprites/StartHoverBtn");
            tryBtns[OFF] = Content.Load<Texture2D>("Images/Sprites/TryBtn");
            tryBtns[ON] = Content.Load<Texture2D>("Images/Sprites/TryHoverBtn");
            saveBtns[OFF] = Content.Load<Texture2D>("Images/Sprites/SaveBtn");
            saveBtns[ON] = Content.Load<Texture2D>("Images/Sprites/SaveHoverBtn");

            //Determine the bounding box for the menu icon
            menuIconRec = new Rectangle(screenWidth / 2 - reactionIconImg.Width / 2, TITLE_OFFSET, reactionIconImg.Width, reactionIconImg.Height);

            //Determine the locations for the menu title and description
            menuTitleLoc = new Vector2(screenWidth / 2 - titleFont.MeasureString(menuTitle).X / 2, menuIconRec.Bottom + TITLE_SPACER);
            menuDescLoc = new Vector2(screenWidth / 2 - instFont.MeasureString(menuDesc).X / 2, menuTitleLoc.Y + titleFont.MeasureString(menuTitle).Y + TITLE_SPACER);

            //Determine the locations for the menu game texts
            menuReactionLoc = new Vector2(screenWidth / 2 - menuFont.MeasureString(menuSequence).X / 2, (int)(menuDescLoc.Y + instFont.MeasureString(menuDesc).Y + MENU_TEXT_SPACER));
            menuAimLoc = new Vector2(menuReactionLoc.X, menuReactionLoc.Y + menuFont.MeasureString(menuSequence).Y + MENU_TEXT_SPACER);
            menuSequenceLoc = new Vector2(menuReactionLoc.X, menuAimLoc.Y + menuFont.MeasureString(menuSequence).Y + MENU_TEXT_SPACER);

            //Determine the locations for the menu best scores
            menuBestLocs[REACTION] = new Vector2(menuReactionLoc.X + menuFont.MeasureString(menuSequence).X + MENU_HORIZ_SPACER, menuReactionLoc.Y);
            menuBestLocs[AIM] = new Vector2(menuBestLocs[REACTION].X, menuAimLoc.Y);
            menuBestLocs[SEQUENCE] = new Vector2(menuBestLocs[REACTION].X, menuSequenceLoc.Y);

            //Determine the locations for the end game titles
            endTitleLocs[REACTION] = new Vector2((screenWidth - menuFont.MeasureString(endTitles[REACTION]).X) / 2, END_TITLE_SPACER);
            endTitleLocs[AIM] = new Vector2((screenWidth - menuFont.MeasureString(endTitles[AIM]).X) / 2, END_TITLE_SPACER);
            endTitleLocs[SEQUENCE] = new Vector2((screenWidth - menuFont.MeasureString(endTitles[SEQUENCE]).X) / 2, END_TITLE_SPACER);

            //Determine the location for the end game instruction
            endInstLoc = new Vector2((screenWidth - instFont.MeasureString(endInst).X) / 2, screenHeight - END_INST_SPACER);

            //Determine the bounding boxes for the menu buttons
            menuBtnRecs[REACTION] = new Rectangle((int)(menuReactionLoc.X - reactionBtns[OFF].Width - MENU_HORIZ_SPACER), (int)(menuReactionLoc.Y - 5), reactionBtns[OFF].Width, reactionBtns[OFF].Height);
            menuBtnRecs[AIM] = new Rectangle(menuBtnRecs[REACTION].X, menuBtnRecs[REACTION].Bottom + MENU_BTN_SPACER, reactionBtns[OFF].Width, reactionBtns[OFF].Height);
            menuBtnRecs[SEQUENCE] = new Rectangle(menuBtnRecs[REACTION].X, menuBtnRecs[AIM].Bottom + MENU_BTN_SPACER, reactionBtns[OFF].Width, reactionBtns[OFF].Height);

            //Determine the bounding boxes for the pregame icon images
            preIconRecs[REACTION] = new Rectangle(screenWidth / 2 - pregameImgs[REACTION].Width / 2, screenHeight / 2 - pregameImgs[REACTION].Height / 2, pregameImgs[REACTION].Width, pregameImgs[REACTION].Height);
            preIconRecs[AIM] = new Rectangle(screenWidth / 2 - pregameImgs[AIM].Width / 2, screenHeight / 2 - pregameImgs[AIM].Height / 2, pregameImgs[AIM].Width, pregameImgs[AIM].Height);
            preIconRecs[SEQUENCE] = new Rectangle(screenWidth / 2 - pregameImgs[SEQUENCE].Width / 2, screenHeight / 2 - pregameImgs[SEQUENCE].Height / 2, pregameImgs[SEQUENCE].Width, pregameImgs[SEQUENCE].Height);

            //Determine the bounding boxes for the start, try, and save buttons
            startRec = new Rectangle(screenWidth / 2 - startBtns[OFF].Width / 2, screenHeight - PRE_VERT_SPACER, startBtns[OFF].Width, startBtns[OFF].Height);
            tryBtnsRec = new Rectangle((screenWidth - tryBtns[OFF].Width) / 2 + END_HORIZ_BUTTON_SPACER, screenHeight - END_VERT_BUTTON_SPACER, tryBtns[OFF].Width, tryBtns[OFF].Height);
            saveBtnsRec = new Rectangle((screenWidth - saveBtns[OFF].Width) / 2 - END_HORIZ_BUTTON_SPACER, screenHeight - END_VERT_BUTTON_SPACER, saveBtns[OFF].Width, saveBtns[OFF].Height);

            //Determine the locations for the pregame titles and descriptions
            pregameTitleLoc = new Vector2(0, PRE_VERT_SPACER);
            pregameDescLoc = new Vector2(0, startRec.Y - PRE_TEXT_SPACER);

            //Determine the bounding boxes for the reaction game icons
            reactionGameIconRecs[WAIT] = new Rectangle((screenWidth - reactionGameIconImgs[WAIT].Width) / 2, WAIT_ICON_SPACER, reactionGameIconImgs[WAIT].Width, reactionGameIconImgs[WAIT].Height);
            reactionGameIconRecs[CLICK] = new Rectangle((screenWidth - reactionGameIconImgs[CLICK].Width) / 2, WAIT_ICON_SPACER, reactionGameIconImgs[CLICK].Width, reactionGameIconImgs[CLICK].Height);
            reactionGameIconRecs[RECLICK] = new Rectangle((screenWidth - reactionGameIconImgs[RECLICK].Width) / 2, OOPS_ICON_SPACER, reactionGameIconImgs[RECLICK].Width, reactionGameIconImgs[RECLICK].Height);
            reactionGameIconRecs[FINISH] = new Rectangle((screenWidth - reactionGameIconImgs[FINISH].Width) / 2, CLOCK_ICON_SPACER, reactionGameIconImgs[FINISH].Width, reactionGameIconImgs[FINISH].Height);

            //Determine the locations for the reaction titles
            reactionTitleLocs[WAIT] = new Vector2((screenWidth - titleFont.MeasureString(reactionTitles[WAIT]).X) / 2, (screenHeight - reactionGameIconRecs[WAIT].Height) / 2);
            reactionTitleLocs[CLICK] = new Vector2((screenWidth - titleFont.MeasureString(reactionTitles[CLICK]).X) / 2, (screenHeight - reactionGameIconRecs[CLICK].Height) / 2);
            reactionTitleLocs[RECLICK] = new Vector2((screenWidth - titleFont.MeasureString(reactionTitles[RECLICK]).X) / 2, (screenHeight - reactionGameIconRecs[RECLICK].Height) / 2);

            //Determine the locations for the reaction descriptions
            reactionDescLocs[RECLICK] = new Vector2((screenWidth - instFont.MeasureString(reactionDescs[RECLICK]).X) / 2, REACTION_INST_SPACER);
            reactionDescLocs[FINISH] = new Vector2((screenWidth - instFont.MeasureString(reactionDescs[FINISH]).X) / 2, REACTION_INST_SPACER);

            //Determine the bounding boxes for the aim target icons
            CalcTargetRec(targetRecs);

            //Determine the location for the aim target ramaining
            aimGameLoc = new Vector2((screenWidth - menuFont.MeasureString(aimRemainingText).X) / 2, TITLE_SPACER);

            //Generate the starting sequence round
            RegenerateSequenceRounds();

            //Determine the bounding boxes for the sequence square icons
            sequenceSqRecs[0] = new Rectangle((screenWidth - sequenceSqs[OFF].Width) / 2 - SQUARE_SPACER, (screenHeight - sequenceSqs[OFF].Height) / 2 - SQUARE_SPACER, sequenceSqs[OFF].Width, sequenceSqs[OFF].Height);
            sequenceSqRecs[1] = new Rectangle((screenWidth - sequenceSqs[OFF].Width) / 2, (screenHeight - sequenceSqs[OFF].Height) / 2 - SQUARE_SPACER, sequenceSqs[OFF].Width, sequenceSqs[OFF].Height);
            sequenceSqRecs[2] = new Rectangle((screenWidth - sequenceSqs[OFF].Width) / 2 + SQUARE_SPACER, (screenHeight - sequenceSqs[OFF].Height) / 2 - SQUARE_SPACER, sequenceSqs[OFF].Width, sequenceSqs[OFF].Height);
            sequenceSqRecs[3] = new Rectangle((screenWidth - sequenceSqs[OFF].Width) / 2 - SQUARE_SPACER, (screenHeight - sequenceSqs[OFF].Height) / 2, sequenceSqs[OFF].Width, sequenceSqs[OFF].Height);
            sequenceSqRecs[4] = new Rectangle((screenWidth - sequenceSqs[OFF].Width) / 2, (screenHeight - sequenceSqs[OFF].Height) / 2, sequenceSqs[OFF].Width, sequenceSqs[OFF].Height);
            sequenceSqRecs[5] = new Rectangle((screenWidth - sequenceSqs[OFF].Width) / 2 + SQUARE_SPACER, (screenHeight - sequenceSqs[OFF].Height) / 2, sequenceSqs[OFF].Width, sequenceSqs[OFF].Height);
            sequenceSqRecs[6] = new Rectangle((screenWidth - sequenceSqs[OFF].Width) / 2 - SQUARE_SPACER, (screenHeight - sequenceSqs[OFF].Height) / 2 + SQUARE_SPACER, sequenceSqs[OFF].Width, sequenceSqs[OFF].Height);
            sequenceSqRecs[7] = new Rectangle((screenWidth - sequenceSqs[OFF].Width) / 2, (screenHeight - sequenceSqs[OFF].Height) / 2 + SQUARE_SPACER, sequenceSqs[OFF].Width, sequenceSqs[OFF].Height);
            sequenceSqRecs[8] = new Rectangle((screenWidth - sequenceSqs[OFF].Width) / 2 + SQUARE_SPACER, (screenHeight - sequenceSqs[OFF].Height) / 2 + SQUARE_SPACER, sequenceSqs[OFF].Width, sequenceSqs[OFF].Height);

            //Determine the location for the sequence levels
            sequenceGameLoc = new Vector2((screenWidth - menuFont.MeasureString(sequenceLevelText).X) / 2, TITLE_SPACER);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //Assign the mouse states
            prevMouse = mouse;
            mouse = Mouse.GetState();

            //Update the appropriate game state based on the part of the game the user is on
            switch (gameState)
            {
                case MENU:
                    //Update the menu, game state
                    UpdateMenu();
                    break;
                case PREGAME:
                    //Update the pregame, game state
                    UpdatePreGame();
                    break;
                case GAME:
                    //Update the game, game state
                    UpdateGame(gameTime);
                    break;
                case ENDGAME:
                    //Update the end game, game state
                    UpdateEndGame();
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //Draw the background color as blue
            GraphicsDevice.Clear(bgBlue);

            //Start drawing the images and texts
            spriteBatch.Begin();

            //Draw the appropriate game state based on the part of the game the user is on
            switch (gameState)
            {
                case MENU:
                    //Draw the menu, game state
                    DrawMenu();
                    break;
                case PREGAME:
                    //Draw the pregame, game state
                    DrawPreGame();
                    break;
                case GAME:
                    //Draw the game, game state
                    DrawGame();
                    break;
                case ENDGAME:
                    //Draw the end game, game state
                    DrawEndGame();
                    break;
            }

            //End drawing the images and texts
            spriteBatch.End();

            base.Draw(gameTime);
        }

        //Pre: None
        //Post: None
        //Desc: Update the menu
        private void UpdateMenu()
        {
            //Turn on the menu game buttons if the user hovers over them
            hoverReactionBtn = menuBtnRecs[REACTION].Contains(mouse.Position);
            hoverAimBtn = menuBtnRecs[AIM].Contains(mouse.Position);
            hoverSequenceBtn = menuBtnRecs[SEQUENCE].Contains(mouse.Position);

            //Check to see if the user clicked the mouse left button
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
            {
                //Perform the appropriate operation based on what the user chooses
                if (menuBtnRecs[REACTION].Contains(mouse.Position))
                {
                    //Setup the reaction pregame
                    SetupPreGame(REACTION);
                }
                else if (menuBtnRecs[AIM].Contains(mouse.Position))
                {
                    //Setup the aim pregame
                    SetupPreGame(AIM);
                }
                else if (menuBtnRecs[SEQUENCE].Contains(mouse.Position))
                {
                    //Setup the sequence pregame
                    SetupPreGame(SEQUENCE);
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Update the pregame being played
        private void UpdatePreGame()
        {
            //Turn on the start button if the user hover over it
            hoverStartBtn = startRec.Contains(mouse.Position);

            //Check to see if the user clicked the mouse left button and is also hovering over the start button
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed && startRec.Contains(mouse.Position))
            {
                //Change the game state to game no matter what game is being played
                switch (gameType)
                {
                    case REACTION:
                        //TODO: Reaction Game Setup
                        gameState = GAME;
                        break;
                    case AIM:
                        //TODO: Aim Trainer Game Setup
                        gameState = GAME;
                        break;
                    case SEQUENCE:
                        //TODO: Sequence Memory Game Setup
                        gameState = GAME;
                        break;
                }
            }
        }

        //Pre: Provides a snapshot of timing values
        //Post: None
        //Desc: Update the game being played
        private void UpdateGame(GameTime gameTime)
        {
            //Update the appropriate game type based on what game is being played
            switch (gameType)
            {
                case REACTION:
                    //Update reaction game
                    UpdateReaction(gameTime);
                    break;
                case AIM:
                    //Update aim game
                    UpdateAim(gameTime);
                    break;
                case SEQUENCE:
                    //Update sequence game
                    UpdateSequence(gameTime);
                    break;
            }
        }

        //Pre: Provides a snapshot of timing values
        //Post: None
        //Desc: Update the reaction game
        private void UpdateReaction(GameTime gameTime)
        {   
            //Randomly generate a new wait time duration
            ranWaitNum = rng.Next(MIN_WAIT_TIME, MAX_WAIT_TIME);

            //Perform the appropriate operation based on which game state is active
            switch (reactionGameState)
            {
                case WAIT:
                    //Update the wait timer
                    waitTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

                    //Perform the appropriate operation based on which values are true in the game
                    if (waitTimer.IsActive() && mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
                    {
                        //Switch the reaction game state
                        reactionGameState = RECLICK;
                    }
                    else if (waitTimer.IsFinished())
                    {
                        //Switch the reaction game state
                        reactionGameState = CLICK;
                    }
                    break;

                case CLICK:
                    //Update the user click timer and calculate the time passed for each round
                    reactionClickTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
                    reactionClickTimePassed[reactionRound] = (int)reactionClickTimer.GetTimePassed();

                    //Check to see if the user clicked the mouse left button
                    if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
                    {
                        //Reset and activate the user click timer
                        reactionClickTimer.ResetTimer(true);

                        //Create a new wait timer based on the new wait time duration
                        waitTimer = new Timer(ranWaitNum, true);

                        //Perform the appropriate operation based on if the user reached the last round
                        if (reactionRound >= END_REACTION_ROUND)
                        {
                            //Calculate the end game scores
                            CalcEndScores();

                            //Reset the reaction game state and round
                            reactionGameState = WAIT;
                            reactionRound = START_REACTION_ROUND;
                        }
                        else
                        {
                            //Switch the reaction game state
                            reactionGameState = FINISH;

                            //Assign the reaction score to the click time passed and set it's location
                            reactionTitles[FINISH] = reactionClickTimePassed[reactionRound] + " ms";
                            reactionTitleLocs[FINISH] = new Vector2((screenWidth - titleFont.MeasureString(reactionTitles[FINISH]).X) / 2, (screenHeight - reactionGameIconRecs[FINISH].Height) / 2);
                        }
                    }
                    break;

                case RECLICK:
                    //Check to see if the user clicked the mouse left button
                    if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
                    {
                        //Switch the reaction game state
                        reactionGameState = WAIT;

                        //Create a new wait timer based on the new wait time duration
                        waitTimer = new Timer(ranWaitNum, true);
                    }
                    break;

                case FINISH:
                    //Check to see if the user clicked the mouse left button
                    if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
                    {
                        //Switch the reaction game state and continue onto the next level
                        reactionGameState = WAIT;
                        reactionRound++;
                    } 
                    break;
            }
        }

        //Pre: Provides a snapshot of timing values
        //Post: None
        //Desc: Update the aim game
        private void UpdateAim(GameTime gameTime)
        {
            //Update the user click timer and calculate the time passed for each target
            aimClickTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
            aimClickTimePassed[aimRound] = (int)aimClickTimer.GetTimePassed();

            //Check to see if the user clicked the mouse left button and the mouse is hovering on the target
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed && targetRecs[aimRound].Contains(mouse.Position) && CalcTargetCollision())
            {
                //Reset and activate the user click timer
                aimClickTimer.ResetTimer(true);

                //Continue onto the next target
                aimRound--;

                //Perform the appropriate operation based on if the user reached the last round
                if (aimRound < END_AIM_ROUND)
                {
                    //Calculate the end game scores
                    CalcEndScores();

                    //Reset the aim round to the beginning
                    aimRound = START_AIM_ROUND;
                }
                else
                {
                    //Calculate a new target bounding box
                    CalcTargetRec(targetRecs);
                }
            }

            //Assign the current targets remaining to the text
            aimRemainingText = "Remaining " + (aimRound + 1);
        }

        //Pre: Provides a snapshot of timing values
        //Post: None
        //Desc: Update the sequence game
        private void UpdateSequence(GameTime gameTime)
        {
            //Update the sequence timers
            prepareTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
            squareTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Check to see if the timer to prepare the user is finished
            if (prepareTimer.IsFinished())
            {
                //Active the timer for how long the squares are lit up
                squareTimer.Activate();
                hoverSquareBtns[seqOrder[squareRound]] = squareTimer.IsActive();

                //Check to see if the timer to prepare the user is finished and the amount of squares does not equal the current level
                if (squareTimer.IsFinished() && squareRound != sequenceRound)
                {
                    //Reset and activate the timer for how long the squares are lit up
                    squareTimer.ResetTimer(false);

                    //Continue onto the next lit up square
                    squareRound++;
                }
            }

            //Perform the appropriate operation based on which values are true in the game
            if (prepareTimer.IsFinished() && !squareTimer.IsActive() && squareRound == sequenceRound)
            {
                //Check to see if the user clicked the mouse left button
                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    //Turn on the square based on which one the user is hovering over
                    hoverSquareBtns[0] = sequenceSqRecs[0].Contains(mouse.Position);
                    hoverSquareBtns[1] = sequenceSqRecs[1].Contains(mouse.Position);
                    hoverSquareBtns[2] = sequenceSqRecs[2].Contains(mouse.Position);
                    hoverSquareBtns[3] = sequenceSqRecs[3].Contains(mouse.Position);
                    hoverSquareBtns[4] = sequenceSqRecs[4].Contains(mouse.Position);
                    hoverSquareBtns[5] = sequenceSqRecs[5].Contains(mouse.Position);
                    hoverSquareBtns[6] = sequenceSqRecs[6].Contains(mouse.Position);
                    hoverSquareBtns[7] = sequenceSqRecs[7].Contains(mouse.Position);
                    hoverSquareBtns[8] = sequenceSqRecs[8].Contains(mouse.Position);
                }
                else if (mouse.LeftButton == ButtonState.Released && prevMouse.LeftButton != ButtonState.Released && sequenceSqRecs.Any(rec => rec.Contains(mouse.Position)))
                {
                    //Set all the on square buttons to off
                    hoverSquareBtns = new bool[] { false, false, false, false, false, false, false, false, false };

                    //Perform the appropriate operation based on if the user is clicking the correct squares
                    if (sequenceSqRecs[seqOrder[numClicks]].Contains(mouse.Position))
                    {
                        //Continue onto the next user click
                        numClicks++;
                    }
                    else
                    {
                        //Calculate the end game scores
                        CalcEndScores();

                        //Reset and regenerate the sequence game to the beginning
                        sequenceRound = START_SEQUENCE_ROUND;
                        RegenerateSequenceRounds();
                    }
                }
            }

            //Check to see if the number of user clicks is greater than the current round
            if (numClicks > sequenceRound)
            {
                //Continue onto the next level
                sequenceRound++;

                //Check to see if the current round is greater than the end round
                if (sequenceRound > END_SEQUENCE_ROUND)
                {
                    //Calculate the end game scores
                    CalcEndScores();

                    //Reset the sequence game to the beginning
                    sequenceRound = START_SEQUENCE_ROUND;
                }

                //Regenerate the sequence game
                RegenerateSequenceRounds();
            }

            //Assign the current sequence level to the text
            sequenceLevelText = "Level: " + (sequenceRound + 1);
        }

        //Pre: None
        //Post: None
        //Desc: Update the end game
        private void UpdateEndGame()
        {
            //Turn on the try or save buttons if the user hovers over them
            hoverTryBtn = tryBtnsRec.Contains(mouse.Position);
            hoverSaveBtn = saveBtnsRec.Contains(mouse.Position);

            //Perform the appropriate operation based on if the user clicks on the try or save button
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed && tryBtnsRec.Contains(mouse.Position))
            {
                //Switch the game state
                gameState = GAME;
            }
            else if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed && saveBtnsRec.Contains(mouse.Position))
            {
                //Switch the game state
                gameState = MENU;

                //Assign the pregame icon images to a new bounding box
                preIconRecs[gameType].Y = (screenHeight - pregameImgs[gameType].Height) / 2;

                //Perform the appropriate operation based on what game is being played
                if (endScores[gameType] < bestScores[gameType] && gameType != SEQUENCE)
                {
                    //Set the best score for the current game to the new score
                    bestScores[gameType] = endScores[gameType];
                    menuBestScores[gameType] = bestScores[gameType] + " ms";
                }
                else if (endScores[gameType] > bestScores[gameType] && gameType == SEQUENCE)
                {
                    //Set the best score for the current game to the new score
                    bestScores[gameType] = endScores[gameType];
                    menuBestScores[gameType] = bestScores[gameType] + " pts";
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Draw the menu
        private void DrawMenu()
        {
            //Draw the menu icon image, title text, and instruction text
            spriteBatch.Draw(reactionIconImg, menuIconRec, Color.White);
            spriteBatch.DrawString(titleFont, menuTitle, menuTitleLoc, Color.White);
            spriteBatch.DrawString(instFont, menuDesc, menuDescLoc, Color.White);

            //Draw the on/off buttons
            spriteBatch.Draw(reactionBtns[Convert.ToInt32(hoverReactionBtn)], menuBtnRecs[REACTION], Color.White);
            spriteBatch.Draw(aimBtns[Convert.ToInt32(hoverAimBtn)], menuBtnRecs[AIM], Color.White);
            spriteBatch.Draw(sequenceBtns[Convert.ToInt32(hoverSequenceBtn)], menuBtnRecs[SEQUENCE], Color.White);

            //Draw the game titles
            spriteBatch.DrawString(menuFont, menuReaction, menuReactionLoc, Color.White);
            spriteBatch.DrawString(menuFont, menuAim, menuAimLoc, Color.White);
            spriteBatch.DrawString(menuFont, menuSequence, menuSequenceLoc, Color.White);

            //Draw the game best scores
            spriteBatch.DrawString(menuFont, menuBestScores[REACTION], menuBestLocs[REACTION], Color.Black);
            spriteBatch.DrawString(menuFont, menuBestScores[AIM], menuBestLocs[AIM], Color.Black);
            spriteBatch.DrawString(menuFont, menuBestScores[SEQUENCE], menuBestLocs[SEQUENCE], Color.Black);
        }

        //Pre: None
        //Post: None
        //Desc: Draw the pregame
        private void DrawPreGame()
        {
            //Draw the pregame icon image and the on/off start button
            spriteBatch.Draw(pregameImgs[gameType], preIconRecs[gameType], Color.White);
            spriteBatch.Draw(startBtns[Convert.ToInt32(hoverStartBtn)], startRec, Color.White);

            //Draw the title and instruction texts
            spriteBatch.DrawString(titleFont, preTitle[gameType], pregameTitleLoc, Color.White);
            spriteBatch.DrawString(instFont, preDesc[gameType], pregameDescLoc, Color.White);
        }

        //Pre: None
        //Post: None
        //Desc: Draw the game
        private void DrawGame()
        {
            //Draw the appropriate game type based on what game is being played
            switch (gameType)
            {
                case REACTION:
                    //Draw reaction game
                    DrawReaction();
                    break;
                case AIM:
                    //Draw aim game
                    DrawAim();
                    break;
                case SEQUENCE:
                    //Draw sequence game
                    DrawSequence();
                    break;
            }
        }

        //Pre: None
        //Post: None
        //Desc: Draw the end game
        private void DrawEndGame()
        {
            //Draw the pregame image icons
            spriteBatch.Draw(pregameImgs[gameType], preIconRecs[gameType], Color.White);

            //Draw the title, description, and final score
            spriteBatch.DrawString(menuFont, endTitles[gameType], endTitleLocs[gameType], Color.White);
            spriteBatch.DrawString(instFont, endInst, endInstLoc, Color.White);
            spriteBatch.DrawString(titleFont, endScoreTexts[gameType], endScoreLocs[gameType], Color.White);

            //Draw the on/off try and save buttons
            spriteBatch.Draw(tryBtns[Convert.ToInt32(hoverTryBtn)], tryBtnsRec, Color.White);
            spriteBatch.Draw(saveBtns[Convert.ToInt32(hoverSaveBtn)], saveBtnsRec, Color.White);
        }

        //Pre: None
        //Post: None
        //Desc: Draw the reaction game
        private void DrawReaction()
        {
            //Set the background color to the corresponding game state
            GraphicsDevice.Clear(bgColors[reactionGameState]);

            //Draw the icons, instructions, and titles to the corresponding game state
            spriteBatch.Draw(reactionGameIconImgs[reactionGameState], reactionGameIconRecs[reactionGameState], Color.White);
            spriteBatch.DrawString(titleFont, reactionTitles[reactionGameState], reactionTitleLocs[reactionGameState], Color.White);
            spriteBatch.DrawString(instFont, reactionDescs[reactionGameState], reactionDescLocs[reactionGameState], Color.White);
        }

        //Pre: None
        //Post: None
        //Desc: Draw the aim game
        private void DrawAim()
        {
            //Draw the target icons and target remaining text
            spriteBatch.Draw(pregameImgs[AIM], targetRecs[aimRound], Color.White);
            spriteBatch.DrawString(menuFont, aimRemainingText, aimGameLoc, Color.White);
        }

        //Pre: None
        //Post: None
        //Desc: Draw the sequence game
        private void DrawSequence()
        {
            //Draw the on/off squares
            spriteBatch.Draw(sequenceSqs[Convert.ToInt32(hoverSquareBtns[0])], sequenceSqRecs[0], Color.White);
            spriteBatch.Draw(sequenceSqs[Convert.ToInt32(hoverSquareBtns[1])], sequenceSqRecs[1], Color.White);
            spriteBatch.Draw(sequenceSqs[Convert.ToInt32(hoverSquareBtns[2])], sequenceSqRecs[2], Color.White);
            spriteBatch.Draw(sequenceSqs[Convert.ToInt32(hoverSquareBtns[3])], sequenceSqRecs[3], Color.White);
            spriteBatch.Draw(sequenceSqs[Convert.ToInt32(hoverSquareBtns[4])], sequenceSqRecs[4], Color.White);
            spriteBatch.Draw(sequenceSqs[Convert.ToInt32(hoverSquareBtns[5])], sequenceSqRecs[5], Color.White);
            spriteBatch.Draw(sequenceSqs[Convert.ToInt32(hoverSquareBtns[6])], sequenceSqRecs[6], Color.White);
            spriteBatch.Draw(sequenceSqs[Convert.ToInt32(hoverSquareBtns[7])], sequenceSqRecs[7], Color.White);
            spriteBatch.Draw(sequenceSqs[Convert.ToInt32(hoverSquareBtns[8])], sequenceSqRecs[8], Color.White);

            //Draw the sequence level text
            spriteBatch.DrawString(menuFont, sequenceLevelText, sequenceGameLoc, Color.White);
        }

        //Pre: The current game type
        //Post: None
        //Desc: Setup the pregame
        private void SetupPreGame(int newType)
        {
            //Set the game type to the game played and game state to pregame
            gameType = newType;
            gameState = PREGAME;

            //Assign the titles and description to a new location
            pregameTitleLoc.X = screenWidth / 2 - titleFont.MeasureString(preTitle[gameType]).X / 2;
            pregameDescLoc.X = screenWidth / 2 - instFont.MeasureString(preDesc[gameType]).X / 2;
        }

        //Pre: Each target ready to be randomized
        //Post: None
        //Desc: Randomize each target to a new bounding box
        private void CalcTargetRec(Rectangle[] targetRecs)
        {
            //Randomize and assign the new target dimensions
            ranTargetX = rng.Next(MIN_TARGET_VALUE, MAX_TARGET_X - preIconRecs[AIM].Width);
            ranTargetY = rng.Next(MIN_TARGET_VALUE, MAX_TARGET_Y - preIconRecs[AIM].Height);
            targetRecs[aimRound] = new Rectangle(ranTargetX, ranTargetY, pregameImgs[AIM].Width, pregameImgs[AIM].Height);
        }

        //Pre: None
        //Post: Return the resulting collision as a bool
        //Desc: Calculate the collision between the target icon and the mouse
        private bool CalcTargetCollision()
        {
            //Store the collision to false
            bool collision = false;

            //Store the distance between the target and the mouse
            int distanceX;
            int distanceY;
            int totalDistance;

            //Store the radius of the target
            double targetRadius;

            //Calculated the distance between the target and the mouse
            distanceX = targetRecs[aimRound].Center.X - mouse.Position.X;
            distanceY = targetRecs[aimRound].Center.Y - mouse.Position.Y;
            totalDistance = (int)Math.Sqrt(Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2));

            //Calculate the radius of the target
            targetRadius = preIconRecs[AIM].Width / 2;

            //Check to see if the distance between the target and the mouse is less than the radius of the target
            if (totalDistance <= targetRadius)
            {
                //Set the collision to true
                collision = true;
            }

            //Return the collision
            return collision;
        }

        //Pre: None
        //Post: None
        //Desc: Regenerate the sequence rounds
        private void RegenerateSequenceRounds()
        {
            //Randomly set the order of squares in the game
            seqOrder[sequenceRound] = GenerateSequenceIndex(sequenceRound, sequenceSqRecs.Length);

            //Reset the amount of squares active and number of clicks to the start
            squareRound = START_SEQUENCE_ROUND;
            numClicks = START_SEQUENCE_ROUND;

            //Reset the sequence timers
            prepareTimer.ResetTimer(true);
            squareTimer.ResetTimer(false);
        }

        //Pre: index is the array index being generated, numSquares is the number of grid squares
        //Post: Return a random number from 0 to numSquares - 1
        //Desc: Generate a random number that is different to the previously generated one
        private int GenerateSequenceIndex(int index, int numSquares)
        {
            //Store the previous and current index numbers
            int prev;
            int num;

            //Perform the appropriate operation based on if the game just started
            if (index == 0)
            {
                //Return the generated index number
                return rng.Next(0, numSquares);
            }
            else
            {
                //Assign the previous and current index numbers
                prev = seqOrder[index - 1];
                num = prev;

                //While the index number is equal to the previous number
                while (num == prev)
                {
                    //Re-randomize the index number
                    num = rng.Next(0, numSquares);
                }

                //Return the generated index number
                return num;
            }
        }

        //Pre: None
        //Post: None
        //Desc: Calculate the end scores
        private void CalcEndScores()
        {
            //Switch the game state to end game
            gameState = ENDGAME;

            //Assign the pregame icon images to a new bounding box
            preIconRecs[gameType].Y = END_ICON_SPACER;

            //Calculate the end scores corresponding to the game being played
            endScores[REACTION] = reactionClickTimePassed.Sum() / reactionClickTimePassed.Length;
            endScores[AIM] = aimClickTimePassed.Sum() / aimClickTimePassed.Length;
            endScores[SEQUENCE] = sequenceRound + 1;

            //Assign the texts to the end scores
            endScoreTexts[REACTION] = endScores[REACTION] + " ms";
            endScoreTexts[AIM] = endScores[AIM] + " ms";
            endScoreTexts[SEQUENCE] = endScores[SEQUENCE] + " pts";

            //Reassign the end score location
            endScoreLocs[gameType] = new Vector2((screenWidth - titleFont.MeasureString(endScoreTexts[gameType]).X) / 2, (screenHeight - titleFont.MeasureString(endScoreTexts[gameType]).Y) / 2);
        }
    }
}