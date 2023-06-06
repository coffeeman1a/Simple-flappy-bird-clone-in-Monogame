using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Threading;

namespace MyFirstGame
{
    public class Game1 : Game
    {
        Texture2D birdTexture;
        Texture2D pipeUpTexture;
        Texture2D pipeDownTexture;

        private Vector2 birdPos;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Player player;
        private List<Pipe> pipes;

        private float PipeDelay = 2.5f; // Delay in seconds
        private float elapsedTime = 0f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            pipes = new List<Pipe>();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
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

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            player.Update(gameTime);
            if(player.pos.Y > _graphics.PreferredBackBufferHeight - player.rect.Height / 2)
            {
                player.pos.Y = _graphics.PreferredBackBufferHeight / 2 - player.rect.Height;
            }
            else if(player.pos.Y < player.rect.Height / 2)
            {
                player.pos.Y = player.rect.Height / 2;
            }

            foreach(var item in pipes)
            {
                
                item.Update(gameTime);

                if(item.pos.X < 0 - pipeDownTexture.Width)
                {
                    pipes.Remove(item);
                    break;
                }
            }
            
            if(pipes.Count < 3)
            {
                if(elapsedTime >= PipeDelay)
                {
                    PipeRndomSpawn();
                    elapsedTime = 0f;
                }
            }

            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // TODO: Add your drawing code here
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            player.Draw();

            foreach(var item in pipes)
            {
                item.Draw();
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void PipeRndomSpawn()
        {
            int minHeight = 50; 
            int maxHeight = 200;
            int minGap = 100;
            int maxGap = 200;
            Random random = new Random();

            // Генерация случайной высоты для верхней трубы
            int topPipeHeight = random.Next(minHeight, maxHeight + 1);
            int gapSize = random.Next(minGap, maxGap);
            // Определение высоты нижней трубы как разницы между высотой экрана и высотой верхней трубы
            int bottomPipeHeight = _graphics.PreferredBackBufferHeight - topPipeHeight - gapSize; // gapSize - промежуток между трубами

            int x = _graphics.PreferredBackBufferWidth + pipeDownTexture.Width;

            // Создание верхней и нижней трубы с заданными размерами
            pipes.Add(new Pipe(x, topPipeHeight - 500, 100, topPipeHeight, pipeUpTexture, _spriteBatch));
            pipes.Add(new Pipe(x, topPipeHeight + gapSize, 100, bottomPipeHeight, pipeDownTexture, _spriteBatch));
        }
    }

    class Player
    {
        public Rectangle rect;
        public Vector2 pos;

        private Texture2D texture;
        private SpriteBatch spriteBatch;
        private Vector2 velocity;
        private float speed = 10f;

        private float buttonDelay = 0.5f; // Delay in seconds
        private float elapsedTime = 0f;

        public Player(Texture2D texture, SpriteBatch spriteBatch, Vector2 pos)
        {
            this.texture = texture;
            this.spriteBatch = spriteBatch;
            this.velocity = new Vector2(0, 0);
            this.pos = pos;
            this.rect = (new Rectangle(
                (int)(pos.X - 64 / 2),
                (int)(pos.Y - 64 / 2),
                64,
                64));
        }

        public void Draw()
        {

            float minAngle = -60f;
            float maxAngle = 60f;
            float minY = 50f;
            float maxY = 500f;

            rect.X = (int)(pos.X - 64 / 2);
            rect.Y = (int)(pos.Y - 64 / 2);

            float t = MathHelper.Clamp((pos.Y - minY) / (maxY - minY), 0f, 1f);
            float angle = MathHelper.Lerp(minAngle, maxAngle, t);

            spriteBatch.Draw(texture, rect, null, Color.White, MathHelper.ToRadians(angle), Vector2.Zero, SpriteEffects.None, 0f);
        }

        public void Update(GameTime gameTime)
        {
            var kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.Space) & (velocity.Y > 0))
            {
                if(elapsedTime >= buttonDelay)
                {
                    velocity.Y -= speed;
                    elapsedTime = 0f;
                }

            }

            velocity.Y += speed * 2f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            pos += velocity;

            if (velocity.Y > 3)
            {
                velocity.Y = 3;
            }

            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    class Pipe
    {
        public Rectangle rect;
        public Vector2 pos;

        private Texture2D texture;
        private float speed;
        private SpriteBatch spriteBatch;

        public Pipe(int x, int y, int width, int height, Texture2D texture, SpriteBatch spriteBatch)
        {

            this.pos.X = x;
            this.pos.Y = y;
            this.speed = 300f;
            this.texture = texture;
            this.rect = new Rectangle(x, y, width, height);
            this.spriteBatch = spriteBatch;
        }

        public void Update(GameTime gameTime) 
        {
            pos.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw()
        {
            spriteBatch.Draw(texture, new Vector2(pos.X, pos.Y), Color.White);
        }
    }
}