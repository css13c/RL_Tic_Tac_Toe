using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;



namespace RL_TicTacToe
{
	public class Win
	{
		static public int[][] winning =
		{
			new int[] {0,1,2},
			new int[] {3,4,5}, 
			new int[] {6,7,8},
			new int[] {0,3,6},
			new int[] {1,5,7},
			new int[] {2,6,8},
			new int[] {0,4,8},
			new int[] {2,4,6}
		};
		static public int range = 3;
	}

	class State
	{
		//members
		private string board; //string represents the current board state
		private double score; //the RL value of this action
		private List<State> actions; //array of possible action states
		private State parent;

		//functions
		public State(string b, State p)
		{
			board = b;
			score = 0.5;
			actions = null;
			parent = p;
		}
		public void populateActions(char turn) //go through string and for each empty space, create a board where that space is filled and add it to actions
		{
			for(int i=0; i<9; i++)
			{
				if(board[i] == '.')
				{
					StringBuilder temp = new StringBuilder(board);
					temp[i] = turn;
					actions.Add( new State(temp.ToString(), this) );
				}
			}
		}
		public double getScore()
		{
			return score;
		}
		public string getBoard()
		{
			return board;
		}
		public State getParent()
		{
			return parent;
		}
		public List<State> getActions()
		{
			return actions;
		}
		public void setScore(double s)
		{
			score = s;
		}
		public bool isWin(char turn)
		{
			int[] moves = new int[9];
			int count = 0;
			for(int i=0; i<9; i++)
			{
				if(board[i] == turn)
				{
					moves[count] = i;
				}
			}
			foreach(var obj in Win.winning)
			{
				bool win = true;
				for(int i=0; i<3; i++)
				{
					if (!moves.Contains(obj[i]))
						win = false;
				}
				if (win)
					return true;
			}
			return false;
		}
		public bool isFinished()
		{
			return board.Contains(".");
		}
	}

	class Agent
	{
		//member
		private List<State> boards;
		private char player;
		private bool explore
		{
			get { return explore; }
			set { value = explore; }
		}
		private Random rng;
		private const double learnFactor = 0.2;
		private const double learnDecay = .03;

		//functions
		public Agent(char turn)
		{
			player = turn;
			boards = new List<State>();
			boards.Add(new State("........."));
			rng = new Random();
			explore = true;
		}
		public Agent(char turn, List<State> array)
		{
			player = turn;
			boards = array;
			explore = false;
			rng = new Random();
		}
		public char getSide()
		{
			return player;
		}
		public List<State> getBoards()
		{
			return boards;
		}

		public State makeMove(State prev)
		{
			if (!boards.Contains(prev)) //if the list doesn't have that board, add it
			{
				boards.Add(prev);
			}

			//decide what the next move should be
			State current = boards.Find(new Predicate<State>(n => prev == n));
			State next;
			if(current.getActions() != null) //if the board doesn't have the possible moves, make them then decide
			{
				current.getActions().Sort((x, y) => y.getScore().CompareTo(x.getScore()));
				if (explore)
				{
					int random = rng.Next(1, current.getActions().Count);
					next = current.getActions()[random];
				}
				else
					next = current.getActions()[0];
			}
			else
			{
				current.populateActions(player);
				int random = rng.Next(current.getActions().Count);
				next = current.getActions()[random];
			}

			return next;
		}
		public void reward(string win, State final)
		{
			if(win == "win")
			{
				final.setScore(1);
				double value = learnFactor;
				State current = final.getParent();
				while(current != null)
				{
					current.setScore(current.getScore() + value);
					value -= learnDecay;
					current = current.getParent();
				}
			}
			else if(win == "draw")
			{
				return;
			}
			else
			{
				final.setScore(-1);
				double value = -learnFactor;
				State current = final.getParent();
				while(current != null)
				{
					current.setScore(current.getScore() - value);
					value += learnDecay;
					current = current.getParent();
				}
			}
		}

	}

	class Program
	{
		static void populateAgents(Agent x, Agent o, int gameCount)
		{
			Random rng = new Random();
			int count = 0;
			State start = new State(".........", null);
			while(count < gameCount)
			{
				//set up game at each loop iteration
				State current = start;
				var select = rng.Next(0, 2);
				bool xWin = false;
				bool draw = false;
				char turn;
				if (select == 0) //randomly select who goes first
					turn = 'X';
				else
					turn = 'O';

				//play the game
				while(!current.isFinished() && !current.isWin(turn))
				{
					if(turn == 'X')
					{
						current = x.makeMove(current);
						turn = 'O';
						if (current.isWin(turn))
						{
							xWin = true;
						}
					}
					else
					{
						current = o.makeMove(current);
						turn = 'X';
						if(current.isWin(turn))
						{
							xWin = false;
						}
					}
					if (current.isFinished())
						draw = true;
				}

				//once game is over, have both agents give rewards
				if(xWin && !draw)
				{
					x.reward("win", current);
					o.reward("lose", current);
				}
				if(!xWin && !draw)
				{
					o.reward("win", current);
					x.reward("lose", current);
				}
				if(draw)
				{
					o.reward("draw", current);
					x.reward("draw", current);
				}
			}
		}
		static void saveData(Agent a)
		{
			//make a text file in the MyDocuments folder
			string filename;
			if(a.getSide() == 'X')
				filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\agentX.csv";
			else
				filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\agentO.csv";
			StreamWriter file = new StreamWriter(@filename);
			file.AutoFlush = true;

			//store board data in csv as board,score,parent
			int count = 0;
			while(count < a.getBoards().Count)
			{
				State current = a.getBoards()[count];
				file.Write(current.getBoard().ToString());
				file.Write(",");
				file.Write(current.getScore().ToString());
				file.Write(",");
				if (current.getParent() != null)
					file.Write(current.getParent().getBoard().ToString() + Environment.NewLine);
				else
					file.Write("null" + Environment.NewLine);

				count++;
			}
			file.WriteLine();
		}
		static Agent readData(char side)
		{
			//open file
			string filename;
			if (side == 'X')
				filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\agentX.csv";
			else
				filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\agentO.csv";
			StreamReader file = new StreamReader(@filename);

			//read in data, and create a new State List from it
			var newItem = file.ReadLine();
			while(newItem != null)
			{

			}
		}

		static void Main(string[] args)
		{
		}
	}
}
