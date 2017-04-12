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
		private State[] actions; //array of possible action states

		//functions
		public State(string b)
		{
			board = b;
			score = 0.5;
			actions = null;
		}
		public void populateActions(char turn) //go through string and for each empty space, create a board where that space is filled and add it to actions
		{
			int count = 0;
			for(int i=0; i<9; i++)
			{
				if(board[i] == '.')
				{
					StringBuilder temp = new StringBuilder(board);
					temp[i] = turn;
					actions[count] = new State(temp.ToString());
					count++;
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
		public State[] getActions()
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

	class Program
	{
		static void Main(string[] args)
		{
		}
	}
}
