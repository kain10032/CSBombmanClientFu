using System;
using System.Collections.Generic;
using System.Text;

namespace CSBombmanClientFu
{
    class Com
    {
        private const string LEFT = "LEFT";
        private const string RIGHT = "RIGHT";
        private const string UP = "UP";
        private const string DOWN = "DOWN";
        private const string STAY = "STAY";

        private const int AREA_SIZE = 15;

        private int myId = -1;
        private const string playerName = "ふ";

        public Com() { }

        public void Run()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            // player name
            Console.WriteLine(playerName, Console.OutputEncoding.CodePage);

            // get player id
            SetId(Console.ReadLine());

            while (true)
            {
                string data = Console.ReadLine();
                State state = null;

                try
                {
                    state = Newtonsoft.Json.JsonConvert.DeserializeObject<State>(data);
                }
                catch (Exception e)
                {
                    state = null;
                    continue;
                }
                AnalyzeArea(state);
            }
        }

        private void AnalyzeArea(State state)
        {
            Position mypos = state.players[myId].pos;
            int[,] movableArea = new int[AREA_SIZE, AREA_SIZE];
            int[,] explodeTime = new int[AREA_SIZE, AREA_SIZE];
            int[,] canDestloyBlock = new int[AREA_SIZE, AREA_SIZE];
            for (int i=0; i<AREA_SIZE; i++)
            {
                for (int j=0; j<AREA_SIZE; j++)
                {
                    movableArea[i, j] = -1;
                    explodeTime[i, j] = -1;
                    canDestloyBlock[i, j] = 0;
                }
            }

            foreach (int[] wall in state.walls)
            {
                movableArea[wall[0], wall[1]] = -3;
            }
            foreach (int[] block in state.blocks)
            {
                movableArea[block[0], block[1]] = -2;
            }

            // 爆風の及ぶ位置および及ぶまでの時間の計算
            // TODO: 誘爆の考慮
            foreach (Bomb bomb in state.bombs)
            {
                for (int i=0; i<bomb.power+1; i++)
                {
                    if (movableArea[bomb.pos.x+i, bomb.pos.y] < -1)
                    {
                        break;
                    }
                    explodeTime[bomb.pos.x+i, bomb.pos.y] = bomb.timer;
                }
                for (int i = 0; i < bomb.power + 1; i++)
                {
                    if (movableArea[bomb.pos.x - i, bomb.pos.y] < -1)
                    {
                        break;
                    }
                    explodeTime[bomb.pos.x - i, bomb.pos.y] = bomb.timer;
                }
                for (int i = 0; i < bomb.power + 1; i++)
                {
                    if (movableArea[bomb.pos.x, bomb.pos.y + i] < -1)
                    {
                        break;
                    }
                    explodeTime[bomb.pos.x, bomb.pos.y + i] = bomb.timer;
                }
                for (int i = 0; i < bomb.power + 1; i++)
                {
                    if (movableArea[bomb.pos.x, bomb.pos.y - i] < -1)
                    {
                        break;
                    }
                    explodeTime[bomb.pos.x, bomb.pos.y - i] = bomb.timer;
                }
            }
            foreach (int[] fire in state.fires)
            {
                explodeTime[fire[0], fire[1]] = 0;
            }

            // 幅優先探索で移動できる範囲をスコアリングする
            Queue<Position> search = new Queue<Position>();
            search.Enqueue(state.players[myId].pos);
            movableArea[state.players[myId].pos.x, state.players[myId].pos.y] = 0;
            while (search.Count > 0)
            {
                Position posDeq = search.Dequeue();
                int moveTime = movableArea[posDeq.x, posDeq.y];

                List<Position> posList = posDeq.Surrounds();
                foreach (Position pos in posList)
                {
                    if (movableArea[pos.x, pos.y] == -1)
                    {
                        movableArea[pos.x, pos.y] = moveTime + 1;
                        search.Enqueue(pos);
                    } else if (movableArea[pos.x, pos.y] == -2)
                    {
                        canDestloyBlock[posDeq.x, posDeq.y] += 1;
                    }
                }

            }

            // 次の狙いの場所を定める。
            Position minMovePos = null;
            int minMoveTime = 1000;
            for (int i=0; i<AREA_SIZE; i++)
            {
                for (int j=0; j<AREA_SIZE; j++)
                {
                    if ((explodeTime[i,j] < 0 || explodeTime[i,j]>6) && canDestloyBlock[i,j] > 0 && movableArea[i,j]>-1)
                    {
                        if (minMovePos == null || minMoveTime > movableArea[i,j])
                        {
                            minMovePos = new Position(i, j);
                            minMoveTime = movableArea[i, j];
                        }
                    }
                }
            }

            string direct = "STAY";
            string bombBool = "false";
            if (minMovePos != null)
            {
                direct = GetFirstDirect(mypos, minMovePos, movableArea, explodeTime);
                bombBool = "false";
                if (minMoveTime == 0)
                {
                    bombBool = "true";
                }
            } 

            string[] ss = new string[] { direct, bombBool, "message" };

            Console.WriteLine(string.Join(",", ss));

            return;
        }

        private string GetFirstDirect(Position startpos, Position goalpos, int[,] moveMatrix, int[,] explodeTime)
        {
            int[] start = new int[2] { startpos.x, startpos.y };
            int[] goal = new int[2] { goalpos.x, goalpos.y };

            int time = moveMatrix[goal[0], goal[1]];
            int[] now = goal;
            int[] prev = goal;
            string firstDirect = "STAYEE";

            List<string> enableDirects = new List<string>();
            int allowExplodeTime = 1;
            // 大丈夫な方向を探しておく
            if (explodeTime[start[0], start[1]] > allowExplodeTime || explodeTime[start[0], start[1]] == -1)
            {
                enableDirects.Add("STAY");
            }
            if (explodeTime[start[0]-1, start[1]] > allowExplodeTime || explodeTime[start[0]-1, start[1]] == -1)
            {
                enableDirects.Add("LEFT");
            }
            if (explodeTime[start[0]+1, start[1]] > allowExplodeTime || explodeTime[start[0]+1, start[1]] == -1)
            {
                enableDirects.Add("RIGHT");
            }
            if (explodeTime[start[0], start[1]-1] > allowExplodeTime || explodeTime[start[0], start[1]-1] == -1)
            {
                enableDirects.Add("DOWN");
            }
            if (explodeTime[start[0], start[1]+1] > allowExplodeTime || explodeTime[start[0], start[1]+1] == -1)
            {
                enableDirects.Add("UP");
            }

            while (time > 0)
            {
                prev = now;
                time -= 1;
                if (moveMatrix[now[0]-1, now[1]] == time)
                {
                    now[0] -= 1;
                    firstDirect = "RIGHT";
                }
                else if (moveMatrix[now[0]+1, now[1]] == time)
                {
                    now[0] += 1;
                    firstDirect = "LEFT";
                }
                else if (moveMatrix[now[0], now[1]-1] == time)
                {
                    now[1] -= 1;
                    firstDirect = "DOWN";
                }
                else if (moveMatrix[now[0], now[1] + 1] == time)
                {
                    now[1] += 1;
                    firstDirect = "UP";
                }
                else
                {
                    firstDirect = "STAY";
                    break;
                }
            }

            // 次の経路にいけない場合
            if (!enableDirects.Contains(firstDirect))
            {
                firstDirect = enableDirects[0];
            }

            return firstDirect;
        }

        private void Action(State state)
        {
            string[] response = new string[] { STAY, "false" };

            string direct = STAY;

            double dp = 0.0;
            double dpTmp = 0.0;

            Player me = state.players[myId];
            Position pos = me.pos;
            dp = DangerProb(pos, state);

            // LEFT
            pos = me.pos;
            pos.x = pos.x + 1;
            dpTmp = DangerProb(pos, state);
            if (dpTmp < dp)
            {
                dp = dpTmp;
                direct = LEFT;
            }

            // RIGHT
            pos = me.pos;
            pos.x = pos.x - 1;
            dpTmp = DangerProb(pos, state);
            if (dpTmp < dp)
            {
                dp = dpTmp;
                direct = RIGHT;
            }

            // UP
            pos = me.pos;
            pos.y = pos.y + 1;
            dpTmp = DangerProb(pos, state);
            if (dpTmp < dp)
            {
                dp = dpTmp;
                direct = UP;
            }

            // DOWN
            pos = me.pos;
            pos.y = pos.y - 1;
            dpTmp = DangerProb(pos, state);
            if (dpTmp < dp)
            {
                dp = dpTmp;
                direct = DOWN;
            }

            // 爆弾を置くかどうか
            Bomb b = new Bomb(state.players[myId].pos, 10, state.players[myId].power);
            response[0] = direct;
            response[1] = "false";


            Console.WriteLine(string.Join(",", response));

            return;
        }

        // set player id string to int variable myId.
        private void SetId(string idStr)
        {
            if (int.TryParse(idStr, out int x))
            {
                myId = x;
            }
        }

        // TODO: 壁が迫ってくる場合の考慮
        private double DangerProb(Position pos, State state)
        {
            int bdtime = 1000;
            foreach (Bomb b in state.bombs)
            {
                bdtime = Math.Min(bdtime, b.BurnDownTime(pos, state));
            }
            foreach (int[] f in state.fires)
            {
                if (pos.Equals(f))
                {
                    bdtime = 0;
                    break;
                }
            }

            return 10.0 * Math.Pow(0.1, bdtime);
        }
    }
}
