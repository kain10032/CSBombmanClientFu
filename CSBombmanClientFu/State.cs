using System;
using System.Collections.Generic;
using System.Text;

namespace CSBombmanClientFu
{
    public class State
    {
        public int turn { get; set; }
        public List<int[]> walls { get; set; }
        public List<int[]> blocks { get; set; }
        public List<Player> players { get; set; }
        public List<Bomb> bombs { get; set; }
        public List<Item> items { get; set; }
        public List<int[]> fires { get; set; }

        public State(
            int turn,
            List<int[]> walls,
            List<int[]> blocks,
            List<Player> players,
            List<Bomb> bombs,
            List<Item> items,
            List<int[]> fires)
        {
            this.turn = turn;
            this.walls = walls;
            this.blocks = blocks;
            this.players = players;
            this.bombs = bombs;
            this.items = items;
            this.fires = fires;
        }
    }

    public class Player
    {
        public string name { get; set; }
        public Position pos { get; set; }
        public int power { get; set; }
        public int setBombLimit { get; set; }
        public char ch { get; set; }
        public bool isAlive { get; set; }
        public int setBombCount { get; set; }
        public int totalSetBombCount { get; set; }
        public int id { get; set; }

        public Player(
            string name,
            Position pos,
            int power,
            int setBombLimit,
            char ch,
            bool isAlive,
            int setBombCount,
            int totalSetBombCount,
            int id)
        {
            this.name = name;
            this.pos = pos;
            this.power = power;
            this.setBombLimit = setBombLimit;
            this.ch = ch;
            this.isAlive = isAlive;
            this.setBombCount = setBombCount;
            this.totalSetBombCount = totalSetBombCount;
            this.id = id;
        }
    }

    public class Position
    {
        public int x { get; set; }
        public int y { get; set; }

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Position p)
        {
            if ((object)p == null)
            {
                return false;
            }

            return (x == p.x) && (y == p.y);
        }

        public bool Equals(int[] p)
        {
            if (p.Length != 2)
            {
                return false;
            }

            return (x == p[0]) && (y == p[1]);
        }

        public List<Position> Surrounds()
        {
            List<Position> s = new List<Position>();
            s.Add(new Position(x + 1, y));
            s.Add(new Position(x - 1, y));
            s.Add(new Position(x, y + 1));
            s.Add(new Position(x, y - 1));

            return s;
        }
    }

    public class Bomb
    {
        public Position pos { get; set; }
        public int timer { get; set; }
        public int power { get; set; }

        public Bomb(Position pos, int timer, int power)
        {
            this.pos = pos;
            this.timer = timer;
            this.power = power;
        }

        // 誘爆を考慮していない
        // 壁が間にあることを考慮していない
        public int BurnDownTime(Position pos, State state)
        {
            int time = 1000;
            if (this.pos.x == pos.x)
            {
                int d = Math.Abs(this.pos.y - pos.y);
                if (d <= this.power)
                {
                    time = this.timer;
                }
            }
            else if (this.pos.y == pos.y)
            {
                int d = Math.Abs(this.pos.x - pos.x);
                if (d <= this.power)
                {
                    time = this.timer;
                }
            }
            return time;
        }
    }

    public class Item
    {
        public Position pos { get; set; }
        public char name { get; set; }

        public Item(Position pos, char name)
        {
            this.pos = pos;
            this.name = name;
        }
    }
}
