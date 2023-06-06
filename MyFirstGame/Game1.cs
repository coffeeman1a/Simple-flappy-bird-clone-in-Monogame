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
        Texture2D birdTexture;
        Texture2D pipeUpTexture;
        Texture2D pipeDownTexture;
        Texture2D b_cloud1, b_cloud2, b_cloud3;
        SpriteFont font;

        private Song background;

        private SoundEffect scorePoint;


        private GameState gameState;

        private Vector2 birdPos;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Player player;
        private List<Pipe> pipes;
        private List<Cloud> clouds;
        private List<Texture2D> cloudTexture;

        private float pipeDelay = 2.5f; // Delay in seconds
        private float elapsedTime = 0f;
        private float cloudTime = 0f;
        private bool isSpawnDelayReduced = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            pipes = new List<Pipe>();
            clouds = new List<Cloud>();
            cloudTexture = new List<Texture2D>();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            gameState = GameState.Menu;
            font = Content.Load<SpriteFont>("EightBits");
            birdPos = new Vector2(_graphics.PreferredBackBufferWidth / 2,
                                  _graphics.PreferredBackBufferHeight / 2);

            birdTexture = Content.Load<Texture2D>("bird");
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            pipeDownTexture = Content.Load<Texture2D>("pipe_down");
            pipeUpTexture = Content.Load<Texture2D>("pipe_up");

            player = new Player(birdTexture, _spriteBatch, birdPos);
            base.Initialize();
        }

        protected override void LoadContent()
        {

            // TODO: use this.Content to load your game content here
            pipeDownTexture = Content.Load<Texture2D>("pipe_down");
            pipeUpTexture = Content.Load<Texture2D>("pipe_up");

            b_cloud1 = Content.Load<Texture2D>("cloud1");
            b_cloud2 = Content.Load<Texture2D>("cloud2");
            b_cloud3 = Content.Load<Texture2D>("cloud3");
            cloudTexture.Add(b_cloud1);
            cloudTexture.Add(b_cloud2);
            cloudTexture.Add(b_cloud3);

            scorePoint = Content.Load<SoundEffect>("score_point");
            background = Content.Load<Song>("background");
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            switch(gameState)
            {
                case(GameState.Menu):
                    if(Keyboard.GetState().IsKeyDown(Keys.Enter))
                    {
                        gameState = GameState.Playing;
                        MediaPlayer.Play(background);
                    }
                    break;

                case(GameState.Playing):

                    if ((player.score / 2) % 10 == 0 && pipeDelay > 1f && !isSpawnDelayReduced && player.score > 0)
                    {
                        pipeDelay -= 0.1f;
                        isSpawnDelayReduced = true;
                    }

                    CloudSpawnTimer(gameTime);
                    player.Update(gameTime);
                    if (player.pos.Y > _graphics.PreferredBackBufferHeight - player.rect.Height / 2)
                    {
                        player.pos.Y = _graphics.PreferredBackBufferHeight / 2 - player.rect.Height;
                    }
                    else if(player.pos.Y < player.rect.Height / 2)
                    {
                        player.pos.Y = player.rect.Height / 2;
                    }

                    foreach (var item in pipes)
                    {
                        item.Update(gameTime);

                        if(item.rect.Intersects(player.rect))
                        {
                            gameState = GameState.GameOver;
                        }

                        if(item.pos.X < 0 - pipeDownTexture.Width)
                        {
                            pipes.Remove(item);
                            break;
                        }

                        if((item.pos.X + item.rect.Width < player.pos.X) & (!item.isPassed))
                        {
                            player.score += 1;
                            SoundEffectInstance score = scorePoint.CreateInstance();
                            score.Play();
                            item.isPassed = true;
                            Debug.WriteLine(player.score);
                        }
                    }

                    foreach(var item in clouds)
                    {
                        item.Update(gameTime);
                    }

                    if(pipes.Count < 4)
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
                    MediaPlayer.Stop();
                    if(Keyboard.GetState().IsKeyDown(Keys.R)) 
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
            int scoreOffset = 200;

            //Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
            //pixel.SetData(new[] { Color.Red });


            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            //_spriteBatch.Draw(pixel, player.rect, Color.White);
            foreach (var item in clouds)
            {
                item.Draw();
            }

            player.Draw();
            foreach (var item in pipes)
            {
                item.Draw();
            }

            switch(gameState)
            {
                case(GameState.Menu):
                    _spriteBatch.DrawString(font, "Press Enter to start", new Vector2(100, 100), Color.White);
                    break;
                case(GameState.Playing):
                    _spriteBatch.DrawString(font, ((int)player.score/2).ToString(), new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2 - scoreOffset), Color.White);          
                    
                    break;
                case(GameState.GameOver):
                    _spriteBatch.DrawString(font, "Game Over", new Vector2(100, 100), Color.White);
                    break;
            } 

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void PipeRndomSpawn()
        {
            int minHeight = 50; 
            int maxHeight = 200;
            int minGap = 150;
            int maxGap = 200;
            Random random = new Random();

            // Генерация случайной высоты для верхней трубы
            int topPipeHeight = random.Next(minHeight, maxHeight + 1);
            int gapSize = random.Next(minGap, maxGap);
            // Определение высоты нижней трубы как разницы между высотой экрана и высотой верхней трубы
            int bottomPipeHeight = _graphics.PreferredBackBufferHeight - topPipeHeight - gapSize; // gapSize - промежуток между трубами

            int x = _graphics.PreferredBackBufferWidth + pipeDownTexture.Width;
            float _speed = player.score / 2 * 5 + 300f;
            // Создание верхней и нижней трубы с заданными размерами
            pipes.Add(new Pipe(x, topPipeHeight - 500, pipeUpTexture, _spriteBatch, _speed));
            pipes.Add(new Pipe(x, topPipeHeight + gapSize, pipeDownTexture, _spriteBatch, _speed));
        }

        private void CloudSpawnTimer(GameTime gameTime)
        {
            float c_delay = 3f;
            if(cloudTime > c_delay)
            {
                int minHeight = 50;
                int maxHeight = 400;
                Random random = new Random();

                int clHeight = random.Next(minHeight, maxHeight+1);
                clouds.Add(new Cloud(new Vector2(_graphics.PreferredBackBufferWidth + b_cloud1.Width, clHeight),
                    cloudTexture[random.Next(cloudTexture.Count)],
                    _spriteBatch));
                cloudTime = 0f;
            }
            cloudTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
           
        }
    }

    class Player
    {
        public Rectangle rect;
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

            float minAngle = -45f;
            float maxAngle = 45f;
            float minY = 50f;
            float maxY = 500f;

            rect.X = (int)(pos.X - 68 / 4 + 10);
            rect.Y = (int)(pos.Y - 48 / 2 + 5);

            float t = MathHelper.Clamp((pos.Y - minY) / (maxY - minY), 0f, 1f);
            float angle = MathHelper.Lerp(minAngle, maxAngle, t);

            spriteBatch.Draw(texture, new Vector2(pos.X - texture.Width / 2, pos.Y - texture.Height / 2), null, Color.White, MathHelper.ToRadians(angle), Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        public void Update(GameTime gameTime)
        {
            var kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.Space) & (velocity.Y > 0))
            {
                if(p_elapsedTime >= buttonDelay)
                {
                    velocity.Y -= speed;
                    p_elapsedTime = 0f;
                }

            }

            velocity.Y += speed * 2f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            pos += velocity;

            if (velocity.Y > 3)
            {
                velocity.Y = 3;
            }

            p_elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    class Pipe
    {
        public Rectangle rect;
        public Vector2 pos;
        public bool isPassed = false;

        private Texture2D texture;
        private SpriteBatch spriteBatch;
        private float speed;

        public Pipe(int x, int y, Texture2D texture, SpriteBatch spriteBatch, float speed)
        {

            this.pos.X = x;
            this.pos.Y = y;
            this.speed = speed;
            this.texture = texture;
            this.rect = new Rectangle(x, y, texture.Width, texture.Height);
            this.spriteBatch = spriteBatch;
        }

        public void Update(GameTime gameTime) 
        {
            pos.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            rect.X = (int)pos.X;
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
            this.pos = pos;
            this.texture = texture;
            this.spriteBatch = spriteBatch;
        }

        public void Update(GameTime gameTime)
        {
            pos.X -= 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw()
        {
            spriteBatch.Draw(texture, new Vector2(pos.X, pos.Y), Color.White);
        }
    }
}