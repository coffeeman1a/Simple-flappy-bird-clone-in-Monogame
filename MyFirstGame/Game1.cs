using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Threading;
using System.ComponentModel;

namespace MyFirstGame
{
    public enum GameState
    {
        Menu,
        Playing,
        GameOver
    }

    public class Game1 : Game
    {   
        // stuff for sprites and text
        Texture2D birdTexture;
        Texture2D pipeUpTexture;
        Texture2D pipeDownTexture;
        SpriteFont font;
        private List<Texture2D> cloudTexture; // list for picking random cloud texture

        // sound sectioin 
        private Song background;
        private SoundEffect scorePoint;

        // game state variable for menu, game and game over
        private GameState gameState;

        // BASED
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // game objects to draw and update
        private Player player;
        private List<Pipe> pipes;
        private List<Cloud> clouds;

        // optional variables for game control 
        private float pipeDelay = 2.5f; // Delay in seconds
        private float elapsedTime = 0f;
        private float cloudTime = 0f;
        private bool isSpawnDelayReduced = false;
        private int pipeLimit = 4;
        private int scoreOffset = 200;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            // lists of game objects
            pipes = new List<Pipe>();
            clouds = new List<Cloud>();
            // list of cloud textures
            cloudTexture = new List<Texture2D>();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            gameState = GameState.Menu; //starting with menu state

            // assets setup
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("EightBits");
            birdTexture = Content.Load<Texture2D>("bird");
            pipeDownTexture = Content.Load<Texture2D>("pipe_down");
            pipeUpTexture = Content.Load<Texture2D>("pipe_up");

            // start pos of player
            Vector2 birdPos = new Vector2(_graphics.PreferredBackBufferWidth / 2,
                                  _graphics.PreferredBackBufferHeight / 2);

            player = new Player(birdTexture, _spriteBatch, birdPos);

            base.Initialize();
        }

        protected override void LoadContent()
        {

            // TODO: use this.Content to load your game content here
            pipeDownTexture = Content.Load<Texture2D>("pipe_down");
            pipeUpTexture = Content.Load<Texture2D>("pipe_up");

            int countCloud = 3; // 3 different textures of cloud
            for (int i = 1; i <= countCloud; i++)
            {
                string textureName = $"cloud{i}"; // Формирование имени текстуры с использованием интерполяции строк
                Texture2D texture = Content.Load<Texture2D>(textureName);
                cloudTexture.Add(texture);
            }

            // loading sounds
            scorePoint = Content.Load<SoundEffect>("score_point");
            background = Content.Load<Song>("background");
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            // using game state to control game objects
            switch(gameState)
            {
                case(GameState.Menu): // menu mode 

                    if(Keyboard.GetState().IsKeyDown(Keys.Enter))
                    {
                        gameState = GameState.Playing;
                        MediaPlayer.Play(background);
                    }

                    break;

                case(GameState.Playing):

                    if((player.score / 2) % 10 == 0 && pipeDelay > 1f && !isSpawnDelayReduced && player.score > 0) // decreasing pipe spawn delay every 10 score points
                    {
                        pipeDelay -= 0.1f;
                        isSpawnDelayReduced = true; // variable to control delay decreasing
                    }

                    CloudSpawnTimer(gameTime); // void to spawn clouds in background

                    player.Update(gameTime); // updating player pos and velocity

                    if (player.pos.Y > _graphics.PreferredBackBufferHeight - player.rect.Height / 2) // you fall you lose
                    {
                        gameState = GameState.GameOver;
                    }
                    else if(player.pos.Y < player.rect.Height / 2) // player position controling, can't escape the screen
                    {
                        gameState = GameState.GameOver;
                    }

                    foreach (var item in pipes) // updating pipes
                    {
                        item.Update(gameTime);

                        if(item.rect.Intersects(player.rect)) // player collision with pipe
                        {
                            gameState = GameState.GameOver;
                        }

                        if(item.pos.X < 0 - pipeDownTexture.Width) // passing screen
                        {
                            pipes.Remove(item);
                            break;
                        }

                        if((item.pos.X + item.rect.Width < player.pos.X) & (!item.isPassed)) // passing player
                        {
                            player.score += 1; 
                            SoundEffectInstance score = scorePoint.CreateInstance();
                            score.Play();

                            item.isPassed = true;
                        }
                    }
                    // updating clouds pos
                    foreach(var item in clouds)
                    {
                        item.Update(gameTime);
                    }
                    // pipe spawn control
                    if(pipes.Count < pipeLimit) 
                    {
                        if(elapsedTime >= pipeDelay)
                        {
                            PipeRndomSpawn();
                            isSpawnDelayReduced = false;
                            elapsedTime = 0f;
                        }
                    }

                    elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;

                case (GameState.GameOver):

                    MediaPlayer.Stop(); // stop background music

                    if(Keyboard.GetState().IsKeyDown(Keys.R)) // game restart
                    {
                        gameState = GameState.Playing;
                        player.score = 0;
                        pipes.Clear();
                        clouds.Clear();
                        MediaPlayer.Play(background);
                    }

                    break;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // TODO: Add your drawing code here

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // commented code for player rect debugging
            //Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
            //pixel.SetData(new[] { Color.Red });
            //_spriteBatch.Draw(pixel, player.rect, Color.White);

            foreach (var item in clouds) // drawing clouds
            {
                item.Draw();
            }

            player.Draw(); // drawing player

            foreach (var item in pipes) // drawing pipes
            {
                item.Draw();
            }

            switch(gameState) // using game state to control drawing of sprites
            {
                case(GameState.Menu):

                    _spriteBatch.DrawString(font, "Press Enter to start", new Vector2(100, 100), Color.White);
                    break;

                case(GameState.Playing):

                    _spriteBatch.DrawString(font,
                        ((int)player.score/2).ToString(), // deviding player score by 2 because every passed pipe (up and down) he gets points
                        new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2 - scoreOffset),
                        Color.White);          
                    break;

                case(GameState.GameOver):

                    _spriteBatch.DrawString(font, "Game Over", new Vector2(100, 100), Color.White);
                    break;
            } 

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        // pipe spawn void
        private void PipeRndomSpawn()
        {   
            // setup
            int minHeight = 50; 
            int maxHeight = 200;
            int minGap = 150;
            int maxGap = 200;
            int pipeHeight = 500;
            float scoreModifier = 5f;
            float pipeSpeed = 300f;
            Random random = new Random();

            // generating random top pipe height and gap between top and down pipes
            int topPipeHeight = random.Next(minHeight, maxHeight + 1);
            int gapSize = random.Next(minGap, maxGap);

            // definition bottom pipe height
            int bottomPipeHeight = _graphics.PreferredBackBufferHeight - topPipeHeight - gapSize; // gapSize - промежуток между трубами
            
            // pipe spawn pos
            int x = _graphics.PreferredBackBufferWidth + pipeDownTexture.Width; // spawn pipe outside the screen
            float _speed = player.score / 2 * scoreModifier + pipeSpeed; // increase pipe speed when player scores

            // creating pipes and adding them to list
            pipes.Add(new Pipe(x, topPipeHeight - pipeHeight, pipeUpTexture, _spriteBatch, _speed));
            pipes.Add(new Pipe(x, topPipeHeight + gapSize, pipeDownTexture, _spriteBatch, _speed));
        }

        private void CloudSpawnTimer(GameTime gameTime)
        {   
            float c_delay = 3f; // cloud spawn delay

            if(cloudTime > c_delay)
            {
                // setup
                int minHeight = 50;
                int maxHeight = 400;
                Random random = new Random();
                // generating random cloud Y pos
                int clHeight = random.Next(minHeight, maxHeight+1);

                // creating cloud and adding it to the list
                clouds.Add(new Cloud(new Vector2(_graphics.PreferredBackBufferWidth + 100, clHeight),
                    cloudTexture[random.Next(cloudTexture.Count)],
                    _spriteBatch));
                cloudTime = 0f;
            }
            // timer
            cloudTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    class Player
    {   

        public Rectangle rect; // player's hit box
        public Vector2 pos;
        public int score = 0;

        private Texture2D texture;
        private SpriteBatch spriteBatch;
        private Vector2 velocity;
        private float speed = 10f;

        private float buttonDelay = 0.5f; // Delay in seconds
        private float p_elapsedTime = 0f;

        public Player(Texture2D texture, SpriteBatch spriteBatch, Vector2 pos)
        {
            //player setup
            this.texture = texture;
            this.spriteBatch = spriteBatch;
            this.velocity = new Vector2(0, 0);
            this.pos = pos;
            this.rect = (new Rectangle(
                (int)(pos.X - 68 / 4 + 10),
                (int)(pos.Y - 48 / 2 + 5),
                texture.Width / 2,
                texture.Height - 20));
        }

        public void Draw()
        {
            // setup for player sprite swinging 
            float minAngle = -45f;
            float maxAngle = 45f;
            float minY = 50f;
            float maxY = 500f;

            float t = MathHelper.Clamp((pos.Y - minY) / (maxY - minY), 0f, 1f);
            float angle = MathHelper.Lerp(minAngle, maxAngle, t);

            spriteBatch.Draw(texture, new Vector2(pos.X - texture.Width / 2, pos.Y - texture.Height / 2), null, Color.White, MathHelper.ToRadians(angle), Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        public void Update(GameTime gameTime)
        {
            // updating player rect
            rect.X = (int)(pos.X - 68 / 4 + 10);
            rect.Y = (int)(pos.Y - 48 / 2 + 5);

            var kstate = Keyboard.GetState(); // getting keyboard for buttons tracking

            if (kstate.IsKeyDown(Keys.Space) & (velocity.Y > 0)) // updating velocity by pressing space button
            {
                if(p_elapsedTime >= buttonDelay)
                {
                    velocity.Y -= speed;
                    p_elapsedTime = 0f;
                }

            }

            velocity.Y += speed * 2f * (float)gameTime.ElapsedGameTime.TotalSeconds; // updatin player velocity
            
            pos += velocity; // updating player pos

            // player velocity limit
            if (velocity.Y > 3)
            {
                velocity.Y = 3;
            }

            p_elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds; // timer
        }
    }

    class Pipe
    {
        public Rectangle rect; // rect for collider 
        public Vector2 pos;
        public bool isPassed = false; // for pipe and player pos tracking 

        private Texture2D texture;
        private SpriteBatch spriteBatch;
        private float speed;

        public Pipe(int x, int y, Texture2D texture, SpriteBatch spriteBatch, float speed)
        {
            // setup
            this.pos.X = x;
            this.pos.Y = y;
            this.speed = speed;
            this.texture = texture;
            this.rect = new Rectangle(x, y, texture.Width, texture.Height);
            this.spriteBatch = spriteBatch;
        }

        public void Update(GameTime gameTime) 
        {
            pos.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds; // pipes move to the left of the screen
            rect.X = (int)pos.X; // updating collider pos
        }

        public void Draw()
        {
            spriteBatch.Draw(texture, new Vector2(pos.X, pos.Y), Color.White);
        }
    }

    class Cloud
    {
        public Vector2 pos;
        private Texture2D texture;
        private SpriteBatch spriteBatch;

        public Cloud(Vector2 pos, Texture2D texture, SpriteBatch spriteBatch)
        {
            // setup
            this.pos = pos;
            this.texture = texture;
            this.spriteBatch = spriteBatch;
        }

        public void Update(GameTime gameTime)
        {
            pos.X -= 100 * (float)gameTime.ElapsedGameTime.TotalSeconds; // clouds move to the left of the screen
        }

        public void Draw()
        {
            spriteBatch.Draw(texture, new Vector2(pos.X, pos.Y), Color.White);
        }
    }
}