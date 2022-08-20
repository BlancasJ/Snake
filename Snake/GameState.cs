using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public class GameState
    {
        public int Rows { get; }
        public int Columns { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        private readonly LinkedList<Direction> directionChanges = new LinkedList<Direction>();
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        private readonly Random random = new Random();

        public GameState(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Grid = new GridValue[rows, columns];
            Dir = Direction.Right;

            AddSnake();
            AddFood();
        }

        private void AddSnake()
        {
            int row = Rows / 2;
            for (int column = 1; column <= 3; column++)
            {
                Grid[row, column] = GridValue.Snake;
                snakePositions.AddFirst(new Position(row, column));
            }
        }

        private IEnumerable<Position> EmptyPositions()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    if (Grid[row, column] == GridValue.Empty)
                    {
                        yield return new Position(row, column);
                    }
                }
            }
        }

        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());
            if (empty.Count == 0)
            {
                return;
            }

            Position randomPosition = empty[random.Next(empty.Count)];
            Grid[randomPosition.Row, randomPosition.Column] = GridValue.Food;
        }

        public Position HeadPosition()
        {
            return snakePositions.First.Value;
        }

        public Position TailPosition()
        {
            return snakePositions.Last.Value;
        }

        public IEnumerable<Position> SnakePositions()
        {
            return snakePositions;
        }

        private void AddHead(Position pos)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Column] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            Position tail = TailPosition();
            Grid[tail.Row, tail.Column] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if (directionChanges.Count == 0)
            {
                return Dir;
            }

            return directionChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDirection)
        {
            if (directionChanges.Count == 2)
            {
                return false;
            }

            Direction LastDirection = GetLastDirection();
            return newDirection != LastDirection && newDirection != LastDirection.Opposite();
        }

        public void ChangeDirection(Direction dir)
        {
            if (CanChangeDirection(dir))
            {
                directionChanges.AddLast(dir);
            }
        }

        private bool OutsideGrid(Position pos)
        {
            bool hasExceedRows = pos.Row < 0 || pos.Row >= Rows;
            bool hasExceedColums = pos.Column < 0 || pos.Column >= Columns;
            return hasExceedRows || hasExceedColums;
        }

        private GridValue WillHit(Position newHeadPosition)
        {
            if (OutsideGrid(newHeadPosition))
            {
                return GridValue.Outside;
            }

            if (newHeadPosition == TailPosition())
            {
                return GridValue.Empty;
            }

            return Grid[newHeadPosition.Row, newHeadPosition.Column];
        }

        public void Move()
        {
            if (directionChanges.Count > 0)
            {
                Dir = directionChanges.First.Value;
                directionChanges.RemoveFirst();
            }

            Position newHeadPosition = HeadPosition().Translate(Dir);
            GridValue hit = WillHit(newHeadPosition);

            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            }
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPosition);
            }
            else if (hit == GridValue.Food)
            {
                AddHead(newHeadPosition);
                Score++;
                AddFood();
            }
        }
    }
}
