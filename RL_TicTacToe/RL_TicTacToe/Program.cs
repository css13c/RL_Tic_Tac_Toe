using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace RL_TicTacToe
{
	public class Win
	{
		static public string[] winningX =
			{
			"XXX......", //top row
			"X..X..X..", //left column
			"X...X...X", //diagonal from top left
			"...XXX...", //middle row
			".X..X..X.", //middle column
			"..X.X.X..", //diagonal from top right
			"......XXX", //bottom row
			"..X..X..X" //right column
		};
		static public string[] winningO =
			{
			"OOO......", //top row
			"O..O..O..", //left column
			"O...O...O", //diagonal from top left
			"...OOO...", //middle row
			".O..O..O.", //middle column
			"..O.O.O..", //diagonal from top right
			"......OOO", //bottom row
			"..O..O..O" //right column
		};
	}

	class State
	{
		//members
		private string board; //string represents the current board state
		private double score; //the RL value of this action
		private List<State> actions; //array of possible action states

		//functions
		public State(string b)
		{
			board = b;
			score = 0.5;
			actions = null;
		}
		public void populateActions(char turn) //go through string and for each empty space, create a board where that space is filled and add it to actions
		{
			for(int i=0; i<9; i++)
			{
				if(board[i] == '.')
				{
					StringBuilder temp = new StringBuilder(board);
					temp[i] = turn;
					actions.Add( new State(temp.ToString()) );
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
			if (turn == 'X')
			{
				foreach(var obj in Win.winningX)
				{
					if (board == obj)
						return true;
				}
				return false;
			}
			else
			{
				foreach (var obj in Win.winningO)
				{
					if (board == obj)
						return true;
				}
				return false;
			}

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
		private const double learnDecay = 0.2;

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
		public void reward(bool win)
		{
			if(win)
		}
	}

	class Program
	{
		static void populateAgents(Agent x, Agent o, int gameCount)
		{
			int count = 0;
			State start = new State(".........");
			while(count < gameCount)
			{
				State current = start;
				char turn = 'X';
				while(!current.isFinished() && !current.isWin(turn))
				{

				}
			}
		}
		static void saveData(Agent x, Agent o)
		{

		}

		static void Main(string[] args)
		{
		}
	}
}
