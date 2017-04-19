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
			if (p != null)
				parent = p;
			else
				parent = null;
		}
		public State(string b, double s)
		{
			board = b;
			score = s;
			actions = null;
			parent = null;
		}
		public void populateActions(char turn) //go through string and for each empty space, create a board where that space is filled and add it to actions
		{
			actions = new List<State>();
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
		public void setBoard(string b)
		{
			board = b;
		}
		public void setParent(State p)
		{
			parent = p;
		}
		public void addAction(State a)
		{
			if (actions == null)
			{
				actions = new List<State>();
				actions.Add(a);
			}
			else
				actions.Add(a);
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
			return !board.Contains(".");
		}
		public void print()
		{
			for(int i=0; i<9; i++)
			{
				if (board[i] != '.')
					Console.Write("{0}", board[i]);
				else
					Console.Write(" ");
				if ((i + 1) % 3 == 0)
				{
					Console.WriteLine();
					Console.WriteLine("-----");
				}
				else
					Console.Write("|");
			}
		}
	}

	class Agent
	{
		//member
		private List<State> boards;
		private char player;
		private bool explore;
		private Random rng;
		private const double learnFactor = 0.2;
		private const double learnDecay = .03;

		//functions
		public Agent(char turn)
		{
			player = turn;
			boards = new List<State>();
			boards.Add(new State(".........", null));
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
		public void setExplore(bool e)
		{
			explore = e;
		}
		public bool getExplore()
		{
			return explore;
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
					int random = rng.Next(0, current.getActions().Count);
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
					Console.WriteLine("X Wins");
				}
				if(!xWin && !draw)
				{
					o.reward("win", current);
					x.reward("lose", current);
					Console.WriteLine("O Wins");
				}
				if(draw)
				{
					o.reward("draw", current);
					x.reward("draw", current);
					Console.WriteLine("Draw");
				}

				count++;
			}
		}
		static void saveData(Agent a)
		{
			//make a text file in the MyDocuments folder
			string filename;
			if(a.getSide() == 'X')
				filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\agentX.csv";
			else
				filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\agentO.csv";
			StreamWriter file = new StreamWriter(@filename);
			file.AutoFlush = true;

			//store board data in csv as board,score,parent
			int count = 0;
			foreach(var current in a.getBoards())
			{
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
		public struct readIn
		{
			public string board;
			public string parent;
		};
		static Agent readData(char side)
		{
			//open file
			string filename;
			if (side == 'X')
				filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\agentX.csv";
			else
				filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\agentO.csv";
			StreamReader file = new StreamReader(@filename);

			//read in data, and create a new State List from it, along with the parent of each board
			var newItem = file.ReadLine();
			List<readIn> connect = new List<readIn>();
			List<State> boards = new List<State>();
			while(newItem != null)
			{
				Console.WriteLine("newItem: {0}", newItem);
				var thing = newItem.Split(',');
				var b = thing[0];
				var s = Convert.ToDouble(thing[1]);
				var p = thing[2];
				boards.Add(new State(b,s));
				readIn temp;
				temp.board = b;
				temp.parent = p;
				connect.Add(temp);
				newItem = file.ReadLine();
			}

			Console.WriteLine("Read all data");
			//go through the connect list and connect all states to each other
			foreach(var obj in connect)
			{
				if (obj.parent != "null")
				{
					Console.WriteLine("Board: {0}", obj.board);
					Console.WriteLine("Parent: {0}", obj.parent);
					var x = boards.FindIndex(new Predicate<State>(n => obj.board == n.getBoard()));//get the index of the current board
					var y = boards.FindIndex(new Predicate<State>(n => obj.parent == n.getBoard()));//get the index of the current board's parent
					boards[x].setParent(boards[y]);//set y as x's parent
					boards[y].addAction(boards[x]);//add x to y's action list
				}
			}

			return new Agent(side, boards);
		}
		static void play(Agent agentO, Agent agentX)
		{
			//determine if the player is playing X or O, and get the opposing agent
			Agent comp;
			char agent;
			char human;
			Console.WriteLine("X or O? ");
			var input = Console.ReadLine();
			if (input == "X" | input == "x")
			{
				comp = agentO;
				agent = 'O';
				human = 'X';
			}
			else
			{
				comp = agentX;
				agent = 'X';
				human = 'O';
			}

			//while the player wants to play, continue playing games
			bool done = false;
			Random rng = new Random();
			State start = new State(".........", null);
			while (!done)
			{
				State current = start;
				var select = rng.Next(0, 2);
				bool compWin = false;
				bool draw = false;
				char turn;
				if (select == 0) //randomly select who goes first
					turn = 'X';
				else
					turn = 'O';

				//play the game
				while (!current.isFinished() && !current.isWin(turn))
				{
					if (turn == agent)
					{
						current = comp.makeMove(current);
						turn = human;
						if (current.isWin(agent))
							compWin = true;
					}
					if (turn == human)
					{
						current.print();
						Console.WriteLine("Input the number of the square you want to play in (the top-left square is 0): ");
						var index = Convert.ToInt32(Console.ReadLine());
						while (current.getBoard()[index] != '.')
						{
							Console.WriteLine("Try again: ");
							index = Convert.ToInt32(Console.ReadLine());
						}
						StringBuilder temp = new StringBuilder(current.getBoard());
						temp[index] = human;
						current.setBoard(temp.ToString());
						turn = agent;
					}
					if (current.isFinished())
						draw = true;
				}

				//output the results of the game, if its not a draw, give rewards to agents
				if (!draw)
				{
					if (compWin)
					{
						comp.reward("win", current);
						Console.WriteLine("\n");
						Console.WriteLine("You Win!!!");
					}
					else
					{
						comp.reward("lose", current);
						Console.WriteLine("\n");
						Console.WriteLine("You Lose.");
					}
				}
				else
				{
					Console.WriteLine("Draw.");
				}
				Console.WriteLine("Go again? ");
				input = Console.ReadLine();
				if (input == "n" | input == "no")
					done = false;
			}
		}

		static void Main(string[] args)
		{
			//create strings for where agent data is stored
			string filenameX = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\agentX.csv";
			string filenameO = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\agentO.csv";

			//check if the agent data exists, if so build those, otherwise build new agents
			Agent agentX;
			Agent agentO;
			if (File.Exists(@filenameX))
			{
				agentX = readData('X');
			}
			else
			{
				Console.WriteLine("Making agent X");
				agentX = new Agent('X');
			}
			if(File.Exists(filenameO))
				agentO = readData('O');
			else
				agentO = new Agent('O');

			Console.WriteLine("Play? ");
			var input = Console.ReadLine();
			if (input == "y" | input == "yes")
			{
				play(agentO, agentX);
				saveData(agentO);
				saveData(agentX);
			}
			else
			{
				Console.WriteLine("How many games to play? ");
				var count = Convert.ToInt32(Console.ReadLine());
				populateAgents(agentX, agentO, count);
				Console.WriteLine("Save Data? ");
				input = Console.ReadLine();
				if (input == "y" | input == "yes")
				{
					saveData(agentO);
					saveData(agentX);
				}
			}
			
		}
	}
}
